// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RemoveReleaseNotesMaxLength.cs" company="Hämmer Electronics">
// The project is licensed under the MIT license.
// </copyright>
// <summary>
//    The migration to remove the release notes maximum length.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NaGet.Database.SqlServer.Migrations;

/// <inheritdoc cref="Migration"/>
/// <summary>
/// The migration to remove the release notes maximum length.
/// </summary>
public partial class RemoveReleaseNotesMaxLength : Migration
{
    /// <inheritdoc cref="Migration"/>
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "ReleaseNotes",
            table: "Packages",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(4000)",
            oldMaxLength: 4000,
            oldNullable: true);
    }

    /// <inheritdoc cref="Migration"/>
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "ReleaseNotes",
            table: "Packages",
            type: "nvarchar(4000)",
            maxLength: 4000,
            nullable: true,
            oldClrType: typeof(string),
            oldNullable: true);
    }
}
