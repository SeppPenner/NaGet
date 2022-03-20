// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SqlServerContext.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The SQL server context class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Database.SqlServer;

/// <inheritdoc cref="AbstractContext{TContext}"/>
/// <summary>
/// The SQL server context class.
/// </summary>
public class SqlServerContext : AbstractContext<SqlServerContext>
{
    /// <summary>
    /// The SQL server error code for when a unique contraint is violated.
    /// </summary>
    private const int UniqueConstraintViolationErrorCode = 2627;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerContext"/> class.
    /// </summary>
    /// <param name="options">The database options.</param>
    public SqlServerContext(DbContextOptions<SqlServerContext> options): base(options)
    {
    }

    /// <inheritdoc cref="AbstractContext{TContext}"/>
    public override bool IsUniqueConstraintViolationException(DbUpdateException exception)
    {
        if (exception.GetBaseException() is SqlException sqlException)
        {
            return sqlException.Errors
                .OfType<SqlError>()
                .Any(error => error.Number == UniqueConstraintViolationErrorCode);
        }

        return false;
    }
}
