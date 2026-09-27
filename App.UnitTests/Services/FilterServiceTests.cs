using App.Exceptions;
using App.Models;
using App.Models.Dtos.Filter;
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
using Shared.Models;
using static App.Services.FilterService;

namespace App.UnitTests.Services
{
    public class FilterServiceTests
    {
        private readonly FilterService sut;
        private readonly AssetsCatalogContext context;
        private readonly Fixture fixture;


        public FilterServiceTests()
        {
            fixture = new TestFixture();

            context = Substitute.For<AssetsCatalogContext>();

            sut = new FilterService(context);
        }

        #region ListAsync

        [Fact]
        public async Task ListAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            // Arrange
            ListAssetsQuery queryModel = null!;

            // Act
            Func<Task> action = () => sut.ListAsync(queryModel);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task ListAsync_ThrowsValidationException_WhenInvalidQuery()
        {
            // Arrange
            ListAssetsQuery queryModel = fixture.Build<ListAssetsQuery>()
                .With(x => x.GalleryName, "")
                .Create();

            // Act
            Func<Task> action = () => sut.ListAsync(queryModel);

            // Assert
            await Assert.ThrowsAsync<ValidationException>(action);
        }

        [Fact]
        public async Task ListAsync_ThrowsEntityNotFoundException_WhenNonExistingGallery()
        {
            // Arrange
            ListAssetsQuery queryModel = CreateValidListAssetsQuery();

            var galleriesDbSetMock = Array.Empty<Gallery>().BuildMockDbSet();
            context.Galleries.Returns(galleriesDbSetMock);

            // Act
            Func<Task> action = () => sut.ListAsync(queryModel);

            // Assert
            await Assert.ThrowsAnyAsync<EntityNotFoundException>(action);
        }

        [Fact]
        public async Task ListAsync_ReturnsEmptyPage_WhenNoAssetsExist()
        {
            // Arrange
            string galleryName = "gallery";
            ListAssetsQuery queryModel = CreateValidListAssetsQuery();

            Gallery gallery = fixture.Build<Gallery>()
                .With(x => x.Name, galleryName)
                .Create();

            Gallery[] galleries = [gallery];
            var galleriesDbSetMock = galleries.BuildMockDbSet();
            context.Galleries.Returns(galleriesDbSetMock);

            var assetsDbSetMock = Array.Empty<Asset>().BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            var tagsDbSetMock = Array.Empty<Tag>().BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            var groupsDbSetMock = Array.Empty<AssetGroup>().BuildMockDbSet();
            context.AssetGroups.Returns(groupsDbSetMock);


            // Act
            PagedData<DisplayItemDto> result = await sut.ListAsync(queryModel);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(queryModel.Pagination.Skip, result.Skip);
            Assert.Equal(queryModel.Pagination.Take, result.Take);
        }

        [Fact]
        public async Task ListAsync_ReturnsEmptyPage_WhenUnknownTagsAreProvided()
        {
            // Arrange
            string galleryName = "gallery";
            ListAssetsQuery queryModel = CreateValidListAssetsQuery();

            Gallery gallery = fixture.Build<Gallery>()
                .With(x => x.Name, galleryName)
                .Create();

            Gallery[] galleries = [gallery];
            var galleriesDbSetMock = galleries.BuildMockDbSet();
            context.Galleries.Returns(galleriesDbSetMock);

            var assetsDbSetMock = Array.Empty<Asset>().BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            var tagsDbSetMock = Array.Empty<Tag>().BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            var groupsDbSetMock = Array.Empty<AssetGroup>().BuildMockDbSet();
            context.AssetGroups.Returns(groupsDbSetMock);

            // Act
            PagedData<DisplayItemDto> result = await sut.ListAsync(queryModel);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(queryModel.Pagination.Skip, result.Skip);
            Assert.Equal(queryModel.Pagination.Take, result.Take);
        }

        #endregion

        #region ResolveTagFiltersAsync

        [Fact]
        public async Task ResolveTagFiltersAsync_ReturnsEmptyTagIds_WhenNoFiltersProvided()
        {
            // Arrange
            TagFilters tagFilters = new()
            {
                Tags = Array.Empty<string>(),
                ExcludeTags = Array.Empty<string>()
            };

            Tag[] tags = Array.Empty<Tag>();
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            ResolvedTags result = await sut.ResolveTagFiltersAsync(tagFilters);

            // Assert
            Assert.Empty(result.IncludeTagIds);
            Assert.Empty(result.ExcludeTagIds);
        }

        [Fact]
        public async Task ResolveTagFiltersAsync_ReturnsIncludeTagIds()
        {
            // Arrange
            string tagName = "red";

            TagFilters tagFilters = new()
            {
                Tags = [tagName],
                ExcludeTags = Array.Empty<string>()
            };

            Tag tag = fixture.Build<Tag>()
                .With(x => x.Name, tagName)
                .Create();

            Tag[] tags = [tag];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            ResolvedTags result = await sut.ResolveTagFiltersAsync(tagFilters);

            // Assert
            int tagId = Assert.Single(result.IncludeTagIds);
            Assert.Equal(tag.Id, tagId);
            Assert.Empty(result.ExcludeTagIds);
        }

        [Fact]
        public async Task ResolveTagFiltersAsync_ReturnsExcludeTagIds()
        {
            // Arrange
            string tagName = "red";

            TagFilters tagFilters = new()
            {
                Tags = Array.Empty<string>(),
                ExcludeTags = [tagName]
            };

            Tag tag = fixture.Build<Tag>()
                .With(x => x.Name, tagName)
                .Create();

            Tag[] tags = [tag];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            ResolvedTags result = await sut.ResolveTagFiltersAsync(tagFilters);

            // Assert
            int tagId = Assert.Single(result.ExcludeTagIds);
            Assert.Equal(tag.Id, tagId);
            Assert.Empty(result.IncludeTagIds);
        }

        [Fact]
        public async Task ResolveTagFiltersAsync_ReturnsIncludeAndExcludeTagIds()
        {
            // Arrange
            string includeTagName = "red";
            string excludeTagName = "blue";

            TagFilters tagFilters = new()
            {
                Tags = [includeTagName],
                ExcludeTags = [excludeTagName]
            };

            Tag includeTag = fixture.Build<Tag>()
                .With(x => x.Name, includeTagName)
                .Create();

            Tag excludeTag = fixture.Build<Tag>()
                .With(x => x.Name, excludeTagName)
                .Create();

            Tag[] tags = [includeTag, excludeTag];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            ResolvedTags result = await sut.ResolveTagFiltersAsync(tagFilters);

            // Assert
            int includeTagId = Assert.Single(result.IncludeTagIds);
            int excludeTagId = Assert.Single(result.ExcludeTagIds);

            Assert.Equal(includeTag.Id, includeTagId);
            Assert.Equal(excludeTag.Id, excludeTagId);
        }

        [Fact]
        public async Task ResolveTagFiltersAsync_ThrowsUnknownTagsException_WhenNonExistingTag()
        {
            // Arrange
            string tagName = "red";

            TagFilters tagFilters = new()
            {
                Tags = [tagName],
                ExcludeTags = Array.Empty<string>()
            };

            Tag[] tags = Array.Empty<Tag>();
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            Func<Task> action = () => sut.ResolveTagFiltersAsync(tagFilters);

            // Assert
            var exception = await Assert.ThrowsAsync<UnknownTagsException>(action);
            Assert.Contains(tagName, exception.InvalidTags);
        }

        [Fact]
        public async Task ResolveTagFiltersAsync_DoesNotIncludeTagId_WhenTagIsBothIncludedAndExcluded()
        {
            // Arrange
            string tagName = "red";

            TagFilters tagFilters = new()
            {
                Tags = [tagName],
                ExcludeTags = [tagName]
            };

            Tag tag = fixture.Build<Tag>()
                .With(x => x.Name, tagName)
                .Create();

            Tag[] tags = [tag];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            ResolvedTags result = await sut.ResolveTagFiltersAsync(tagFilters);

            // Assert
            Assert.Empty(result.IncludeTagIds);

            int excludeTagId = Assert.Single(result.ExcludeTagIds);
            Assert.Equal(tag.Id, excludeTagId);
        }

        [Fact]
        public async Task ResolveTagFiltersAsync_DeduplicatesTags()
        {
            // Arrange
            string tagName = "red";

            TagFilters tagFilters = new()
            {
                Tags = [tagName, tagName],
                ExcludeTags = Array.Empty<string>()
            };

            Tag tag = fixture.Build<Tag>()
                .With(x => x.Name, tagName)
                .Create();

            Tag[] tags = [tag];
            var tagsDbSetMock = tags.BuildMockDbSet();
            context.Tags.Returns(tagsDbSetMock);

            // Act
            ResolvedTags result = await sut.ResolveTagFiltersAsync(tagFilters);

            // Assert
            int tagId = Assert.Single(result.IncludeTagIds);
            Assert.Equal(tag.Id, tagId);
        }

        #endregion

        #region ApplyTagFilters

        [Fact]
        public void ApplyTagFilters_ReturnsOriginalQuery_WhenNoFiltersProvided()
        {
            // Arrange
            Asset[] assets = [.. fixture.CreateMany<Asset>(2)];

            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            ResolvedTags tags = new(Array.Empty<int>(), Array.Empty<int>());

            // Act
            IQueryable<Asset> result = sut.ApplyTagFilters(context.Assets, tags);

            // Assert
            Assert.Equal(assets.Length, result.Count());
        }

        [Fact]
        public void ApplyTagFilters_ReturnsAssets_WithAllIncludedTags()
        {
            // Arrange
            int includedTagId = 10;
            int otherTagId = 20;

            Asset matchingAsset = fixture.Create<Asset>();
            Asset nonMatchingAsset = fixture.Create<Asset>();

            AssetTag[] assetTags =
            [
                fixture.Build<AssetTag>()
                    .With(x => x.AssetId, matchingAsset.Id)
                    .With(x => x.TagId, includedTagId)
                    .Create(),

                fixture.Build<AssetTag>()
                    .With(x => x.AssetId, matchingAsset.Id)
                    .With(x => x.TagId, otherTagId)
                    .Create(),

                fixture.Build<AssetTag>()
                    .With(x => x.AssetId, nonMatchingAsset.Id)
                    .With(x => x.TagId, otherTagId)
                    .Create(),
            ];

            Asset[] assets = [matchingAsset, nonMatchingAsset];

            var assetsDbSetMock = assets.BuildMockDbSet();
            var assetTagsDbSetMock = assetTags.BuildMockDbSet();

            context.Assets.Returns(assetsDbSetMock);
            context.AssetTags.Returns(assetTagsDbSetMock);

            ResolvedTags tags = new([includedTagId], Array.Empty<int>());

            // Act
            Asset[] result = sut.ApplyTagFilters(context.Assets, tags).ToArray();

            // Assert
            Asset asset = Assert.Single(result);
            Assert.Equal(matchingAsset.Id, asset.Id);
        }

        [Fact]
        public void ApplyTagFilters_ReturnsAssets_WithoutExcludedTags()
        {
            // Arrange
            int excludedTagId = 10;

            Asset matchingAsset = fixture.Create<Asset>();
            Asset excludedAsset = fixture.Create<Asset>();

            AssetTag[] assetTags = [
                    fixture.Build<AssetTag>()
                        .With(x => x.AssetId, excludedAsset.Id)
                        .With(x => x.TagId, excludedTagId)
                        .Create()
                ];

            Asset[] assets = [matchingAsset, excludedAsset];

            var assetsDbSetMock = assets.BuildMockDbSet();
            var assetTagsDbSetMock = assetTags.BuildMockDbSet();

            context.Assets.Returns(assetsDbSetMock);
            context.AssetTags.Returns(assetTagsDbSetMock);

            ResolvedTags tags = new(Array.Empty<int>(), [excludedTagId]);

            // Act
            Asset[] result = sut.ApplyTagFilters(context.Assets, tags).ToArray();

            // Assert
            Asset asset = Assert.Single(result);
            Assert.Equal(matchingAsset.Id, asset.Id);
        }

        [Fact]
        public void ApplyTagFilters_ReturnsAssets_MatchingIncludedAndExcludedTags()
        {
            // Arrange
            int includedTagId = 10;
            int excludedTagId = 20;

            Asset matchingAsset = fixture.Create<Asset>();
            Asset excludedAsset = fixture.Create<Asset>();
            Asset missingIncludedTagAsset = fixture.Create<Asset>();

            AssetTag[] assetTags =
            [
                fixture.Build<AssetTag>()
                    .With(x => x.AssetId, matchingAsset.Id)
                    .With(x => x.TagId, includedTagId)
                    .Create(),

                fixture.Build<AssetTag>()
                    .With(x => x.AssetId, excludedAsset.Id)
                    .With(x => x.TagId, includedTagId)
                    .Create(),

                fixture.Build<AssetTag>()
                    .With(x => x.AssetId, excludedAsset.Id)
                    .With(x => x.TagId, excludedTagId)
                    .Create(),

                fixture.Build<AssetTag>()
                    .With(x => x.AssetId, missingIncludedTagAsset.Id)
                    .With(x => x.TagId, excludedTagId)
                    .Create(),
            ];

            Asset[] assets = [matchingAsset, excludedAsset, missingIncludedTagAsset];

            var assetsDbSetMock = assets.BuildMockDbSet();
            var assetTagsDbSetMock = assetTags.BuildMockDbSet();

            context.Assets.Returns(assetsDbSetMock);
            context.AssetTags.Returns(assetTagsDbSetMock);

            ResolvedTags tags = new([includedTagId], [excludedTagId]);

            // Act
            Asset[] result = sut.ApplyTagFilters(context.Assets, tags).ToArray();

            // Assert
            Asset asset = Assert.Single(result);
            Assert.Equal(matchingAsset.Id, asset.Id);
        }

        #endregion

        #region ListGroupAssetsAsync

        [Fact]
        public async Task ListGroupAssetsAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            // Arrange
            ListGroupAssetsQuery query = null!;

            // Act
            Func<Task> action = () => sut.ListGroupAssetsAsync(query);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public async Task ListGroupAssetsAsync_ThrowsValidationException_WhenInvalidQuery()
        {
            // Arrange
            ListGroupAssetsQuery query = CreateValidListGroupAssetsQuery(0);

            // Act
            Func<Task> action = () => sut.ListGroupAssetsAsync(query);

            // Assert
            var exception = await Assert.ThrowsAsync<ValidationException>(action);
            exception.AssertSingleError(nameof(ListGroupAssetsQuery.GroupId));
        }

        [Fact]
        public async Task ListGroupAssetsAsync_ReturnsEmptyPage_WhenNoGroupAssetsExist()
        {
            // Arrange
            int groupId = 23;
            ListGroupAssetsQuery query = CreateValidListGroupAssetsQuery(groupId);

            Asset[] assets = Array.Empty<Asset>();
            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            PagedData<DisplayItemDto> result = await sut.ListGroupAssetsAsync(query);

            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(query.Pagination.Skip, result.Skip);
            Assert.Equal(query.Pagination.Take, result.Take);
        }

        [Fact]
        public async Task ListGroupAssetsAsync_ReturnsOnlyAssetsFromGroup()
        {
            // Arrange
            int groupId = 23;

            ListGroupAssetsQuery query = CreateValidListGroupAssetsQuery(groupId);

            Asset groupAsset = fixture.Build<Asset>()
                .With(x => x.GroupId, groupId)
                .Create();

            Asset otherGroupAsset = fixture.Build<Asset>()
                .With(x => x.GroupId, groupId + 1)
                .Create();

            Asset[] assets = [groupAsset, otherGroupAsset];
            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            PagedData<DisplayItemDto> result = await sut.ListGroupAssetsAsync(query);

            // Assert
            DisplayItemDto displayItem = Assert.Single(result.Items);

            Assert.Equal(groupAsset.Id, displayItem.Id);
            Assert.Equal(1, result.TotalCount);
        }

        [Fact]
        public async Task ListGroupAssetsAsync_ReturnsAssets_OrderedByGroupPosition()
        {
            // Arrange
            int groupId = 23;

            ListGroupAssetsQuery query = CreateValidListGroupAssetsQuery(groupId);

            Asset firstAsset = fixture.Build<Asset>()
                .With(x => x.GroupId, groupId)
                .With(x => x.GroupPosition, 1)
                .Create();

            Asset secondAsset = fixture.Build<Asset>()
                .With(x => x.GroupId, groupId)
                .With(x => x.GroupPosition, 2)
                .Create();

            Asset[] assets = [secondAsset, firstAsset];
            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            PagedData<DisplayItemDto> result = await sut.ListGroupAssetsAsync(query);

            // Assert
            DisplayItemDto[] resultArray = result.Items.ToArray();

            Assert.Equal(2, resultArray.Length);
            Assert.Equal(firstAsset.Id, resultArray[0].Id);
            Assert.Equal(secondAsset.Id, resultArray[1].Id);
        }

        [Fact]
        public async Task ListGroupAssetsAsync_ReturnsAssets_WithPagination()
        {
            // Arrange
            int groupId = 23;

            ListGroupAssetsQuery query = CreateValidListGroupAssetsQuery(groupId, 1, 1);

            Asset firstAsset = fixture.Build<Asset>()
                .With(x => x.GroupId, groupId)
                .With(x => x.GroupPosition, 1)
                .Create();

            Asset secondAsset = fixture.Build<Asset>()
                .With(x => x.GroupId, groupId)
                .With(x => x.GroupPosition, 2)
                .Create();

            Asset[] assets = [firstAsset, secondAsset];
            var assetsDbSetMock = assets.BuildMockDbSet();
            context.Assets.Returns(assetsDbSetMock);

            // Act
            PagedData<DisplayItemDto> result = await sut.ListGroupAssetsAsync(query);

            // Assert
            DisplayItemDto displayItem = Assert.Single(result.Items);

            Assert.Equal(secondAsset.Id, displayItem.Id);
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(query.Pagination.Skip, result.Skip);
            Assert.Equal(query.Pagination.Take, result.Take);
        }

        #endregion

        private static ListAssetsQuery CreateValidListAssetsQuery(
            string galleryName = "gallery",
            int skip = 0,
            int take = 10,
            string[]? includeTags = null,
            string[]? excludeTags = null)
        {
            return new ListAssetsQuery(
                galleryName,
                new Pagination(skip, take),
                new TagFilters
                {
                    Tags = includeTags ?? Array.Empty<string>(),
                    ExcludeTags = excludeTags ?? Array.Empty<string>()
                });
        }

        private static ListGroupAssetsQuery CreateValidListGroupAssetsQuery(
            int groupId,
            int skip = 0,
            int take = 10)
        {
            return new ListGroupAssetsQuery(groupId, new Pagination(skip, take));
        }
    }
}
