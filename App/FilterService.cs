using App.Dto.Filter;
using App.Enum;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace App
{
    public class FilterService
    {
        private readonly AssetsCatalogContext context;

        public FilterService(AssetsCatalogContext context)
        {
            this.context = context;
        }

        public async Task<PagedData<DisplayItemDto>> ListAsync(BaseFilterDto filter)
        {
            var displayItemKeys = context.Assets
                // .Where(...) // filtering
                .OrderBy(x => x.Id)
                .GroupBy(a => new
                {
                    IsGroup = a.GroupId.HasValue,
                    Id = a.GroupId ?? a.Id
                })
                .Select(g => new
                {
                    g.Key.IsGroup,
                    g.Key.Id
                });

            var pageDisplayItemKeys = await displayItemKeys
                .Skip(filter.Skip)
                .Take(filter.Take)
                .ToListAsync();


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

            return new(displayItems, filter.Skip, filter.Take, totalCount);
        }


        public async Task<PagedData<DisplayItemDto>> ListGroupAssetsAsync(int groupId, BaseFilterDto filter)
        {
            IQueryable<Asset> groupAssets = context.Assets
                .Where(x => x.GroupId == groupId);
            // .ToArrayAsync();

            int totalItems = groupAssets.Count();

            Asset[] takenAssets = await groupAssets
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

        private DisplayItemDto ToDisplayItemDto(Asset asset)
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
