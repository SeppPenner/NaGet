// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqliteContext.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The SQLite context class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Database.Sqlite;

/// <inheritdoc cref="AbstractContext{TContext}"/>
/// <summary>
/// The SQLite context class.
/// </summary>
public class SqliteContext : AbstractContext<SqliteContext>
{
    /// <summary>
    /// The SQLite error code for when a unique constraint is violated.
    /// </summary>
    private const int SqliteUniqueConstraintViolationErrorCode = 19;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqliteContext"/> class.
    /// </summary>
    /// <param name="options">The database options.</param>
    public SqliteContext(DbContextOptions<SqliteContext> options): base(options)
    {
    }

    /// <inheritdoc cref="AbstractContext{TContext}"/>
    public override bool IsUniqueConstraintViolationException(DbUpdateException exception)
    {
        return exception.InnerException is SqliteException sqliteException &&
            sqliteException.SqliteErrorCode == SqliteUniqueConstraintViolationErrorCode;
    }

    /// <inheritdoc cref="AbstractContext{TContext}"/>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Package>()
            .Property(p => p.Id)
            .HasColumnType("TEXT COLLATE NOCASE");

        builder.Entity<Package>()
            .Property(p => p.NormalizedVersionString)
            .HasColumnType("TEXT COLLATE NOCASE");

        builder.Entity<PackageDependency>()
            .Property(d => d.Id)
            .HasColumnType("TEXT COLLATE NOCASE");

        builder.Entity<PackageType>()
            .Property(t => t.Name)
            .HasColumnType("TEXT COLLATE NOCASE");

        builder.Entity<TargetFramework>()
            .Property(f => f.Moniker)
            .HasColumnType("TEXT COLLATE NOCASE");
    }
}
