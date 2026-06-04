# Clean build artifacts (bin and obj folders)
# Usage: .\clean-build-artifacts.ps1

param(
    [string]$Path = (Get-Location),
    [switch]$Verbose
)

Write-Host "Cleaning build artifacts from: $Path"

$binDirs = Get-ChildItem -Path $Path -Directory -Recurse |
           Where-Object { $_.Name -eq "bin" }

$objDirs = Get-ChildItem -Path $Path -Directory -Recurse |
           Where-Object { $_.Name -eq "obj" }

$allDirs = @($binDirs) + @($objDirs)

if ($allDirs.Count -eq 0) {
    Write-Host "No bin or obj folders found."
    exit 0
}

Write-Host "Found $($allDirs.Count) folders to remove:" 

foreach ($dir in $allDirs) {
    $fullPath = $dir.FullName

    if (Test-Path $fullPath) {
        Write-Host "Removing: $fullPath"

        Remove-Item -Path $fullPath -Recurse -Force -ErrorAction SilentlyContinue

        if ($?) {
            Write-Host "  ✓ Removed"         }
        else {
            Write-Host "  ✗ Failed to remove"
        }
    }
}

Write-Host "Cleanup complete"