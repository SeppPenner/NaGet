// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FixVersionCaseSensitivity.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The migration to fix the version case sensitivity.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Database.Sqlite.Migrations;

/// <inheritdoc cref="Migration"/>
/// <summary>
/// The migration to fix the version case sensitivity.
/// </summary>
public partial class FixVersionCaseSensitivity : Migration
{
    /// <inheritdoc cref="Migration"/>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Disable this migration as SQLite does not support altering columns.
        // Customers will need to create a new database and reupload their packages.
        // See: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#migrations-limitations
        //migrationBuilder.AlterColumn<string>(
        //    name: "Version",
        //    table: "Packages",
        //    type: "TEXT COLLATE NOCASE",
        //    maxLength: 64,
        //    nullable: false,
        //    oldClrType: typeof(string),
        //    oldMaxLength: 64);
    }

    /// <inheritdoc cref="Migration"/>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        //migrationBuilder.AlterColumn<string>(
        //    name: "Version",
        //    table: "Packages",
        //    maxLength: 64,
        //    nullable: false,
        //    oldClrType: typeof(string),
        //    oldType: "TEXT COLLATE NOCASE",
        //    oldMaxLength: 64);
    }
}
