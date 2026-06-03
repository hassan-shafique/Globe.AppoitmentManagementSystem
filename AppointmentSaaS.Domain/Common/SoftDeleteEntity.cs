namespace AppointmentSaaS.Domain.Common;

/// <summary>
/// Abstract base class for entities that are never physically deleted from the database.
/// <para>
/// Instead of issuing a <c>DELETE</c> statement, callers invoke <see cref="SoftDelete"/>
/// which sets <see cref="IsDeleted"/> to <c>true</c> and captures the deletion timestamp
/// and actor. An EF Core global query filter (<c>e => !e.IsDeleted</c>) should be applied
/// in <c>OnModelCreating</c> so that deleted rows are invisible to normal queries.
/// </para>
/// </summary>
public abstract class SoftDeleteEntity : AuditableEntity
{
    /// <summary>
    /// <c>true</c> when the entity has been logically deleted; <c>false</c> otherwise.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>UTC timestamp at which the entity was soft-deleted, or <c>null</c> if still active.</summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>Identity (user ID or name) of the actor who deleted the entity, or <c>null</c>.</summary>
    public string? DeletedBy { get; private set; }

    /// <summary>
    /// Marks the entity as logically deleted. Idempotent — calling on an already-deleted
    /// entity is a no-op.
    /// </summary>
    /// <param name="deletedBy">Optional identity of the actor performing the deletion.</param>
    public void SoftDelete(string? deletedBy = null)
    {
        if (IsDeleted) return;
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    /// <summary>
    /// Restores a previously soft-deleted entity, clearing all deletion metadata.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
