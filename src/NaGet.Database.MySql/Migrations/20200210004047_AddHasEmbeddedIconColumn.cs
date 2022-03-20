// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddHasEmbeddedIconColumn.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The migration to add the has embedded icon column.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Database.MySql.Migrations;

/// <inheritdoc cref="Migration"/>
/// <summary>
/// The migration to add the has embedded icon column.
/// </summary>
public partial class AddHasEmbeddedIconColumn : Migration
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
            oldNullable: true)
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);

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

        migrationBuilder.AlterColumn<DateTime>(
            name: "RowVersion",
            table: "Packages",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldRowVersion: true,
            oldNullable: true)
            .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
    }
}
