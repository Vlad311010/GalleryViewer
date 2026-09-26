using App.Commands;
using App.Exceptions;
using App.Interfaces.Services;
using App.Models.Commands;
using App.Models.Dtos.Asset;
using App.Models.Dtos.Tag;
using App.Models.Queries;
using App.Services;
using App.UnitTests.Extensions;
using App.UnitTests.Fixtures;
using AutoFixture;
using Data.Context;
using Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace App.UnitTests.Services
{
    public class AssetsServiceTests
    {
        private readonly AssetsService sut;
        private readonly AssetsCatalogContext context;
        private readonly IMediaAccessorService mediaAccessorServiceStub;
        private readonly TimeProvider timeProvider;
        private readonly Fixture fixture;

        public AssetsServiceTests()
        {
            fixture = new TestFixture();

            mediaAccessorServiceStub = Substitute.For<IMediaAccessorService>();
            timeProvider = Substitute.For<TimeProvider>();


            context = Substitute.For<AssetsCatalogContext>();

            sut = new AssetsService(context, mediaAccessorServiceStub, timeProvider, NullLogger<AssetsService>.Instance);
        }

        #region StageDelete
        [Fact]
        public void StageDelete_ThrowsArgumentNullExceptiont_WhenCommandIsNull()
        {
            // Arrange
            AssetDeleteCommand command = null!;

            // Act
            var action = () => sut.StageDelete(command);

            // Assert
            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        public void StageDelete_RemovesAsset_WhenCalled()
        {
            // Arrange
            int assetId = 1;
            AssetDeleteCommand command = new(assetId);

            // Act
            sut.StageDelete(command);

            // Assert
            context.Assets.Received(1).Remove(Arg.Is<Asset>(x => x.Id == assetId));
        }
        #endregion

        #region ExistsAsync
        [Fact]
        public async Task ExistsAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            // Arrange
            AssetExistsQuery query = null!;

            // Act
            var action = async () => await sut.ExistsAsync(query);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task ExistsAsync_ThrowsValidationException_WhenInvalidGalleryId()
        {
            // Arrange
            int galleryId = -1;
            string assetPath = "./somePath.png";
            AssetExistsQuery query = new(galleryId, assetPath);

            // Act
            var action = async () => await sut.ExistsAsync(query);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(AssetExistsQuery.GalleryId));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("  ")]
        [InlineData("/somePath")]
        [InlineData("/somePath.png")]
        public async Task ExistsAsync_ThrowsValidationException_WhenInvalidAssetPath(string? assetPath)
        {
            // Arrange
            int galleryId = 1;
            AssetExistsQuery query = new(galleryId, assetPath!);

            // Act
            var action = async () => await sut.ExistsAsync(query);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(AssetExistsQuery.RelativePath));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(3)]
        public async Task ExistsAsync_RetunsFalse_WhenNoMatchingAssets(int assetsCount)
        {
            // Arrange
            int galleryId = 1;
            string assetPath = "./somePath.png";
            AssetExistsQuery query = new(galleryId, assetPath);

            Asset[] assets = fixture.Build<Asset>()
                .CreateMany(assetsCount)
                .ToArray();

            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            var result = await sut.ExistsAsync(query);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_RetunsTrue_WhenMatchingAssets()
        {
            // Arrange
            int galleryId = 1;
            string assetPath = "./somePath.png";
            AssetExistsQuery query = new(galleryId, assetPath);

            Asset[] assets = [
                    fixture.Build<Asset>()
                    .With(x => x.GalleryId, galleryId)
                    .With(x => x.RelativePath, assetPath)
                    .Create()
                ];

            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            var result = await sut.ExistsAsync(query);

            // Assert
            Assert.True(result);
        }
        #endregion

        #region StageCreateAssetAsync

        [Fact]
        public async Task StageCreateAssetAsync_ThrowsArgumentNullException_WhenCommandIsNull()
        {
            // Arrange
            AssetCreateCommand command = null!;

            // Act
            var action = async () => await sut.StageCreateAssetAsync(command);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task StageCreateAssetAsync_ThrowsValidationException_WhenInvalidGalleryId()
        {
            // Arrange
            AssetCreateCommand command = fixture.Build<AssetCreateCommand>()
                .With(x => x.GalleryId, -1)
                .Create();

            // Act
            var action = async () => await sut.StageCreateAssetAsync(command);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(AssetCreateCommand.GalleryId));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("  ")]
        [InlineData("/somePath")]
        [InlineData("/somePath.png")]
        public async Task StageCreateAssetAsync_ThrowsValidationException_WhenInvalidRelativePath(string? relativePath)
        {
            // Arrange
            AssetCreateCommand command = fixture.Build<AssetCreateCommand>()
                .With(x => x.RelativePath, relativePath)
                .Create();

            // Act
            var action = async () => await sut.StageCreateAssetAsync(command);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(AssetCreateCommand.RelativePath));
        }

        [Fact]
        public async Task StageCreateAssetAsync_ThrowsEntityNotFoundException_WhenNonExistingGallery()
        {
            // Arrange
            int galleryId = 1;

            AssetCreateCommand command = fixture.Build<AssetCreateCommand>()
                .With(x => x.GalleryId, galleryId)
                .Create();

            context.Galleries.FindAsync(Arg.Any<int>())
                .Returns((Gallery?)null);

            // Act
            var action = async () => await sut.StageCreateAssetAsync(command);

            // Assert
            var exception = await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
            Assert.Equal(typeof(Gallery), exception.EntityType);
        }

        [Fact]
        public async Task StageCreateAssetAsync_ThrowsMediaNotFoundException_WhenAssetFileDoesNotExist()
        {
            // Arrange
            int galleryId = 1;
            string galleryPath = "./gallery";
            string relativePath = "somePath.png";
            string expectedAssetPath = Path.Combine(galleryPath, relativePath);

            Gallery gallery = fixture.Build<Gallery>()
                .With(x => x.Id, galleryId)
                .With(x => x.Path, galleryPath)
                .Create();

            AssetCreateCommand command = fixture.Build<AssetCreateCommand>()
                .With(x => x.GalleryId, galleryId)
                .With(x => x.RelativePath, relativePath)
                .Create();

            context.Galleries.FindAsync(Arg.Any<int>())
                .Returns(gallery);

            mediaAccessorServiceStub.Exists(expectedAssetPath)
                .Returns(false);

            // Act
            var action = async () => await sut.StageCreateAssetAsync(command);

            // Assert
            var exception = await Assert.ThrowsAsync<MediaNotFoundException>(action);
            Assert.Equal(expectedAssetPath, exception.MediaPath);
        }

        [Fact]
        public async Task StageCreateAssetAsync_CreatesAsset_WhenValidCommand()
        {
            // Arrange
            int galleryId = 1;
            int groupId = 2;
            int groupPosition = 3;
            string galleryPath = "./gallery";
            string relativePath = "somePath.png";
            string previewPath = "preview.png";
            string assetPath = Path.Combine(galleryPath, relativePath);

            string expectedHashhash = "d41d8cd98f00b204e9800998ecf8427e";


            DateTime modifiedTime = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            DateTime utcNow = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

            Gallery gallery = fixture.Build<Gallery>()
                .With(x => x.Id, galleryId)
                .With(x => x.Path, galleryPath)
                .Create();

            AssetCreateCommand command = fixture.Build<AssetCreateCommand>()
                .With(x => x.GalleryId, galleryId)
                .With(x => x.RelativePath, relativePath)
                .With(x => x.PreviewPath, previewPath)
                .With(x => x.GroupId, groupId)
                .With(x => x.GroupPosition, groupPosition)
                .Create();

            context.Galleries.FindAsync(Arg.Any<int>())
                .Returns(gallery);

            mediaAccessorServiceStub.Exists(assetPath)
                .Returns(true);

            mediaAccessorServiceStub.GetLastModifiedTime(assetPath)
                .Returns(modifiedTime);

            mediaAccessorServiceStub.GetMediaData(assetPath)
                .Returns(Stream.Null);

            timeProvider.GetUtcNow().Returns(utcNow);

            Asset addedAsset = null!;
            context.Assets
                .AddAsync(Arg.Do<Asset>(asset => addedAsset = asset))
                .Returns(new ValueTask<EntityEntry<Asset>>());

            // Act
            await sut.StageCreateAssetAsync(command);

            // Assert
            await context.Assets.Received(1).AddAsync(Arg.Any<Asset>());

            Assert.Equal(relativePath, addedAsset.RelativePath);
            Assert.Equal(previewPath, addedAsset.PreviewPath);
            Assert.Equal(galleryId, addedAsset.GalleryId);
            Assert.Equal(groupId, addedAsset.GroupId);
            Assert.Equal(groupPosition, addedAsset.GroupPosition);
            Assert.Equal(utcNow, addedAsset.ImportTime);
            Assert.Equal(modifiedTime, addedAsset.CreationTime);
            Assert.Equal(expectedHashhash, addedAsset.Hash);
        }
        #endregion

        #region GetAssetGroupInfoAsync

        [Fact]
        public async Task GetAssetGroupInfoAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            // Arrange
            AssetGroupInfoQuery query = null!;

            // Act
            var action = async () => await sut.GetAssetGroupInfoAsync(query);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task GetAssetGroupInfoAsync_ThrowsValidationException_WhenInvalidId()
        {
            // Arrange
            AssetGroupInfoQuery query = new(-1);

            // Act
            var action = async () => await sut.GetAssetGroupInfoAsync(query);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(AssetGroupInfoQuery.AssetId));
        }

        [Fact]
        public async Task GetAssetGroupInfoAsync_ReturnsNull_WhenNonExistingAsset()
        {
            // Arrange
            int assetId = 1;

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns((Asset?)null);

            AssetGroupInfoQuery query = new(assetId);

            // Act
            var result = await sut.GetAssetGroupInfoAsync(query);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAssetGroupInfoAsync_ReturnsAssetInfoWithoutGroup_WhenAssetIsNotInGroup()
        {
            // Arrange
            int assetId = 1;

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Without(x => x.GroupId)
                .Create();

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns(asset);

            AssetGroupInfoQuery query = new(assetId);

            // Act
            var result = await sut.GetAssetGroupInfoAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(assetId, result.AssetId);
            Assert.Empty(result.GroupAssets);
        }

        [Fact]
        public async Task GetAssetGroupInfoAsync_ReturnsGroupAssets_WhenAssetBelongsToGroup()
        {
            // Arrange
            int assetId = 1;
            int groupId = 10;

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .With(x => x.GroupId, groupId)
                .With(x => x.GroupPosition, 2)
                .Create();

            Asset firstGroupAsset = fixture.Build<Asset>()
                .With(x => x.Id, 2)
                .With(x => x.GroupId, groupId)
                .With(x => x.GroupPosition, 1)
                .Create();

            Asset lastGroupAsset = fixture.Build<Asset>()
                .With(x => x.Id, 3)
                .With(x => x.GroupId, groupId)
                .With(x => x.GroupPosition, 3)
                .Create();

            Asset[] assets =
            [
                asset,
                lastGroupAsset,
                firstGroupAsset
            ];

            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            context.Assets.FindAsync(Arg.Any<int>()).Returns(asset);

            AssetGroupInfoQuery query = new(assetId);

            // Act
            var result = await sut.GetAssetGroupInfoAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(assetId, result.AssetId);
            Assert.Equal([firstGroupAsset.Id, asset.Id, lastGroupAsset.Id], result.GroupAssets);
        }
        #endregion

        #region GetAssetTagsAsync
        [Fact]
        public async Task GetAssetTagsAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            // Arrange
            AssetTagsQuery query = null!;

            // Act
            var action = async () => await sut.GetAssetTagsAsync(query);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task GetAssetTagsAsync_ThrowsValidationException_WhenInvalidId()
        {
            // Arrange
            AssetTagsQuery query = new(-1);

            // Act
            var action = async () => await sut.GetAssetTagsAsync(query);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(AssetTagsQuery.AssetId));
        }

        [Fact]
        public async Task GetAssetTagsAsync_ThrowsEntityNotFoundException_WhenNonExistingAsset()
        {
            // Arrange
            int assetId = 1;

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns((Asset?)null);

            AssetTagsQuery query = new(assetId);

            // Act
            var action = async () => await sut.GetAssetTagsAsync(query);

            // Assert
            var exception = await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
            Assert.Equal(typeof(Asset), exception.EntityType);
        }

        [Fact]
        public async Task GetAssetTagsAsync_ReturnsEmptyTags_WhenAssetHasNoTags()
        {
            // Arrange
            int assetId = 1;

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns(asset);

            var assetTagsDbSetMock = Array.Empty<AssetTag>().BuildMockDbSet();
            var tagsDbSetMock = Array.Empty<Tag>().BuildMockDbSet();

            context.AssetTags.Returns(assetTagsDbSetMock);
            context.Tags.Returns(tagsDbSetMock);

            AssetTagsQuery query = new(assetId);

            // Act
            var result = await sut.GetAssetTagsAsync(query);

            // Assert
            Assert.Empty(result.Tags);
        }

        [Fact]
        public async Task GetAssetTagsAsync_ReturnsTagsGroupedByCategory()
        {
            // Arrange
            int assetId = 1;
            string categoryName = "color";

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            TagCategory category = fixture.Build<TagCategory>()
                .With(x => x.Name, categoryName)
                .Create();

            Tag tag = fixture.Build<Tag>()
                .With(x => x.Name, "Red")
                .With(x => x.Category, category)
                .Create();

            AssetTag assetTag = fixture.Build<AssetTag>()
                .With(x => x.AssetId, assetId)
                .With(x => x.TagId, tag.Id)
                .With(x => x.Tag, tag)
                .Create();

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns(asset);

            var assetTagsDbSetMock = new[] { assetTag }.BuildMockDbSet(); ;
            var tagsDbSetMock = new[] { tag }.BuildMockDbSet();

            context.AssetTags.Returns(assetTagsDbSetMock);
            context.Tags.Returns(tagsDbSetMock);

            AssetTagsQuery query = new(assetId);

            // Act
            var result = await sut.GetAssetTagsAsync(query);

            // Assert
            Assert.True(result.Tags.ContainsKey(categoryName));

            TagDtoInfo[] tags = result.Tags[categoryName];

            TagDtoInfo tagDto = Assert.Single(tags);
            Assert.Equal(tag.Id, tagDto.Id);
            Assert.Equal(tag.Name, tagDto.Name);
            Assert.Equal(categoryName, tagDto.Category);
        }

        [Fact]
        public async Task GetAssetTagsAsync_ReturnsTagsOrderedByName()
        {
            // Arrange
            int assetId = 1;
            string categoryName = "color";

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            TagCategory category = fixture.Build<TagCategory>()
                .With(x => x.Name, categoryName)
                .Create();

            Tag redTag = fixture.Build<Tag>()
                .With(x => x.Id, 10)
                .With(x => x.Name, "Red")
                .With(x => x.Category, category)
                .Create();

            Tag blueTag = fixture.Build<Tag>()
                .With(x => x.Id, 20)
                .With(x => x.Name, "Blue")
                .With(x => x.Category, category)
                .Create();

            AssetTag[] assetTags =
            [
                fixture.Build<AssetTag>()
                    .With(x => x.AssetId, assetId)
                    .With(x => x.TagId, redTag.Id)
                    .With(x => x.Tag, redTag)
                    .Create(),

                fixture.Build<AssetTag>()
                    .With(x => x.AssetId, assetId)
                    .With(x => x.TagId, blueTag.Id)
                    .With(x => x.Tag, blueTag)
                    .Create()
                    ];

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns(asset);

            var assetTagsDbSetMock = assetTags.BuildMockDbSet();
            var tagsDbSetMock = new[] { redTag, blueTag }.BuildMockDbSet();

            context.AssetTags.Returns(assetTagsDbSetMock);
            context.Tags.Returns(tagsDbSetMock);

            AssetTagsQuery query = new(assetId);

            // Act
            var result = await sut.GetAssetTagsAsync(query);

            // Assert
            Assert.Equal(["Blue", "Red"], result.Tags[categoryName].Select(x => x.Name));
        }
        #endregion

        #region AddTagsAsync

        [Fact]
        public async Task AddTagsAsync_ThrowsArgumentNullException_WhenCommandIsNull()
        {
            // Arrange
            AssetAddTagsCommand command = null!;

            // Act
            var action = async () => await sut.AddTagsAsync(command);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("  ")]
        [InlineData("snake_case")]
        [InlineData("tagWith[")]
        [InlineData("tagWith]")]
        [InlineData("tagWith{")]
        [InlineData("tagWith}")]
        [InlineData("tagWith:")]

        public async Task AddTagsAsync_ThrowsValidationException_WhenInvalidTag(string? tag)
        {
            // Arrange

            AssetAddTagsCommand command = fixture.Build<AssetAddTagsCommand>()
                .With(x => x.Tags, [tag!])
                .Create();

            // Act
            var action = async () => await sut.AddTagsAsync(command);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(AssetAddTagsCommand.Tags));
        }

        [Fact]
        public async Task AddTagsAsync_ThrowsValidationException_WhenInvalidAssetId()
        {
            // Arrange
            var validTag = $"tag{fixture.Create<Guid>():N}";

            AssetAddTagsCommand command = fixture.Build<AssetAddTagsCommand>()
                .With(x => x.AssetId, -1)
                .With(x => x.Tags, [validTag])
                .Create();

            // Act
            var action = async () => await sut.AddTagsAsync(command);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(AssetAddTagsCommand.AssetId));
        }

        [Fact]
        public async Task AddTagsAsync_ThrowsEntityNotFoundException_WhenNonExistingAsset()
        {
            // Arrange
            int assetId = 1;
            var validTag = $"tag{fixture.Create<Guid>():N}";

            AssetAddTagsCommand command = fixture.Build<AssetAddTagsCommand>()
                .With(x => x.AssetId, assetId)
                .With(x => x.Tags, [validTag])
                .Create();

            context.Assets.FindAsync(Arg.Any<int>()).Returns((Asset?)null);

            // Act
            var action = async () => await sut.AddTagsAsync(command);

            // Assert
            var exception = await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
            Assert.Equal(typeof(Asset), exception.EntityType);
        }

        [Fact]
        public async Task AddTagsAsync_ThrowsUnknownTagsException_WhenTagsAreUndefined()
        {
            // Arrange
            int assetId = 1;
            string[] tags = ["tag1", "undefined tag2"];

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            AssetAddTagsCommand command = fixture.Build<AssetAddTagsCommand>()
                .With(x => x.AssetId, assetId)
                .With(x => x.Tags, tags)
                .Create();

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns(asset);

            Tag definedTags = fixture.Build<Tag>()
                .With(x => x.Name, tags[0])
                .Create();


            var tagsDbSetMock = new Tag[] { definedTags }.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            var action = async () => await sut.AddTagsAsync(command);

            // Assert
            var exception = await Assert.ThrowsAsync<UnknownTagsException>(action);

            Assert.Equal([tags[1]], exception.InvalidTags);
        }

        [Fact]
        public async Task AddTagsAsync_AddsTags_WhenTagsAreDefined()
        {
            // Arrange
            int assetId = 1;

            string firstTagName = "tag1";
            string secondTagName = "tag2";

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            Tag firstTag = fixture.Build<Tag>()
                .With(x => x.Name, firstTagName)
                .Create();

            Tag secondTag = fixture.Build<Tag>()
                .With(x => x.Name, secondTagName)
                .Create();

            Tag[] definedTags = [firstTag, secondTag];

            AssetAddTagsCommand command = fixture.Build<AssetAddTagsCommand>()
                .With(x => x.AssetId, assetId)
                .With(x => x.Tags, [firstTagName, secondTagName])
                .Create();

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns(asset);

            var tagsDbSetMock = definedTags.BuildMockDbSet();
            var assetTagsDbSetMock = Array.Empty<AssetTag>().BuildMockDbSet();

            context.Tags.Returns(tagsDbSetMock);
            context.AssetTags.Returns(assetTagsDbSetMock);

            // Act
            await sut.AddTagsAsync(command);

            // Assert
            context.AssetTags.Received(1).Add(
                Arg.Is<AssetTag>(x =>
                    x.AssetId == assetId &&
                    x.TagId == firstTag.Id));

            context.AssetTags.Received(1).Add(
                Arg.Is<AssetTag>(x =>
                    x.AssetId == assetId &&
                    x.TagId == secondTag.Id));

            await context.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddTagsAsync_DuplicateTagsAddedOnce_WhenCommandContainsDuplicates()
        {
            // Arrange
            int assetId = 1;
            string tagName = "tag1";

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            Tag tag = fixture.Build<Tag>()
                .With(x => x.Name, tagName)
                .Create();

            AssetAddTagsCommand command = fixture.Build<AssetAddTagsCommand>()
                .With(x => x.AssetId, assetId)
                .With(x => x.Tags, new[] { tagName, tagName })
                .Create();

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns(asset);

            var tagsDbSetMock = new[] { tag }.BuildMockDbSet();
            var assetTagsDbSetMock = Array.Empty<AssetTag>().BuildMockDbSet();

            context.Tags.Returns(tagsDbSetMock);
            context.AssetTags.Returns(assetTagsDbSetMock);

            // Act
            await sut.AddTagsAsync(command);

            // Assert
            context.AssetTags.Received(1).Add(
                Arg.Is<AssetTag>(x =>
                    x.AssetId == assetId &&
                    x.TagId == tag.Id));

            await context.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task AddTagsAsync_DoesNotAddAlreadyAssignedTags()
        {
            // Arrange
            int assetId = 1;

            Tag existingTag = fixture.Build<Tag>()
                .With(x => x.Name, "existing")
                .Create();

            Tag newTag = fixture.Build<Tag>()
                .With(x => x.Name, "new")
                .Create();

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            AssetTag existingAssetTag = fixture.Build<AssetTag>()
                .With(x => x.AssetId, assetId)
                .With(x => x.TagId, existingTag.Id)
                .Create();

            AssetAddTagsCommand command = fixture.Build<AssetAddTagsCommand>()
                .With(x => x.AssetId, assetId)
                .With(x => x.Tags, new[] { existingTag.Name, newTag.Name })
                .Create();

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns(asset);

            var tagsDbSetMock = new[] { existingTag, newTag }.BuildMockDbSet();
            var assetTagsDbSetMock = new[] { existingAssetTag }.BuildMockDbSet();

            context.Tags.Returns(tagsDbSetMock);
            context.AssetTags.Returns(assetTagsDbSetMock);

            // Act
            await sut.AddTagsAsync(command);

            // Assert
            context.AssetTags.DidNotReceive().Add(
                Arg.Is<AssetTag>(x =>
                    x.TagId == existingTag.Id));

            context.AssetTags.Received(1).Add(
                Arg.Is<AssetTag>(x =>
                    x.AssetId == assetId &&
                    x.TagId == newTag.Id));

            await context.Received(1).SaveChangesAsync();
        }

        #endregion

        #region RemoveTagAsync

        [Fact]
        public async Task RemoveTagAsync_ThrowsArgumentNullException_WhenCommandIsNull()
        {
            // Arrange
            AssetRemoveTagCommand command = null!;

            // Act
            var action = async () => await sut.RemoveTagAsync(command);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task RemoveTagAsync_ThrowsValidationException_WhenInvalidAssetId()
        {
            // Arrange
            AssetRemoveTagCommand command = new(0, "tag");

            // Act
            var action = async () => await sut.RemoveTagAsync(command);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(AssetRemoveTagCommand.AssetId));
        }


        [Fact]
        public async Task RemoveTagAsync_ThrowsValidationException_WhenTagIsInvalid()
        {
            // Arrange
            AssetRemoveTagCommand command = new(1, "sna_ke");

            // Act
            var action = async () => await sut.RemoveTagAsync(command);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(AssetRemoveTagCommand.Tag));
        }

        [Fact]
        public async Task RemoveTagAsync_ThrowsEntityNotFoundException_WhenNonExistingAsset()
        {
            // Arrange
            int assetId = 1;
            AssetRemoveTagCommand command = new(assetId, "tag");

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns((Asset?)null);

            // Act
            var action = async () => await sut.RemoveTagAsync(command);

            // Assert
            var exception = await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
            Assert.Equal(typeof(Asset), exception.EntityType);
        }

        [Fact]
        public async Task RemoveTagAsync_ThrowsEntityNotFoundException_WhenNonExistingTag()
        {
            // Arrange
            int assetId = 1;
            string tagName = "tag";

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            AssetRemoveTagCommand command = new(assetId, tagName);

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns(asset);

            var tagsDbSetMock = Array.Empty<Tag>().BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            var action = async () => await sut.RemoveTagAsync(command);

            // Assert
            var exception = await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
            Assert.Equal(typeof(Tag), exception.EntityType);
        }

        [Fact]
        public async Task RemoveTagAsync_RemovesTag_WhenTagExists()
        {
            // Arrange
            int assetId = 1;
            string tagName = "tag";

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            Tag tag = fixture.Build<Tag>()
                .With(x => x.Name, tagName)
                .Create();

            AssetRemoveTagCommand command = new(assetId, tagName);

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns(asset);

            var tagsDbSetMock = new[] { tag }.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            await sut.RemoveTagAsync(command);

            // Assert
            context.AssetTags.Received(1).Remove(
                Arg.Is<AssetTag>(x =>
                    x.AssetId == assetId &&
                    x.TagId == tag.Id));

            await context.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RemoveTagAsync_FindsTagUsingLowercaseName()
        {
            // Arrange
            int assetId = 1;
            string tagName = "SomeTag";
            string normalizedTagName = tagName.ToLowerInvariant();

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            Tag tag = fixture.Build<Tag>()
                .With(x => x.Name, normalizedTagName)
                .Create();

            AssetRemoveTagCommand command = new(assetId, tagName);

            context.Assets.FindAsync(Arg.Any<int>())
                .Returns(asset);

            var tagsDbSetMock = new[] { tag }.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            await sut.RemoveTagAsync(command);

            // Assert
            context.AssetTags.Received(1).Remove(
                Arg.Is<AssetTag>(x =>
                    x.AssetId == assetId &&
                    x.TagId == tag.Id));
        }

        #endregion

        #region GetAssetsInBatchesAsync

        [Fact]
        public async Task GetAssetsInBatchesAsync_ReturnsEmpty_WhenNoMatchingAssets()
        {
            // Arrange
            int galleryId = 1;
            DateTime timeStamp = new(2025, 1, 1);

            var assetsDbSetMock = Array.Empty<Asset>().BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            var result = await sut
                .GetAssetsInBatchesAsync(galleryId, timeStamp, 10)
                .ToListAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAssetsInBatchesAsync_ReturnsMatchingAssets()
        {
            // Arrange
            int galleryId = 1;
            DateTime timeStamp = new(2025, 1, 1);

            Asset firstAsset = fixture.Build<Asset>()
                .With(x => x.Id, 1)
                .With(x => x.GalleryId, galleryId)
                .With(x => x.ImportTime, timeStamp.AddDays(-2))
                .Create();

            Asset secondAsset = fixture.Build<Asset>()
                .With(x => x.Id, 2)
                .With(x => x.GalleryId, galleryId)
                .With(x => x.ImportTime, timeStamp.AddDays(-1))
                .Create();

            Asset[] assets = [firstAsset, secondAsset];

            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            var result = await sut
                .GetAssetsInBatchesAsync(galleryId, timeStamp, 10)
                .ToListAsync();

            // Assert
            IReadOnlyList<AssetFileInfoDto> batch = Assert.Single(result);

            Assert.Equal(2, batch.Count);

            Assert.Equal(firstAsset.Id, batch[0].Id);
            Assert.Equal(firstAsset.RelativePath, batch[0].AssetRelativePath);
            Assert.Equal(firstAsset.GroupId, batch[0].GroupId);

            Assert.Equal(secondAsset.Id, batch[1].Id);
            Assert.Equal(secondAsset.RelativePath, batch[1].AssetRelativePath);
            Assert.Equal(secondAsset.GroupId, batch[1].GroupId);
        }

        [Fact]
        public async Task GetAssetsInBatchesAsync_ExcludesAssetsFromDifferentGallery()
        {
            // Arrange
            int galleryId = 1;
            int differentGalleryId = 2;
            DateTime timeStamp = new(2025, 1, 1);
            DateTime yesterdayTimeStamp = timeStamp.AddDays(-1);

            Asset matchingAsset = fixture.Build<Asset>()
                .With(x => x.Id, 1)
                .With(x => x.GalleryId, galleryId)
                .With(x => x.ImportTime, yesterdayTimeStamp)
                .Create();

            Asset differentGalleryAsset = fixture.Build<Asset>()
                .With(x => x.Id, 2)
                .With(x => x.GalleryId, differentGalleryId)
                .With(x => x.ImportTime, yesterdayTimeStamp)
                .Create();

            Asset[] assets = [matchingAsset, differentGalleryAsset];

            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            var result = await sut
                .GetAssetsInBatchesAsync(galleryId, timeStamp, 10)
                .ToListAsync();

            // Assert
            IReadOnlyList<AssetFileInfoDto> batch = Assert.Single(result);

            AssetFileInfoDto assetDto = Assert.Single(batch);

            Assert.Equal(matchingAsset.Id, assetDto.Id);
        }

        [Fact]
        public async Task GetAssetsInBatchesAsync_ExcludesAssetsImportedAtOrAfterTimestamp()
        {
            // Arrange
            int galleryId = 1;
            DateTime timeStamp = new(2025, 1, 1);

            Asset oldAsset = fixture.Build<Asset>()
                .With(x => x.Id, 1)
                .With(x => x.GalleryId, galleryId)
                .With(x => x.ImportTime, timeStamp.AddSeconds(-1))
                .Create();

            Asset timestampAsset = fixture.Build<Asset>()
                .With(x => x.Id, 2)
                .With(x => x.GalleryId, galleryId)
                .With(x => x.ImportTime, timeStamp)
                .Create();

            Asset newerAsset = fixture.Build<Asset>()
                .With(x => x.Id, 3)
                .With(x => x.GalleryId, galleryId)
                .With(x => x.ImportTime, timeStamp.AddSeconds(1))
                .Create();

            Asset[] assets = [oldAsset, timestampAsset, newerAsset];

            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            var result = await sut
                .GetAssetsInBatchesAsync(galleryId, timeStamp, 10)
                .ToListAsync();

            // Assert
            IReadOnlyList<AssetFileInfoDto> batch = Assert.Single(result);

            AssetFileInfoDto assetDto = Assert.Single(batch);

            Assert.Equal(oldAsset.Id, assetDto.Id);
        }

        [Fact]
        public async Task GetAssetsInBatchesAsync_ReturnsAssetsInBatches()
        {
            // Arrange
            int galleryId = 1;
            DateTime timeStamp = new(2025, 1, 1);
            DateTime yesterdayTimeStamp = timeStamp.AddDays(-1);
            int batchSize = 2;

            Asset firstAsset = fixture.Build<Asset>()
                .With(x => x.Id, 1)
                .With(x => x.GalleryId, galleryId)
                .With(x => x.ImportTime, yesterdayTimeStamp)
                .Create();

            Asset secondAsset = fixture.Build<Asset>()
                .With(x => x.Id, 2)
                .With(x => x.GalleryId, galleryId)
                .With(x => x.ImportTime, yesterdayTimeStamp)
                .Create();

            Asset thirdAsset = fixture.Build<Asset>()
                .With(x => x.Id, 3)
                .With(x => x.GalleryId, galleryId)
                .With(x => x.ImportTime, yesterdayTimeStamp)
                .Create();

            Asset[] assets = [thirdAsset, firstAsset, secondAsset];

            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            var result = await sut
                .GetAssetsInBatchesAsync(galleryId, timeStamp, batchSize)
                .ToListAsync();

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal([firstAsset.Id, secondAsset.Id], result[0].Select(x => x.Id));

            AssetFileInfoDto thirdAssetDto = Assert.Single(result[1]);
            Assert.Equal(thirdAsset.Id, thirdAssetDto.Id);
        }
        #endregion

        #region TryGetByHash


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void TryGetByHash_ThrowsArgumentException_WhenHashIsNullOrWhiteSpace(string? md5Hash)
        {
            // Arrange
            // Act
            Action action = () => sut.TryGetByHash(md5Hash!, out _);

            // Assert
            Assert.ThrowsAny<ArgumentException>(action);
        }

        [Fact]
        public void TryGetByHash_ReturnsFalse_WhenNonExistingHash()
        {
            // Arrange
            string md5Hash = "nonExistingHash";

            var assetsDbSetMock = Array.Empty<Asset>().BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            bool result = sut.TryGetByHash(md5Hash, out AssetDto assetDto);

            // Assert
            Assert.False(result);
            Assert.Null(assetDto);
        }

        [Fact]
        public void TryGetByHash_ReturnsTrueAndAsset_WhenMatchingHash()
        {
            // Arrange
            int id = 1;
            string md5Hash = "matchingHash";

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, id)
                .With(x => x.Hash, md5Hash)
                .Create();

            var assetsDbSetMock = new[] { asset }.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            bool result = sut.TryGetByHash(md5Hash, out AssetDto assetDto);

            // Assert
            Assert.True(result);
            Assert.NotNull(assetDto);

            Assert.Equal(id, assetDto.Id);
            Assert.Equal(asset.RelativePath, assetDto.RelativePath);
        }

        #endregion

        #region GetByPathAsync

        [Fact]
        public async Task GetByPathAsync_ReturnsNull_WhenNonExistingAsset()
        {
            // Arrange
            int galleryId = 1;
            string relativePath = "somePath.png";

            var assetsDbSetMock = Array.Empty<Asset>().BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            var result = await sut.GetByPathAsync(galleryId, relativePath);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByPathAsync_ReturnsAsset_WhenMatchingGalleryAndPath()
        {
            // Arrange
            int galleryId = 1;
            string relativePath = "somePath.png";

            Asset asset = fixture.Build<Asset>()
                .With(x => x.GalleryId, galleryId)
                .With(x => x.RelativePath, relativePath)
                .Create();

            var assetsDbSetMock = new[] { asset }.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            var result = await sut.GetByPathAsync(galleryId, relativePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(asset.Id, result.Id);
            Assert.Equal(asset.GalleryId, result.GalleryId);
            Assert.Equal(asset.RelativePath, result.RelativePath);
        }

        [Fact]
        public async Task GetByPathAsync_ReturnsNull_WhenAssetExistsInDifferentGallery()
        {
            // Arrange
            int galleryId = 1;
            int differentGalleryId = 2;
            string relativePath = "somePath.png";

            Asset asset = fixture.Build<Asset>()
                .With(x => x.GalleryId, differentGalleryId)
                .With(x => x.RelativePath, relativePath)
                .Create();

            var assetsDbSetMock = new[] { asset }.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            var result = await sut.GetByPathAsync(galleryId, relativePath);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByPathAsync_ReturnsNull_WhenAssetExistsWithDifferentPath()
        {
            // Arrange
            int galleryId = 1;
            string relativePath = "somePath.png";
            string differentRelativePath = "differentPath.png";

            Asset asset = fixture.Build<Asset>()
                .With(x => x.GalleryId, galleryId)
                .With(x => x.RelativePath, differentRelativePath)
                .Create();

            var assetsDbSetMock = new[] { asset }.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            var result = await sut.GetByPathAsync(galleryId, relativePath);

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}
