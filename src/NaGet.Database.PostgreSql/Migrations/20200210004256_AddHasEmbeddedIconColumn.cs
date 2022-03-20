// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddHasEmbeddedIconColumn.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The migration to add the has embedded icon column.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Database.PostgreSql.Migrations;

/// <inheritdoc cref="Migration"/>
/// <summary>
/// The migration to add the has embedded icon column.
/// </summary>
public partial class AddHasEmbeddedIconColumn : Migration
{
    /// <inheritdoc cref="Migration"/>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "HasEmbeddedIcon",
            table: "Packages",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc cref="Migration"/>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "HasEmbeddedIcon",
            table: "Packages");
    }
}
