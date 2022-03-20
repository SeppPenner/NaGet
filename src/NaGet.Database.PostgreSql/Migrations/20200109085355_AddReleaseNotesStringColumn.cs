// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddReleaseNotesStringColumn.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The migration to add the release notes string.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Database.PostgreSql.Migrations;

/// <inheritdoc cref="Migration"/>
/// <summary>
/// The migration to add the release notes string.
/// </summary>
public partial class AddReleaseNotesStringColumn : Migration
{
    /// <inheritdoc cref="Migration"/>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ReleaseNotes",
            table: "Packages",
            maxLength: 4000,
            nullable: true);
    }

    /// <inheritdoc cref="Migration"/>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ReleaseNotes",
            table: "Packages");
    }
}
