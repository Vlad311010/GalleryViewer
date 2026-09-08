using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class initialcreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TagCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    CanonicalId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_TagCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TagCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tags_Tags_CanonicalId",
                        column: x => x.CanonicalId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Asset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RelativePath = table.Column<string>(type: "TEXT", nullable: false),
                    PreviewPath = table.Column<string>(type: "TEXT", nullable: true),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    GroupPosition = table.Column<int>(type: "INTEGER", nullable: true),
                    GalleryId = table.Column<int>(type: "INTEGER", nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", nullable: false),
                    Hash = table.Column<string>(type: "TEXT", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImportTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asset", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetTags",
                columns: table => new
                {
                    AssetId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTags", x => new { x.AssetId, x.TagId });
                    table.ForeignKey(
                        name: "FK_AssetTags_Asset_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Galleries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    CoverSourceId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Galleries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Galleries_Asset_CoverSourceId",
                        column: x => x.CoverSourceId,
                        principalTable: "Asset",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CoverAssetIdx = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImportTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PhysicalRelativePath = table.Column<string>(type: "TEXT", nullable: true),
                    GalleryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetGroups_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "Asset_GalleryId_IDX",
                table: "Asset",
                columns: new[] { "GalleryId", "RelativePath" });

            migrationBuilder.CreateIndex(
                name: "Asset_GroupId_IDX",
                table: "Asset",
                columns: new[] { "GroupId", "GroupPosition" });

            migrationBuilder.CreateIndex(
                name: "Asset_Hash_IDX",
                table: "Asset",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "AssetGroups_GalleryId_IDX",
                table: "AssetGroups",
                columns: new[] { "GalleryId", "PhysicalRelativePath" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ImageTags_ImageId_IDX",
                table: "AssetTags",
                columns: new[] { "AssetId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ImageTags_TagId_IDX",
                table: "AssetTags",
                columns: new[] { "TagId", "AssetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Galleries_CoverSourceId",
                table: "Galleries",
                column: "CoverSourceId");

            migrationBuilder.CreateIndex(
                name: "Tags_CanonicalId_IDX",
                table: "Tags",
                column: "CanonicalId");

            migrationBuilder.CreateIndex(
                name: "Tags_CategoryId_IDX",
                table: "Tags",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "Tags_Name_IDX",
                table: "Tags",
                column: "Name",
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Asset_AssetGroups_GroupId",
                table: "Asset");

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Galleries_GalleryId",
                table: "Asset");

            migrationBuilder.DropTable(
                name: "AssetTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "TagCategories");

            migrationBuilder.DropTable(
                name: "AssetGroups");

            migrationBuilder.DropTable(
                name: "Galleries");

            migrationBuilder.DropTable(
                name: "Asset");
        }
    }
}
