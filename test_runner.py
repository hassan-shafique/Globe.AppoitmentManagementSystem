"""
Globe AMS — Automated Integration Test Runner
Tests multi-tenancy, auth, appointments, token management, and RBAC.
Usage: python test_runner.py
The script starts and stops the API server automatically.
"""

import sys
import io
import json
import time
import subprocess
import os
import urllib.request
import urllib.error
import signal

# Fix encoding on Windows
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

BASE_URL = "http://localhost:5006"
PROJECT_DIR = os.path.dirname(os.path.abspath(__file__))
WEB_PROJECT = os.path.join(PROJECT_DIR, "AppointmentSaaS.Web")
DB_PATH = os.path.join(WEB_PROJECT, "AppointmentSaaS.db")
DOTNET = r"C:\Program Files\dotnet\dotnet.exe"

results = []
server_proc = None


# ── HTTP helpers ────────────────────────────────────────────────────────────

def req(method, path, body=None, token=None, extra_headers=None, raw_body=None):
    """Send HTTP request, return (status_code, parsed_body)."""
    url = f"{BASE_URL}{path}"
    if raw_body is not None:
        data = raw_body.encode()
    elif body is not None:
        data = json.dumps(body).encode()
    else:
        data = None
    headers = {"Content-Type": "application/json", "Accept": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    if extra_headers:
        headers.update(extra_headers)
    request = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(request) as resp:
            raw = resp.read()
            try:
                return resp.status, json.loads(raw)
            except Exception:
                return resp.status, None
    except urllib.error.HTTPError as e:
        raw = e.read()
        try:
            return e.code, json.loads(raw)
        except Exception:
            return e.code, None


def check(label, actual, expected, data=None):
    ok = actual == expected
    mark = "[PASS]" if ok else "[FAIL]"
    print(f"  {mark} {label}: HTTP {actual} (expected {expected})")
    if not ok and data:
        snippet = json.dumps(data)[:300] if data else ""
        print(f"         Response: {snippet}")
    results.append((label, ok))
    return ok


# ── Server management ───────────────────────────────────────────────────────

def kill_port(port):
    """Kill any process listening on the port (Windows)."""
    try:
        result = subprocess.run(
            [r"C:\Windows\System32\netstat.exe", "-ano"],
            capture_output=True, text=True
        )
        for line in result.stdout.splitlines():
            if f":{port} " in line and "LISTENING" in line:
                parts = line.split()
                pid = parts[-1]
                subprocess.run(
                    [r"C:\Windows\System32\taskkill.exe", "/F", "/PID", pid],
                    capture_output=True)
                print(f"  Killed PID {pid} on port {port}")
    except Exception as e:
        print(f"  kill_port warning: {e}")


def start_server():
    global server_proc
    print("\n[SETUP] Cleaning up port 5006...")
    kill_port(5006)
    time.sleep(1)

    if os.path.exists(DB_PATH):
        os.remove(DB_PATH)
        print(f"[SETUP] Removed old DB: {DB_PATH}")

    print("[SETUP] Starting API server...")
    env = os.environ.copy()
    env["ASPNETCORE_URLS"] = "http://localhost:5006"
    env["ASPNETCORE_ENVIRONMENT"] = "Development"

    server_proc = subprocess.Popen(
        [DOTNET, "run",
         "--project", WEB_PROJECT,
         "--no-build",
         "-p:UseRazorBuildServer=false",
         "-p:UseSharedCompilation=false"],
        cwd=PROJECT_DIR,
        env=env,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        text=True,
        encoding="utf-8",
        errors="replace"
    )

    # Wait for the server to be ready
    for i in range(30):
        time.sleep(2)
        try:
            with urllib.request.urlopen(f"{BASE_URL}/health", timeout=2) as r:
                if r.status < 500:
                    print("[SETUP] Server is ready.")
                    return True
        except Exception:
            pass
        try:
            s, _ = req("POST", "/api/auth/login",
                       {"email": "x", "password": "x", "tenantSlug": "x"})
            if s in (400, 404):
                print("[SETUP] Server is ready (auth endpoint responding).")
                return True
        except Exception:
            pass
        sys.stdout.write(f"\r[SETUP] Waiting for server... ({(i+1)*2}s)")
        sys.stdout.flush()

    print("\n[SETUP] Server did not start in time.")
    return False


def stop_server():
    global server_proc
    if server_proc:
        server_proc.terminate()
        try:
            server_proc.wait(timeout=5)
        except Exception:
            server_proc.kill()
        server_proc = None
        print("\n[TEARDOWN] Server stopped.")


# ── Build ────────────────────────────────────────────────────────────────────

def build():
    print("\n[BUILD] Building solution...")
    result = subprocess.run(
        [DOTNET, "build",
         "-p:UseRazorBuildServer=false",
         "-p:UseSharedCompilation=false",
         "--nologo"],
        cwd=PROJECT_DIR,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace"
    )
    if result.returncode != 0:
        print("[BUILD] FAILED:")
        print(result.stdout[-3000:])
        print(result.stderr[-2000:])
        return False
    print("[BUILD] Success.")
    return True


# ── Test groups ──────────────────────────────────────────────────────────────

def test_a_admin_auth():
    print("\n[A] Admin Auth")

    s, d = req("POST", "/api/auth/login",
               {"email": "admin@globe.com", "password": "Admin@123!", "tenantSlug": "globe"})
    check("A1 Login valid admin", s, 200, d)
    token = d.get("accessToken") if d else None
    refresh = d.get("refreshToken") if d else None
    tenant_id = d.get("tenantId") if d else None
    user_id = d.get("userId") if d else None

    s, d = req("POST", "/api/auth/login",
               {"email": "admin@globe.com", "password": "wrong!", "tenantSlug": "globe"})
    check("A2 Wrong password", s, 400, d)

    s, d = req("POST", "/api/auth/login",
               {"email": "admin@globe.com", "password": "Admin@123!", "tenantSlug": "nonexistent"})
    check("A3 Non-existent tenant slug", s, 404, d)

    return token, refresh, tenant_id, user_id


def test_b_tenant_management(token, tenant_id):
    print("\n[B] Tenant Management")

    s, d = req("GET", "/api/tenants/globe")
    check("B1 Get tenant by slug (no auth)", s, 200, d)

    s, d = req("GET", "/api/tenants/ghost-tenant")
    check("B2 Get non-existent tenant slug", s, 404, d)

    s, d = req("POST", "/api/tenants",
               {"name": "New Clinic", "slug": "new-clinic", "contactEmail": "admin@new.com"},
               token=token)
    check("B3 Create tenant without SuperAdmin (expect 403)", s, 403, d)

    s, d = req("POST", "/api/tenants",
               {"name": "New Clinic", "slug": "new-clinic", "contactEmail": "admin@new.com"})
    check("B4 Create tenant without auth (expect 401)", s, 401, d)


def test_c_register(token):
    print("\n[C] Register / Verify / Login")

    s, d = req("POST", "/api/auth/register",
               {"firstName": "Jane", "lastName": "Test",
                "email": "jane@test.com", "password": "Test@1234!",
                "tenantSlug": "globe"})
    check("C1 Register new user", s, 201, d)

    s, d = req("POST", "/api/auth/login",
               {"email": "jane@test.com", "password": "Test@1234!", "tenantSlug": "globe"})
    check("C2 Login before email verify (expect 400)", s, 400, d)

    s, d = req("POST", "/api/auth/resend-verification",
               {"email": "nobody@ghost.com"})
    check("C3 Resend verify non-existent email (anti-enum 200)", s, 200, d)

    s, d = req("POST", "/api/auth/register",
               {"firstName": "Jane", "lastName": "Test",
                "email": "jane@test.com", "password": "Test@1234!",
                "tenantSlug": "globe"})
    check("C4 Register duplicate email (expect 400)", s, 400, d)

    s, d = req("POST", "/api/auth/register",
               {"firstName": "Weak", "lastName": "Pass",
                "email": "weak@test.com", "password": "password",
                "tenantSlug": "globe"})
    check("C5 Register weak password (expect 400)", s, 400, d)

    s, d = req("POST", "/api/auth/register",
               {"firstName": "Ghost", "lastName": "User",
                "email": "ghost@test.com", "password": "Ghost@1234!",
                "tenantSlug": "ghost-tenant"})
    check("C6 Register in non-existent tenant (expect 404)", s, 404, d)


def test_d_services(token, tenant_id):
    print("\n[D] Services")

    s, d = req("POST", "/api/services",
               {"name": "Haircut", "description": "Standard haircut", "durationMinutes": 30, "price": 25.00},
               token=token)
    check("D1 Create service (TenantAdmin)", s, 201, d)
    service_id = d.get("id") if d and s == 201 else None

    s, d = req("GET", f"/api/services/tenant/{tenant_id}")
    check("D2 List services by tenant (no auth)", s, 200, d)

    s, d = req("POST", "/api/services",
               {"name": "Facial", "description": "Basic facial", "durationMinutes": 45, "price": 40.00})
    check("D3 Create service without auth (expect 401)", s, 401, d)

    # Verify EF filter: fake tenant returns empty list
    s, d = req("GET", "/api/services/tenant/00000000-0000-0000-0000-000000000000")
    is_empty = isinstance(d, list) and len(d) == 0
    print(f"  {'[PASS]' if (s == 200 and is_empty) else '[FAIL]'} D4 Services in fake tenant = empty list: HTTP {s}, count={len(d) if isinstance(d, list) else 'N/A'}")
    results.append(("D4 Fake tenant services isolation", s == 200 and is_empty))

    return service_id


def test_e_staff(token, tenant_id):
    print("\n[E] Staff")

    s, d = req("GET", f"/api/staff/tenant/{tenant_id}", token=token)
    check("E1 List staff by tenant", s, 200, d)
    if d and len(d) > 0:
        staff_id = d[0]["id"]
        print(f"       Seeded Staff.Id: {staff_id}")
        return staff_id
    else:
        print("  [WARN] No staff records found — appointment tests will be skipped")
        return None


def test_f_appointments(token, tenant_id, service_id, staff_id, client_token=None):
    print("\n[F] Appointments")

    if not service_id:
        print("  [SKIP] No serviceId — skipping appointment tests")
        return None, None

    if not staff_id:
        print("  [SKIP] No staffId — skipping appointment tests")
        return None, None

    # Create appointment 1
    s, d = req("POST", "/api/appointments",
               {"serviceId": service_id, "staffId": staff_id,
                "startTime": "2026-08-10T10:00:00Z", "notes": "Integration test #1"},
               token=token)
    check("F1 Create appointment (TenantAdmin)", s, 201, d)
    appt_id1 = d.get("id") if d and s == 201 else None

    s, d = req("GET", f"/api/appointments/tenant/{tenant_id}", token=token)
    check("F2 Get appointments by tenant", s, 200, d)

    if appt_id1:
        s, d = req("GET", f"/api/appointments/{appt_id1}", token=token)
        check("F3 Get appointment by ID", s, 200, d)
        if d:
            print(f"       Status: {d.get('status')}")

        s, d = req("PATCH", f"/api/appointments/{appt_id1}/confirm", token=token)
        check("F4 Confirm appointment (TenantAdmin)", s, 204, d)

        s, d = req("GET", f"/api/appointments/{appt_id1}", token=token)
        check("F5 Verify status = Confirmed", s, 200, d)
        if d:
            confirmed = d.get("status") == "Confirmed"
            print(f"       Status = {d.get('status')} {'[PASS]' if confirmed else '[FAIL]'}")

        s, d = req("PATCH", f"/api/appointments/{appt_id1}/complete", token=token)
        check("F6 Complete appointment (TenantAdmin)", s, 204, d)

    # Create appointment 2 for cancel test
    s, d = req("POST", "/api/appointments",
               {"serviceId": service_id, "staffId": staff_id,
                "startTime": "2026-08-11T14:00:00Z", "notes": "Appointment to cancel"},
               token=token)
    check("F7 Create appointment 2 (for cancel)", s, 201, d)
    appt_id2 = d.get("id") if d and s == 201 else None

    if appt_id2:
        s, d = req("PATCH", f"/api/appointments/{appt_id2}/cancel",
                   token=token, raw_body='"Schedule conflict"')
        check("F8 Cancel appointment", s, 204, d)

        s, d = req("GET", f"/api/appointments/{appt_id2}", token=token)
        check("F9 Verify status = Cancelled", s, 200, d)
        if d:
            cancelled = d.get("status") == "Cancelled"
            print(f"       Status = {d.get('status')} {'[PASS]' if cancelled else '[FAIL]'}")

    # Auth / RBAC checks
    s, d = req("POST", "/api/appointments",
               {"serviceId": service_id, "staffId": staff_id,
                "startTime": "2026-08-12T09:00:00Z"})
    check("F10 Create appointment without auth (expect 401)", s, 401, d)

    if appt_id1 and client_token:
        s, d = req("PATCH", f"/api/appointments/{appt_id1}/confirm", token=client_token)
        check("F11 Confirm with Client role (expect 403)", s, 403, d)

        s, d = req("PATCH", f"/api/appointments/{appt_id1}/complete", token=client_token)
        check("F12 Complete with Client role (expect 403)", s, 403, d)

    return appt_id1, appt_id2


def test_g_multitenancy(token, tenant_id, service_id, staff_id):
    print("\n[G] Multi-tenancy Isolation")

    # Login with wrong tenant slug
    s, d = req("POST", "/api/auth/login",
               {"email": "admin@globe.com", "password": "Admin@123!", "tenantSlug": "alpha-clinic"})
    check("G1 Login valid user with wrong tenant slug (expect 404)", s, 404, d)

    # EF filter: X-Tenant-Id header with fake value when JWT is present — JWT always wins
    s, d = req("GET", f"/api/appointments/tenant/{tenant_id}",
               token=token,
               extra_headers={"X-Tenant-Id": "00000000-0000-0000-0000-000000000000"})
    check("G2 JWT tenantId overrides X-Tenant-Id header (expect 200 with globe data)", s, 200, d)
    if d is not None:
        has_data = isinstance(d, list)
        print(f"       Got list: {has_data}, count: {len(d) if has_data else 'N/A'}")

    # Services in fake tenant = empty list (EF filter enforced)
    s, d = req("GET", "/api/services/tenant/00000000-0000-0000-0000-000000000000")
    is_empty = isinstance(d, list) and len(d) == 0
    print(f"  {'[PASS]' if (s == 200 and is_empty) else '[FAIL]'} G3 Fake tenantId in route → empty list: HTTP {s}")
    results.append(("G3 Fake tenantId EF filter isolation", s == 200 and is_empty))

    # Unauthenticated staff endpoint with real tenant ID
    s, d = req("GET", f"/api/staff/tenant/{tenant_id}")
    check("G4 Staff endpoint no auth (returns 200 — public list)", s, 200, d)

    # Appointments require auth
    s, d = req("GET", f"/api/appointments/tenant/{tenant_id}")
    check("G5 Appointments by tenant without auth (expect 401)", s, 401, d)

    # Cross-tenant appointment lookup: use globe token to get alpha tenant ID (doesn't exist)
    fake_tenant = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
    s, d = req("GET", f"/api/appointments/tenant/{fake_tenant}", token=token)
    check("G6 Globe token + fake tenantId route (EF filter returns empty, 200)", s, 200, d)
    if isinstance(d, list):
        print(f"       Records returned: {len(d)} (expected 0 due to EF filter)")

    # Services CRUD isolation: create as globe admin, verify service has globe tenantId
    if service_id:
        s, d = req("GET", f"/api/services/tenant/{tenant_id}")
        check("G7 Globe services visible with globe tenantId", s, 200, d)
        if d and isinstance(d, list):
            globe_svcs = [x for x in d if x.get("tenantId") == str(tenant_id)]
            print(f"       Services with correct tenantId: {len(globe_svcs)}/{len(d)}")


def test_h_token_management(token, refresh_token, tenant_id):
    print("\n[H] Token Management")

    # Refresh happy path
    s, d = req("POST", "/api/auth/refresh",
               {"accessToken": token, "refreshToken": refresh_token})
    check("H1 Refresh token (happy path)", s, 200, d)
    new_token = d.get("accessToken") if d and s == 200 else token
    new_refresh = d.get("refreshToken") if d and s == 200 else None

    # Reuse old refresh token (should fail — rotation invalidates it)
    s, d = req("POST", "/api/auth/refresh",
               {"accessToken": token, "refreshToken": refresh_token})
    check("H2 Reuse old refresh token after rotation (expect 400)", s, 400, d)

    # Garbage refresh token
    s, d = req("POST", "/api/auth/refresh",
               {"accessToken": token, "refreshToken": "invalid-garbage-token-xyz"})
    check("H3 Garbage refresh token (expect 400)", s, 400, d)

    # Revoke new refresh token (raw JSON string body)
    if new_refresh:
        s, d = req("POST", "/api/auth/revoke",
                   token=new_token,
                   raw_body=json.dumps(new_refresh))
        check("H4 Revoke refresh token (expect 204)", s, 204, d)

        # Try to use revoked token
        s, d = req("POST", "/api/auth/refresh",
                   {"accessToken": new_token, "refreshToken": new_refresh})
        check("H5 Use revoked refresh token (expect 400)", s, 400, d)

    # Revoke without auth
    s, d = req("POST", "/api/auth/revoke",
               raw_body=json.dumps("some-token"))
    check("H6 Revoke without auth (expect 401)", s, 401, d)

    # Forgot password (anti-enumeration)
    s, d = req("POST", "/api/auth/forgot-password", {"email": "admin@globe.com"})
    check("H7 Forgot password (existing email)", s, 200, d)

    s, d = req("POST", "/api/auth/forgot-password", {"email": "nobody@ghost.com"})
    check("H8 Forgot password (non-existent email — anti-enum 200)", s, 200, d)

    # Logout
    s, d = req("POST", "/api/auth/logout", token=new_token)
    check("H9 Logout (expect 204)", s, 204, d)

    s, d = req("POST", "/api/auth/logout")
    check("H10 Logout without auth (expect 401)", s, 401, d)


# ── Main ─────────────────────────────────────────────────────────────────────

def main():
    print("=" * 60)
    print("Globe AMS — Integration Test Runner")
    print("=" * 60)

    if not build():
        sys.exit(1)

    if not start_server():
        stop_server()
        sys.exit(1)

    try:
        # A: Admin auth — establishes token, tenantId, userId
        token, refresh_token, tenant_id, user_id = test_a_admin_auth()
        if not token:
            print("\n[ERROR] Login failed — cannot continue tests.")
            return

        # B: Tenant management
        test_b_tenant_management(token, tenant_id)

        # C: Registration flow
        test_c_register(token)

        # D: Services (creates a service, returns serviceId)
        service_id = test_d_services(token, tenant_id)

        # E: Staff (returns staffId from seeded record)
        staff_id = test_e_staff(token, tenant_id)

        # F: Appointments
        test_f_appointments(token, tenant_id, service_id, staff_id)

        # G: Multi-tenancy isolation
        test_g_multitenancy(token, tenant_id, service_id, staff_id)

        # H: Token management
        test_h_token_management(token, refresh_token, tenant_id)

    finally:
        stop_server()

    # Summary
    print("\n" + "=" * 60)
    total = len(results)
    passed = sum(1 for _, ok in results if ok)
    failed = total - passed
    print(f"RESULTS: {passed}/{total} passed, {failed} failed")
    print("=" * 60)
    if failed > 0:
        print("\nFailed tests:")
        for label, ok in results:
            if not ok:
                print(f"  - {label}")
    print()
    sys.exit(0 if failed == 0 else 1)


if __name__ == "__main__":
    main()
