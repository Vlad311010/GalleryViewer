using App.Dto.Filter;
using App.Enum;
using App.Exceptions;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace App.Services
{
    public class FilterService
    {
        private readonly AssetsCatalogContext context;

        public FilterService(AssetsCatalogContext context)
        {
            this.context = context;
        }

        public async Task<PagedData<DisplayItemDto>> ListAsync(string galleryName, PaginationDto filter, TagFiltersDto tagFilters)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(galleryName);
            ArgumentNullException.ThrowIfNull(filter);
            ArgumentNullException.ThrowIfNull(tagFilters);

            Gallery? gallery = await context.Galleries.SingleOrDefaultAsync(x => x.Name == galleryName);
            EntityNotFoundException<Gallery>.ThrowIfNull(gallery, galleryName);

            var query = context.Assets.AsQueryable()
                .Where(x => x.GalleryId == gallery.Id);

            string[] tags = tagFilters.Tags
                .Concat(tagFilters.ExcludeTags)
                .Distinct()
                .ToArray();

            Tag[] existingTags = await context.Tags
                .AsNoTracking()
                .Where(x => tags.Contains(x.Name))
                .ToArrayAsync();

            if (existingTags.Length != tags.Length)
            {
                return new(Array.Empty<DisplayItemDto>(), filter.Skip, filter.Take, 0);
            }

            int[] includeTagIds = existingTags
                .Where(x => tagFilters.Tags.Contains(x.Name) && !tagFilters.ExcludeTags.Contains(x.Name))
                .Select(x => x.Id)
                .ToArray();

            int[] excludeTagIds = existingTags
                .Where(x => tagFilters.ExcludeTags.Contains(x.Name))
                .Select(x => x.Id)
                .ToArray();


            if (includeTagIds.Length + excludeTagIds.Length > 0)
            {
                query = query.Where(asset =>
                    includeTagIds
                        .All(tagId => context.AssetTags.Any(at =>
                            at.AssetId == asset.Id &&
                            at.TagId == tagId))
                    &&
                    excludeTagIds
                        .All(tagId => !context.AssetTags.Any(at =>
                            at.AssetId == asset.Id &&
                            at.TagId == tagId)));
            }


            var displayItemKeys = query
                .OrderByDescending(x => x.CreationTime)
                .ThenBy(x => x.Id)
                .GroupBy(a => new
                {
                    IsGroup = a.GroupId.HasValue,
                    Id = a.GroupId ?? a.Id,
                })
                .Select(g => new
                {
                    g.Key.IsGroup,
                    g.Key.Id,
                    CreationTime = g.Min(x => x.CreationTime)
                });

            var pageDisplayItemKeys = await displayItemKeys
                .OrderByDescending(x => x.CreationTime)
                .Skip(filter.Skip)
                .Take(filter.Take)
                .ToListAsync();

            // TODO: move everything below to separate method ItemKeysToDisplayItems(pageDisplayItemKeys);

            // TODO:? group unfolding logic

            List<DisplayItemDto> displayItems = new List<DisplayItemDto>(pageDisplayItemKeys.Count);
            int totalCount = displayItemKeys.Count();
            foreach (var itemKey in pageDisplayItemKeys)
            {
                DisplayItemType itemType = itemKey.IsGroup ? DisplayItemType.Group : DisplayItemType.Asset;
                int id = itemKey.Id;
                string? title = null;
                int? count = null;
                DateTime creationTime = DateTime.MinValue;
                DateTime importTime = DateTime.MinValue;

                switch (itemType)
                {
                    case DisplayItemType.Asset:
                        Asset asset = context.Assets.Single(x => x.Id == id);
                        creationTime = asset.CreationTime;
                        importTime = asset.ImportTime;
                        break;

                    case DisplayItemType.Group:
                        var groupData = await context.AssetGroups // loads group data with preview path from asset
                            .Where(g => g.Id == id)
                            .Select(g => new
                            {
                                g.Id,
                                g.Title,
                                g.CreationTime,
                                g.ImportTime,
                                Count = g.Assets.Count(),
                            })
                            .SingleAsync();

                        title = groupData.Title;
                        count = groupData.Count;
                        creationTime = groupData.CreationTime;
                        importTime = groupData.ImportTime;
                        break;
                }

                displayItems.Add(new()
                {
                    Type = itemType,
                    Id = id,
                    Title = title,
                    Count = count,
                    CreationTime = creationTime,
                    ImportTime = importTime,
                });
            }

            displayItems = displayItems.OrderByDescending(x => x.CreationTime).ToList();

            return new(displayItems, filter.Skip, filter.Take, totalCount);
        }


        public async Task<PagedData<DisplayItemDto>> ListGroupAssetsAsync(int groupId, PaginationDto filter)
        {
            IQueryable<Asset> groupAssetsQuery = context.Assets
                .Where(x => x.GroupId == groupId);

            int totalItems = groupAssetsQuery.Count();

            Asset[] takenAssets = await groupAssetsQuery
                .OrderBy(x => x.GroupPosition)
                .Skip(filter.Skip)
                .Take(filter.Take)
                .ToArrayAsync();


            return new PagedData<DisplayItemDto>(
                takenAssets.Select(x => ToDisplayItemDto(x)),
                filter.Skip,
                filter.Take,
                totalItems
            );
        }

        private static DisplayItemDto ToDisplayItemDto(Asset asset)
        {
            ArgumentNullException.ThrowIfNull(asset);

            return new DisplayItemDto()
            {
                Type = DisplayItemType.Asset,
                Id = asset.Id,
                CreationTime = asset.CreationTime,
                ImportTime = asset.ImportTime,
                Title = null,
                Count = null
            };
        }
    }
}
