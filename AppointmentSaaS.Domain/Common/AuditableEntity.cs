namespace AppointmentSaaS.Domain.Common;

/// <summary>
/// Abstract base class that extends <see cref="BaseEntity"/> with a full audit trail.
/// <para>
/// <c>CreatedAt</c>/<c>CreatedBy</c> are populated on first save;
/// <c>UpdatedAt</c>/<c>UpdatedBy</c> are refreshed on every subsequent mutation.
/// Both are intended to be set by an EF Core <c>SaveChanges</c> interceptor or
/// <see cref="Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync"/> override
/// using <see cref="ICurrentUserService"/>.
/// </para>
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public string? CreatedBy { get; set; }

    /// <inheritdoc/>
    public DateTime? UpdatedAt { get; set; }

    /// <inheritdoc/>
    public string? UpdatedBy { get; set; }
}
