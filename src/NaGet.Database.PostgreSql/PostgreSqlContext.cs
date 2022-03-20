// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MySqlContext.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The PostgreSQL context class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Database.PostgreSql;

/// <inheritdoc cref="AbstractContext{TContext}"/>
/// <summary>
/// The PostgreSQL context class.
/// </summary>
public class PostgreSqlContext : AbstractContext<PostgreSqlContext>
{
    /// <summary>
    /// The PostgreSQL error code for when a unique constraint is violated.
    /// See: https://www.postgresql.org/docs/9.6/errcodes-appendix.html.
    /// </summary>
    private const int UniqueConstraintViolationErrorCode = 23505;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlContext"/> class.
    /// </summary>
    /// <param name="options">The database options.</param>
    public PostgreSqlContext(DbContextOptions<PostgreSqlContext> options): base(options)
    {
    }

    /// <inheritdoc cref="AbstractContext{TContext}"/>
    public override bool IsUniqueConstraintViolationException(DbUpdateException exception)
    {
        return exception.InnerException is PostgresException postgresException &&
               int.TryParse(postgresException.SqlState, out var code) &&
               code == UniqueConstraintViolationErrorCode;
    }

    /// <inheritdoc cref="AbstractContext{TContext}"/>
    public override async Task RunMigrationsAsync(CancellationToken cancellationToken)
    {
        await base.RunMigrationsAsync(cancellationToken);

        // Npgsql caches the database's type information on the initial connection.
        // This causes issues when NaGet creates the database as it may add the citext
        // extension to support case insensitive columns.
        // See: https://github.com/loic-sharma/BaGet/issues/442
        // See: https://github.com/npgsql/efcore.pg/issues/170#issuecomment-303417225
        if (Database.GetDbConnection() is NpgsqlConnection connection)
        {
            await connection.OpenAsync(cancellationToken);
            connection.ReloadTypes();
        }
    }

    /// <inheritdoc cref="AbstractContext{TContext}"/>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasPostgresExtension("citext");

        builder.Entity<Package>()
            .Property(p => p.Id)
            .HasColumnType("citext");

        builder.Entity<Package>()
            .Property(p => p.NormalizedVersionString)
            .HasColumnType("citext");

        builder.Entity<PackageDependency>()
            .Property(p => p.Id)
            .HasColumnType("citext");

        builder.Entity<PackageType>()
            .Property(p => p.Name)
            .HasColumnType("citext");

        builder.Entity<TargetFramework>()
            .Property(p => p.Moniker)
            .HasColumnType("citext");
    }
}
