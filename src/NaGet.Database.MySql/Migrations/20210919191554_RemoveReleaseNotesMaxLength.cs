// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveReleaseNotesMaxLength.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The migration to remove the release notes maximum length.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Database.MySql.Migrations;

/// <inheritdoc cref="Migration"/>
/// <summary>
/// The migration to remove the release notes maximum length.
/// </summary>
public partial class RemoveReleaseNotesMaxLength : Migration
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
            oldType: "timestamp(6)",
            oldNullable: true)
            .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
    }

    /// <inheritdoc cref="Migration"/>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "RowVersion",
            table: "Packages",
            type: "timestamp(6)",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldRowVersion: true,
            oldNullable: true)
            .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn);
    }
}
