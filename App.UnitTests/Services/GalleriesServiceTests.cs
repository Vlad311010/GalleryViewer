using App.Exceptions;
using App.Models.Commands;
using App.Models.Dtos.Gallery;
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
    public class GalleriesServiceTests
    {
        private readonly GalleriesService sut;
        private readonly AssetsCatalogContext context;
        private readonly Fixture fixture;

        public GalleriesServiceTests()
        {
            fixture = new TestFixture();

            context = Substitute.For<AssetsCatalogContext>();

            sut = new GalleriesService(context, NullLogger<GalleriesService>.Instance);
        }

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenCommandIsNull()
        {
            // Arrange
            GalleryCreateCommand command = null!;

            // Act
            Func<Task> action = () => sut.CreateAsync(command);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task CreateAsync_ThrowsValidationException_WhenInvalidName()
        {
            // Arrange
            GalleryCreateCommand command = new("", "/path");

            // Act
            Func<Task> action = () => sut.CreateAsync(command);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(GalleryCreateCommand.Name));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("./path")]
        [InlineData("path/to/gallery")]
        public async Task CreateAsync_ThrowsValidationException_WhenInvalidPath(string? path)
        {
            // Arrange
            GalleryCreateCommand command = new("gallery", path!);

            // Act
            Func<Task> action = () => sut.CreateAsync(command);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(GalleryCreateCommand.Path));
        }

        [Fact]
        public async Task CreateAsync_CreatesGallery()
        {
            // Arrange
            string name = "My Gallery";
            string path = "/galleries/my-gallery";
            int id = 23;
            GalleryCreateCommand command = new(name, path);

            Gallery gallery = fixture.Build<Gallery>()
                .Without(x => x.Id)
                .Create();

            context.Galleries
                .AddAsync(Arg.Do<Gallery>(g => g.Id = id))
                .Returns(ValueTask.FromResult((EntityEntry<Gallery>)null!));

            // Act
            GalleryDto result = await sut.CreateAsync(command);

            // Assert
            await context.Received(1).SaveChangesAsync();

            Assert.Equal(id, result.Id);
            Assert.Equal(name.ToLower(), result.Name);
            Assert.Equal(path, result.Path);
            Assert.Null(result.CoverAssetId);
        }

        #endregion

        #region GetByNameAsync

        [Fact]
        public async Task GetByNameAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            // Arrange
            GalleryByNameQuery query = null!;

            // Act
            Func<Task> action = () => sut.GetByNameAsync(query);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task GetByNameAsync_ThrowsValidationException_WhenInvalidGalleryName()
        {
            // Arrange
            GalleryByNameQuery query = new("");

            // Act
            Func<Task> action = () => sut.GetByNameAsync(query);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(GalleryByNameQuery.GalleryName));
        }

        [Fact]
        public async Task GetByNameAsync_ReturnsNull_WhenNonExistingGallery()
        {
            // Arrange
            GalleryByNameQuery query = new("gallery");

            var galleriesDbSetMock = Array.Empty<Gallery>().BuildMockDbSet();
            context.Galleries.Returns(galleriesDbSetMock);

            // Act
            GalleryDto? result = await sut.GetByNameAsync(query);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByNameAsync_ReturnsGallery_WhenGalleryExists()
        {
            // Arrange
            string name = "gallery";
            string path = "/path";
            GalleryByNameQuery query = new(name);

            Gallery gallery = fixture.Build<Gallery>()
                .With(x => x.Name, name)
                .With(x => x.Path, path)
                .Create();

            var galleriesDbSetMock = new[] { gallery }.BuildMockDbSet();
            context.Galleries.Returns(galleriesDbSetMock);

            // Act
            GalleryDto? result = await sut.GetByNameAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(gallery.Id, result.Id);
            Assert.Equal(gallery.Name, result.Name);
            Assert.Equal(gallery.Path, result.Path);
            Assert.Equal(gallery.CoverSourceId, result.CoverAssetId);
        }

        #endregion

        #region ListAsync

        [Fact]
        public async Task ListAsync_ReturnsEmptyCollection_WhenNoGalleriesExist()
        {
            // Arrange
            Gallery[] galleries = [];

            var galleriesDbSetMock = galleries.BuildMockDbSet();
            context.Galleries.Returns(galleriesDbSetMock);

            // Act
            IEnumerable<GalleryDto> result = await sut.ListAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task ListAsync_ReturnsGalleries_OrderedById()
        {
            // Arrange
            Gallery gallery1 = fixture.Build<Gallery>()
                .With(x => x.Id, 1)
                .Create();

            Gallery gallery2 = fixture.Build<Gallery>()
                .With(x => x.Id, 2)
                .Create();

            Gallery gallery3 = fixture.Build<Gallery>()
                .With(x => x.Id, 3)
                .Create();

            Gallery[] galleries = [gallery3, gallery1, gallery2];

            var galleriesDbSetMock = galleries.BuildMockDbSet();
            context.Galleries.Returns(galleriesDbSetMock);

            // Act
            IEnumerable<GalleryDto> result = await sut.ListAsync();

            // Assert
            GalleryDto[] resultArray = result.ToArray();

            Assert.Equal(3, resultArray.Length);
            Assert.Equal(gallery1.Id, resultArray[0].Id);
            Assert.Equal(gallery2.Id, resultArray[1].Id);
            Assert.Equal(gallery3.Id, resultArray[2].Id);
        }

        [Fact]
        public async Task ListAsync_ReturnsGalleryProperties()
        {
            // Arrange
            Gallery gallery = fixture.Create<Gallery>();

            var galleriesDbSetMock = new[] { gallery }.BuildMockDbSet();
            context.Galleries.Returns(galleriesDbSetMock);

            // Act
            IEnumerable<GalleryDto> result = await sut.ListAsync();

            // Assert
            GalleryDto galleryDto = Assert.Single(result);

            Assert.Equal(gallery.Id, galleryDto.Id);
            Assert.Equal(gallery.Name, galleryDto.Name);
            Assert.Equal(gallery.Path, galleryDto.Path);
            Assert.Equal(gallery.CoverSourceId, galleryDto.CoverAssetId);
        }

        #endregion

        #region StageUpdatePreviewAssetAsync

        [Fact]
        public async Task StageUpdatePreviewAssetAsync_ThrowsEntityNotFoundException_WhenNonExistingGallery()
        {
            // Arrange
            int galleryId = 23;
            int coverSourceId = 42;

            var galleriesDbSetMock = Array.Empty<Gallery>().BuildMockDbSet();
            context.Galleries.Returns(galleriesDbSetMock);

            // Act
            Func<Task> action = () => sut.StageUpdatePreviewAssetAsync(galleryId, coverSourceId);

            // Assert
            await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
        }

        [Fact]
        public async Task StageUpdatePreviewAssetAsync_UpdatesCoverSourceId()
        {
            // Arrange
            int galleryId = 23;
            int coverSourceId = 42;

            Gallery gallery = fixture.Build<Gallery>()
                .With(x => x.Id, galleryId)
                .Create();

            context.Galleries.FindAsync(Arg.Any<int>()).Returns(gallery);

            // Act
            await sut.StageUpdatePreviewAssetAsync(galleryId, coverSourceId);

            // Assert
            Assert.Equal(coverSourceId, gallery.CoverSourceId);
        }

        #endregion
    }
}
