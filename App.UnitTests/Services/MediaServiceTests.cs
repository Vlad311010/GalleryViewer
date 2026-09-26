using App.Enums;
using App.Exceptions;
using App.Interfaces.Services;
using App.Models.Dtos.Media;
using App.Models.Queries;
using App.Services;
using App.UnitTests.Extensions;
using App.UnitTests.Fixtures;
using AutoFixture;
using Data.Context;
using Data.Entities;
using FluentValidation;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace App.UnitTests.Services
{
    public class MediaServiceTests
    {
        private readonly MediaService sut;
        private readonly IMediaAccessorService mediaAccessorServiceStub;
        private readonly AssetsCatalogContext context;
        private readonly Fixture fixture;

        public MediaServiceTests()
        {
            fixture = new TestFixture();

            mediaAccessorServiceStub = Substitute.For<IMediaAccessorService>();
            context = Substitute.For<AssetsCatalogContext>();

            sut = new MediaService(context, mediaAccessorServiceStub);
        }

        #region GetAssetMimeTypeAsync
        [Fact]
        public async Task GetAssetMimeTypeAsync_ThrowsEntityNotFoundException_WhenNonExistentingAsset()
        {
            // Arrange 
            AssetQuery query = new(1);

            // Act 
            var action = async () => await sut.GetAssetMimeTypeAsync(query);

            // Asset
            var exception = await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
            Assert.Equal(typeof(Asset), exception.EntityType);
        }

        [Fact]
        public async Task GetAssetMimeTypeAsync_ThrowsValidationException_WhenInvalidId()
        {
            // Arrange 
            AssetQuery query = new(0);

            // Act 
            var action = async () => await sut.GetAssetMimeTypeAsync(query);

            // Asset
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(AssetQuery.AssetId));
        }

        [Fact]
        public async Task GetAssetMimeTypeAsync_ThrowsArgumentNullException_WhenNullArgument()
        {
            // Arrange 
            AssetQuery query = null!;

            // Act 
            var action = async () => await sut.GetAssetMimeTypeAsync(query);

            // Asset
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task GetAssetMimeTypeAsync_ReturnsMimetype_WhenExistingAsset()
        {
            // Arrange 
            string mimeType = "image/png";
            int assetId = 1;

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .With(x => x.MimeType, mimeType)
                .Create();

            context.Assets.FindAsync(Arg.Any<object>()).Returns(asset);
            AssetQuery query = new(assetId);

            // Act 
            var result = await sut.GetAssetMimeTypeAsync(query);

            // Asset
            Assert.Equal(mimeType, result);
        }
        #endregion


        #region GetPreviewAsync
        [Fact]
        public async Task GetPreviewAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            // Arrange
            PreviewQuery query = null!;

            // Act
            var action = async () => await sut.GetPreviewAsync(query);

            // Asset
            await Assert.ThrowsAnyAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task GetPreviewAsync_ThrowsValidationException_WhenInvalidId()
        {
            // Arrange
            PreviewQuery query = new(DisplayItemType.Asset, -1);

            // Act
            var action = async () => await sut.GetPreviewAsync(query);

            // Asset
            var exception = await Assert.ThrowsAnyAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(PreviewQuery.ItemId));
        }

        [Fact]
        public async Task GetPreviewAsync_ThrowsValidationException_WhenInvalidType()
        {
            // Arrange
            PreviewQuery query = new((DisplayItemType)(-1), 1);

            // Act
            var action = async () => await sut.GetPreviewAsync(query);

            // Asset
            var exception = await Assert.ThrowsAnyAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(PreviewQuery.ItemType));
        }

        [Theory]
        [InlineData(DisplayItemType.Asset, 1, typeof(Asset))]
        [InlineData(DisplayItemType.Group, 1, typeof(Asset))]
        public async Task GetPreviewAsync_ThrowsEntityNotFoundException_WhenNonExistentingAsset(DisplayItemType itemType, int id, Type entityType)
        {
            // Arrange
            AssetGroup group = fixture.Build<AssetGroup>()
                .With(x => x.Id, id)
                .Create();

            var groupsDbSetMock = new AssetGroup[] { group }.BuildMockDbSet();
            var assetsDbSetMock = Array.Empty<Asset>().BuildMockDbSet();

            context.AssetGroups.Returns(groupsDbSetMock);
            context.Assets.Returns(assetsDbSetMock);

            PreviewQuery query = new(itemType, id);

            // Act
            var action = async () => await sut.GetPreviewAsync(query);

            // Assert
            var exception = await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
            Assert.Equal(entityType, exception.EntityType);
        }

        [Fact]
        public async Task GetPreviewAsync_ThrowsEntityNotFoundException_WhenNonExistentingGroup()
        {
            // Arrange
            int id = 1;
            DisplayItemType itemType = DisplayItemType.Group;

            var groupsDbSetMock = Array.Empty<AssetGroup>().BuildMockDbSet();

            context.AssetGroups.Returns(groupsDbSetMock);

            PreviewQuery query = new(itemType, id);

            // Act
            var action = async () => await sut.GetPreviewAsync(query);

            // Asset
            var exception = await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
            Assert.Equal(typeof(AssetGroup), exception.EntityType);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("  ")]
        [InlineData("somePath")]
        public async Task GetPreviewAsync_ThrowsMediaNotFoundException_WhenPreviewNotExists(string? previewFilePath)
        {
            // Arrange
            int assetId = 1;
            DisplayItemType itemType = DisplayItemType.Asset;

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .With(x => x.PreviewPath, previewFilePath)
                .Create();

            context.Assets.FindAsync(Arg.Any<int>()).Returns(asset);

            mediaAccessorServiceStub.Exists(Arg.Any<string>()).Returns(false);

            PreviewQuery query = new(itemType, assetId);

            // Act
            var action = async () => await sut.GetPreviewAsync(query);

            // Assert
            var exception = await Assert.ThrowsAsync<MediaNotFoundException>(action);
            Assert.Equal(previewFilePath, exception.MediaPath);
        }

        [Theory]
        [InlineData("path.jpg", "image/jpeg")]
        [InlineData("path.png", "image/png")]
        [InlineData("path.webp", "image/webp")]
        public async Task GetPreviewAsync_Returns_WhenPreviewExists(string? previewFilePath, string mimeType)
        {
            // Arrange
            int assetId = 1;
            DisplayItemType itemType = DisplayItemType.Asset;
            Stream mediaStream = Stream.Null;

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .With(x => x.PreviewPath, previewFilePath)
                .Create();

            context.Assets.FindAsync(Arg.Any<int>()).Returns(asset);

            mediaAccessorServiceStub.Exists(Arg.Any<string>()).Returns(true);
            mediaAccessorServiceStub.GetMediaData(Arg.Any<string>()).Returns(mediaStream);

            PreviewQuery query = new(itemType, assetId);

            // Act
            var result = await sut.GetPreviewAsync(query);

            // Assert
            Assert.Same(mediaStream, result.MediaStream);
            Assert.Equal(mimeType, result.MimeType);
        }
        #endregion

        #region GetAssetMediaAsync
        [Fact]
        public async Task GetAssetMediaAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            // Arrange
            AssetQuery query = null!;

            // Act
            var action = async () => await sut.GetAssetMediaAsync(query);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task GetAssetMediaAsync_ThrowsValidationException_WhenInvalidAssetId()
        {
            // Arrange
            int assetId = -1;
            AssetQuery query = new(assetId);

            // Act
            var action = async () => await sut.GetAssetMediaAsync(query);

            // Assert
            var excepiton = await Assert.ThrowsAsync<ValidationException>(action);
            excepiton.AssertSingleError(nameof(AssetQuery.AssetId));
        }

        [Fact]
        public async Task GetAssetMediaAsync_ThrowsEntityNotFoundException_WhenNonExistentingAsset()
        {
            // Arrange
            int assetId = 1;
            AssetQuery query = new(assetId);

            // Act
            var result = async () => await sut.GetAssetMediaAsync(query);

            // Assert
            var exception = await Assert.ThrowsAnyAsync<EntityNotFoundException>(result);
            Assert.Equal(typeof(Asset), exception.EntityType);
        }

        [Fact]
        public async Task GetAssetMediaAsync_ThrowsEntityNotFoundException_WhenNonExistentingGallery()
        {
            // Arrange
            int assetId = 1;
            AssetQuery query = new(assetId);

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            context.Assets.FindAsync(Arg.Any<int>()).Returns(asset);

            // Act
            var action = async () => await sut.GetAssetMediaAsync(query);

            // Assert
            var exception = await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
            Assert.Equal(typeof(Gallery), exception.EntityType);
        }

        [Fact]
        public async Task GetAssetMediaAsync_ThrowsMediaNotFoundException_WhenNonExistingMedia()
        {
            // Arrange
            int assetId = 1;
            AssetQuery query = new(assetId);

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .Create();

            Gallery gallery = fixture.Build<Gallery>()
                .Create();

            context.Assets.FindAsync(Arg.Any<int>()).Returns(asset);
            context.Galleries.FindAsync(Arg.Any<int>()).Returns(gallery);

            mediaAccessorServiceStub.Exists(Arg.Any<string>()).Returns(false);

            // Act
            var action = async () => await sut.GetAssetMediaAsync(query);

            // Assert
            var exception = await Assert.ThrowsAsync<MediaNotFoundException>(action);
            Assert.Equal(Path.Combine(gallery.Path, asset.RelativePath), exception.MediaPath);
        }

        [Fact]
        public async Task GetAssetMediaAsync_Returns_WhenExistingMedia()
        {
            // Arrange
            int assetId = 1;
            string assetPath = "file.png";
            Stream mediaStream = Stream.Null;

            AssetQuery query = new(assetId);

            Asset asset = fixture.Build<Asset>()
                .With(x => x.Id, assetId)
                .With(x => x.RelativePath, assetPath)
                .Create();

            Gallery gallery = fixture.Build<Gallery>()
                .Create();

            context.Assets.FindAsync(Arg.Any<int>()).Returns(asset);
            context.Galleries.FindAsync(Arg.Any<int>()).Returns(gallery);

            mediaAccessorServiceStub.Exists(Arg.Any<string>()).Returns(true);
            mediaAccessorServiceStub.GetMediaData(Arg.Any<string>()).Returns(mediaStream);

            // Act
            var result = await sut.GetAssetMediaAsync(query);

            // Assert
            Assert.Same(mediaStream, result.MediaStream);
            Assert.Equal(asset.MimeType, result.MimeType);
        }
        #endregion
    }
}
