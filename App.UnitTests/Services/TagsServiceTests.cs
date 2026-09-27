using App.Exceptions;
using App.Models;
using App.Models.Commands;
using App.Models.Dtos.Tag;
using App.Models.Queries;
using App.Services;
using App.UnitTests.Extensions;
using App.UnitTests.Fixtures;
using AutoFixture;
using Data.Context;
using Data.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable.NSubstitute;
using NSubstitute;
using Shared.Models;

namespace App.UnitTests.Services
{
    public class TagsServiceTests
    {
        private readonly TagsService sut;
        private readonly AssetsCatalogContext context;
        private readonly Fixture fixture;

        public TagsServiceTests()
        {
            fixture = new TestFixture();

            context = Substitute.For<AssetsCatalogContext>();

            sut = new TagsService(context, NullLogger<TagsService>.Instance);
        }

        #region Get

        [Fact]
        public async Task GetAsync_ThrowsEntityNotFoundException_WhenNonExistingTag()
        {
            // Arrange
            int id = 1;

            Tag[] tags = [];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            Func<Task> action = () => sut.GetAsync(id);

            // Assert
            await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
        }

        [Fact]
        public async Task GetAsync_ReturnsTag_WhenTagExists()
        {
            // Arrange
            Tag tag = fixture.Create<Tag>();

            context.Tags.FindAsync(Arg.Any<int>()).Returns(tag);

            // Act
            TagDto result = await sut.GetAsync(tag.Id);

            // Assert
            Assert.Equal(tag.Id, result.Id);
            Assert.Equal(tag.Name, result.Name);
            Assert.Equal(tag.CategoryId, result.CategoryId);
            Assert.Equal(tag.CanonicalId, result.CanonicalId);
        }

        #endregion

        #region SearchAsync

        [Fact]
        public async Task SearchAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            // Arrange
            TagSearchQuery query = null!;

            // Act
            Func<Task> action = () => sut.SearchAsync(query);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task SearchAsync_ThrowsValidationException_WhenInvalidSearchKey()
        {
            // Arrange
            TagSearchQuery query = new("two words", 10);

            // Act
            Func<Task> action = () => sut.SearchAsync(query);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(TagSearchQuery.SearchKey));
        }

        [Fact]
        public async Task SearchAsync_ThrowsValidationException_WhenInvalidTake()
        {
            // Arrange
            TagSearchQuery query = new("key", -10);

            // Act
            Func<Task> action = () => sut.SearchAsync(query);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(TagSearchQuery.Take));
        }

        [Fact]
        public async Task SearchAsync_ReturnsEmptyCollection_WhenNoTagsMatch()
        {
            // Arrange
            TagSearchQuery query = new("red", 10);

            var tagsDbSetMock = Array.Empty<Tag>().BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            IEnumerable<TagDtoSearch> result = await sut.SearchAsync(query);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task SearchAsync_ReturnsMatchingTags()
        {
            // Arrange
            string searchKey = "red";
            string categoryName = "color";
            TagSearchQuery query = new(searchKey, 10);

            Tag tag = fixture.Build<Tag>()
                .With(x => x.Name, searchKey)
                .Without(x => x.CanonicalId)
                .Create();

            TagCategory category = fixture.Build<TagCategory>()
                .With(x => x.Id, tag.CategoryId)
                .With(x => x.Name, categoryName)
                .Create();

            Tag[] tags = [tag];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            TagCategory[] categories = [category];
            var tagCategoriesDbSetMock = categories.BuildMockDbSet();
            context.TagCategories.Returns(tagCategoriesDbSetMock);

            var assetTagsDbSetMock = Array.Empty<AssetTag>().BuildMockDbSet();
            context.AssetTags.Returns(assetTagsDbSetMock);

            // Act
            IEnumerable<TagDtoSearch> result = await sut.SearchAsync(query);

            // Assert
            TagDtoSearch tagDto = Assert.Single(result);

            Assert.Equal(tag.Id, tagDto.Id);
            Assert.Equal(tag.Name, tagDto.Name);
            Assert.Equal(categoryName, tagDto.Category);
            Assert.Equal(0, tagDto.Occurrences);
            Assert.True(tagDto.IsCanonical);
            Assert.Null(tagDto.CanonicalName);
        }

        [Fact]
        public async Task SearchAsync_ReturnsTagsMatchingSeparateWords()
        {
            // Arrange
            string searchKey = "red";
            TagSearchQuery query = new(searchKey, 10);

            Tag redTag = fixture.Build<Tag>()
                .With(x => x.Name, "red")
                .Create();

            Tag blueTag = fixture.Build<Tag>()
                .With(x => x.Name, "dark red")
                .Create();

            TagCategory category = fixture.Build<TagCategory>()
                .With(x => x.Id, redTag.CategoryId)
                .Create();

            TagCategory blueCategory = fixture.Build<TagCategory>()
                .With(x => x.Id, blueTag.CategoryId)
                .Create();

            Tag[] tags = [redTag, blueTag];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            TagCategory[] categories = [category, blueCategory];
            var tagCategoriesDbSetMock = categories.BuildMockDbSet();
            context.TagCategories.Returns(tagCategoriesDbSetMock);

            var assetTagsDbSetMock = Array.Empty<AssetTag>().BuildMockDbSet();
            context.AssetTags.Returns(assetTagsDbSetMock);

            // Act
            IEnumerable<TagDtoSearch> result = await sut.SearchAsync(query);

            // Assert
            TagDtoSearch[] resultArray = [.. result];

            Assert.Equal(2, resultArray.Length);
            Assert.Contains(resultArray, x => x.Id == redTag.Id);
            Assert.Contains(resultArray, x => x.Id == blueTag.Id);
        }

        [Fact]
        public async Task SearchAsync_RespectsTake()
        {
            // Arrange
            TagSearchQuery query = new("tag", 2);

            Tag tag1 = fixture.Build<Tag>()
                .With(x => x.Name, "tag one")
                .Create();

            Tag tag2 = fixture.Build<Tag>()
                .With(x => x.Name, "tag two")
                .Create();

            Tag tag3 = fixture.Build<Tag>()
                .With(x => x.Name, "tag three")
                .Create();

            Tag[] tags = [tag1, tag2, tag3];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            TagCategory category = fixture.Build<TagCategory>()
                .With(x => x.Id, tag1.CategoryId)
                .Create();

            TagCategory category2 = fixture.Build<TagCategory>()
                .With(x => x.Id, tag2.CategoryId)
                .Create();

            TagCategory category3 = fixture.Build<TagCategory>()
                .With(x => x.Id, tag3.CategoryId)
                .Create();

            TagCategory[] categories = [category, category2, category3];
            var tagCategoriesDbSetMock = categories.BuildMockDbSet();
            context.TagCategories.Returns(tagCategoriesDbSetMock);

            var assetTagsDbSetMock = Array.Empty<AssetTag>().BuildMockDbSet();
            context.AssetTags.Returns(assetTagsDbSetMock);

            // Act
            IEnumerable<TagDtoSearch> result = await sut.SearchAsync(query);

            // Assert
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ThrowsValidationException_WhenInvalidName()
        {
            // Arrange
            CreateTagCommand command = new("", "Color", null);

            // Act
            Func<Task> action = () => sut.Create(command);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(CreateTagCommand.Name));
        }

        [Fact]
        public async Task Create_ThrowsValidationException_WhenInvalidCategory()
        {
            // Arrange
            CreateTagCommand command = new("red", "", null);

            // Act
            Func<Task> action = () => sut.Create(command);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(CreateTagCommand.Category));
        }

        [Fact]
        public async Task Create_ThrowsEntityNotFoundException_WhenNonExistingTagCategory()
        {
            // Arrange
            string category = "Color";
            CreateTagCommand command = new("red", category, null);

            var tagCategoriesDbSetMock = Array.Empty<TagCategory>().BuildMockDbSet();
            context.TagCategories.Returns(tagCategoriesDbSetMock);

            // Act
            Func<Task> action = () => sut.Create(command);

            // Assert
            await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
        }

        [Fact]
        public async Task Create_ThrowsEntityAlreadyExistsException_WhenTagAlreadyExists()
        {
            // Arrange
            string name = "red";
            string category = "Color";
            CreateTagCommand command = new(name, category, null);

            TagCategory tagCategory = fixture.Build<TagCategory>()
                .With(x => x.Name, category)
                .Create();

            Tag existingTag = fixture.Build<Tag>()
                .With(x => x.Name, name)
                .Create();

            TagCategory[] categories = [tagCategory];
            var tagCategoriesDbSetMock = categories.BuildMockDbSet();
            context.TagCategories.Returns(tagCategoriesDbSetMock);

            Tag[] tags = [existingTag];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            Func<Task> action = () => sut.Create(command);

            // Assert
            var exception = await Assert.ThrowsAnyAsync<EntityAlreadyExistsException>(action);
            Assert.Equal(typeof(Tag), exception.EntityType);
        }

        [Fact]
        public async Task Create_CreatesTag()
        {
            // Arrange
            string name = "Red Tag";
            string category = "Color";
            int id = 23;

            CreateTagCommand command = new(name, category, null);

            TagCategory tagCategory = fixture.Build<TagCategory>()
                .With(x => x.Name, category)
                .Create();

            TagCategory[] categories = [tagCategory];
            var tagCategoriesDbSetMock = categories.BuildMockDbSet();
            context.TagCategories.Returns(tagCategoriesDbSetMock);

            var tagsDbSetMock = Array.Empty<Tag>().BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            context.Tags
                .Add(Arg.Do<Tag>(tag => tag.Id = id));

            // Act
            TagDtoInfo result = await sut.Create(command);

            // Assert
            await context.Received(1).SaveChangesAsync();

            Assert.Equal(id, result.Id);
            Assert.Equal("red tag", result.Name);
            Assert.Equal(category, result.Category);
            Assert.Null(result.CanonicalId);
        }

        #endregion

        #region ListAsync

        [Fact]
        public async Task ListAsync_ReturnsEmptyPage_WhenNoTagsExist()
        {
            // Arrange
            Pagination pagination = new(0, 10);

            Tag[] tags = Array.Empty<Tag>();
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            var assetTagsDbSetMock = Array.Empty<AssetTag>().BuildMockDbSet();
            context.AssetTags.Returns(assetTagsDbSetMock);

            // Act
            PagedData<TagDtoInfo> result = await sut.ListAsync(pagination);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(pagination.Skip, result.Skip);
            Assert.Equal(pagination.Take, result.Take);
        }

        [Fact]
        public async Task ListAsync_ReturnsTags_OrderedByName()
        {
            // Arrange
            Pagination pagination = new(0, 10);

            TagCategory category = fixture.Create<TagCategory>();

            Tag tag1 = fixture.Build<Tag>()
                .With(x => x.Name, "Blue")
                .With(x => x.CategoryId, category.Id)
                .Create();

            Tag tag2 = fixture.Build<Tag>()
                .With(x => x.Name, "Red")
                .With(x => x.CategoryId, category.Id)
                .Create();

            Tag[] tags = [tag2, tag1];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            var assetTagsDbSetMock = Array.Empty<AssetTag>().BuildMockDbSet();
            context.AssetTags.Returns(assetTagsDbSetMock);

            // Act
            PagedData<TagDtoInfo> result = await sut.ListAsync(pagination);

            // Assert
            TagDtoInfo[] resultArray = result.Items.ToArray();

            Assert.Equal(2, resultArray.Length);
            Assert.Equal(tag1.Name, resultArray[0].Name);
            Assert.Equal(tag2.Name, resultArray[1].Name);
        }

        [Fact]
        public async Task ListAsync_ReturnsTags_WithPagination()
        {
            // Arrange
            Pagination pagination = new(1, 1);

            TagCategory category = fixture.Create<TagCategory>();

            Tag tag1 = fixture.Build<Tag>()
                .With(x => x.Name, "Blue")
                .With(x => x.CategoryId, category.Id)
                .Create();

            Tag tag2 = fixture.Build<Tag>()
                .With(x => x.Name, "Red")
                .With(x => x.CategoryId, category.Id)
                .Create();

            Tag[] tags = [tag1, tag2];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            var assetTagsDbSetMock = Array.Empty<AssetTag>().BuildMockDbSet();
            context.AssetTags.Returns(assetTagsDbSetMock);

            // Act
            PagedData<TagDtoInfo> result = await sut.ListAsync(pagination);

            // Assert
            TagDtoInfo tagDto = Assert.Single(result.Items);

            Assert.Equal(tag2.Id, tagDto.Id);
            Assert.Equal(tag2.Name, tagDto.Name);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(pagination.Skip, result.Skip);
            Assert.Equal(pagination.Take, result.Take);
        }

        [Fact]
        public async Task ListAsync_ReturnsTagProperties()
        {
            // Arrange
            int categoryId = 1;
            Pagination pagination = new(0, 10);

            TagCategory category = fixture.Build<TagCategory>()
                .With(x => x.Id, categoryId)
                .With(x => x.Name, "color")
                .Create();

            Tag tag = fixture.Build<Tag>()
                .With(x => x.Name, "Red")
                .With(x => x.CategoryId, categoryId)
                .Without(x => x.CanonicalId)
                .With(x => x.Category, category)
                .Create();

            Tag[] tags = [tag];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            var assetTagsDbSetMock = Array.Empty<AssetTag>().BuildMockDbSet();
            context.AssetTags.Returns(assetTagsDbSetMock);

            // Act
            PagedData<TagDtoInfo> result = await sut.ListAsync(pagination);

            // Assert
            TagDtoInfo tagDto = Assert.Single(result.Items);

            Assert.Equal(tag.Id, tagDto.Id);
            Assert.Equal(tag.Name, tagDto.Name);
            Assert.Equal(category.Name, tagDto.Category);
            Assert.Equal(0, tagDto.Occurrences);
            Assert.Null(tagDto.CanonicalId);
            Assert.Equal("null", tagDto.CanonicalName);
        }

        #endregion
    }
}
