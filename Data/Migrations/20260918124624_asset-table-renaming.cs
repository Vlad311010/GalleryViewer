using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class assettablerenaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asset_AssetGroups_GroupId",
                table: "Asset");

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Galleries_GalleryId",
                table: "Asset");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetTags_Asset_AssetId",
                table: "AssetTags");

            migrationBuilder.DropForeignKey(
                name: "FK_Galleries_Asset_CoverSourceId",
                table: "Galleries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Asset",
                table: "Asset");

            migrationBuilder.RenameTable(
                name: "Asset",
                newName: "Assets");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Assets",
                table: "Assets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_AssetGroups_GroupId",
                table: "Assets",
                column: "GroupId",
                principalTable: "AssetGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Galleries_GalleryId",
                table: "Assets",
                column: "GalleryId",
                principalTable: "Galleries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetTags_Assets_AssetId",
                table: "AssetTags",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Galleries_Assets_CoverSourceId",
                table: "Galleries",
                column: "CoverSourceId",
                principalTable: "Assets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_AssetGroups_GroupId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Galleries_GalleryId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetTags_Assets_AssetId",
                table: "AssetTags");

            migrationBuilder.DropForeignKey(
                name: "FK_Galleries_Assets_CoverSourceId",
                table: "Galleries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Assets",
                table: "Assets");

            migrationBuilder.RenameTable(
                name: "Assets",
                newName: "Asset");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Asset",
                table: "Asset",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_AssetGroups_GroupId",
                table: "Asset",
                column: "GroupId",
                principalTable: "AssetGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Galleries_GalleryId",
                table: "Asset",
                column: "GalleryId",
                principalTable: "Galleries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetTags_Asset_AssetId",
                table: "AssetTags",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Galleries_Asset_CoverSourceId",
                table: "Galleries",
                column: "CoverSourceId",
                principalTable: "Asset",
                principalColumn: "Id");
        }
    }
}
