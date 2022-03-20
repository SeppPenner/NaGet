// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MySqlContext.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The MySQL context class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Database.MySql;

/// <inheritdoc cref="AbstractContext{TContext}"/>
/// <summary>
/// The MySQL context class.
/// </summary>
public class MySqlContext : AbstractContext<MySqlContext>
{
    /// <summary>
    /// The MySQL Server error code for when a unique constraint is violated.
    /// </summary>
    private const int UniqueConstraintViolationErrorCode = 1062;

    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlContext"/> class.
    /// </summary>
    /// <param name="options">The database options.</param>
    public MySqlContext(DbContextOptions<MySqlContext> options) : base(options)
    {
    }

    /// <inheritdoc cref="AbstractContext{TContext}"/>
    public override bool IsUniqueConstraintViolationException(DbUpdateException exception)
    {
        return exception.InnerException is MySqlException mysqlException &&
               mysqlException.Number == UniqueConstraintViolationErrorCode;
    }

    /// <inheritdoc cref="AbstractContext{TContext}"/>
    /// <summary>
    /// MySQL does not support LIMIT clauses in subqueries for certain subquery operators.
    /// See: https://dev.mysql.com/doc/refman/8.0/en/subquery-restrictions.html.
    /// </summary>
    public override bool SupportsLimitInSubqueries => false;
}
