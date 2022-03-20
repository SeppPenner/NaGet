// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddOriginalVersionStringColumn.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The migration to add the original version string.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Database.MySql.Migrations;

/// <inheritdoc cref="Migration"/>
/// <summary>
/// The migration to add the original version string.
/// </summary>
public partial class AddOriginalVersionStringColumn : Migration
{
    /// <inheritdoc cref="Migration"/>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "RowVersion",
            table: "Packages",
            rowVersion: true,
            nullable: true,
            oldClrType: typeof(DateTime),
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "OriginalVersion",
            table: "Packages",
            maxLength: 64,
            nullable: true);
    }

    /// <inheritdoc cref="Migration"/>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "OriginalVersion",
            table: "Packages");

        migrationBuilder.AlterColumn<DateTime>(
            name: "RowVersion",
            table: "Packages",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldRowVersion: true,
            oldNullable: true);
    }
}
