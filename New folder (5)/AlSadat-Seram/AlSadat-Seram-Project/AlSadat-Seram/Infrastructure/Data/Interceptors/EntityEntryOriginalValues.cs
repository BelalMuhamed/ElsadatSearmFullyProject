using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Data.Interceptors;

/// <summary>
/// Makes an entry's original values trustworthy before the audit interceptors compare them.
/// </summary>
internal static class EntityEntryOriginalValues
{
    /// <summary>
    /// For a <see cref="EntityState.Modified"/> entry that was attached without its database values
    /// (<c>Entry(entity).State = Modified</c> on a new instance: every column is marked modified and
    /// every original value equals the current one), loads the row and copies its values into the
    /// original values. Primary keys and concurrency tokens are left alone so optimistic
    /// concurrency keeps working. Does nothing for a tracked entity whose originals are already known.
    /// </summary>
    /// <param name="entry">The entry to check.</param>
    /// <param name="useAsync">False on the synchronous <c>SaveChanges</c> path.</param>
    /// <param name="cancellationToken">Cancels the database read.</param>
    public static async Task EnsureOriginalValuesAsync(
        this EntityEntry entry, bool useAsync, CancellationToken cancellationToken)
    {
        if (entry.State != EntityState.Modified || !OriginalsAreUnknown(entry))
            return;

        var databaseValues = useAsync
            ? await entry.GetDatabaseValuesAsync(cancellationToken)
            : entry.GetDatabaseValues();

        if (databaseValues is null)
            return; // the row is gone; SaveChanges reports the concurrency failure itself

        foreach (var property in databaseValues.Properties)
        {
            if (property.IsPrimaryKey() || property.IsConcurrencyToken)
                continue;

            entry.Property(property.Name).OriginalValue = databaseValues[property];
        }
    }

    private static bool OriginalsAreUnknown(EntityEntry entry)
    {
        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey())
                continue;

            if (!property.IsModified || !Equals(property.OriginalValue, property.CurrentValue))
                return false;
        }

        return true;
    }
}
