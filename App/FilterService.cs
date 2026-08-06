using App.Dto;
using App.Enum;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace App
{
    public class FilterService
    {
        private readonly AssetsCatalogContext context;

        public FilterService(AssetsCatalogContext context)
        {
            this.context = context;
        }

        public async Task<List<DisplayItemDto>> ListAsync(FilterDto filter)
        {
            // 1, 2, 3, [4,5,6], 7 | Take 5

            var displayItemKeys = context.Assets
                // .Where(...) // filtering
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
            foreach (var itemKey in pageDisplayItemKeys)
            {
                DisplayItemType itemType = itemKey.IsGroup ? DisplayItemType.Group : DisplayItemType.Asset; // TODO: extract to helper 
                int id = itemKey.Id;
                string? previewPath = null;
                string? title = null;
                int? count = null;
                DateTime creationTime = DateTime.MinValue;
                DateTime importTime = DateTime.MinValue;

                switch (itemType)
                {
                    case DisplayItemType.Asset:
                        Asset asset = context.Assets.Single(x => x.Id == id);
                        previewPath = asset.PreviewPath;
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
                                PreviewPath = g.Assets
                                    .Where(a => a.GroupPosition == g.CoverAssetIdx)
                                    .Select(a => a.PreviewPath)
                                    .SingleOrDefault()
                            })
                            .SingleAsync();

                        previewPath = groupData.PreviewPath;
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
                    PreviewPath = previewPath,
                    Title = title,
                    Count = count,
                    CreationTime = creationTime,
                    ImportTime = importTime,
                });
            }

            return displayItems;
        }
    }
}
