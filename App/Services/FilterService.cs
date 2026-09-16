using App.Enums;
using App.Exceptions;
using App.Interfaces.Services;
using App.Models;
using App.Models.Dtos.Filter;
using App.Models.Queries;
using App.Validators;
using Data.Context;
using Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace App.Services
{
    public class FilterService(AssetsCatalogContext context) : IAssetsFilterService
    {
        public async Task<PagedData<DisplayItemDto>> ListAsync(ListAssetsQuery queryModel)
        {
            ArgumentNullException.ThrowIfNull(queryModel);

            await new ListAssetsQueryValidator().ValidateAndThrowAsync(queryModel);


            Gallery? gallery = await context.Galleries.SingleOrDefaultAsync(x => x.Name == queryModel.GalleryName);
            EntityNotFoundException<Gallery>.ThrowIfNull(gallery, queryModel.GalleryName);

            var query = context.Assets.AsQueryable()
                .Where(x => x.GalleryId == gallery.Id);


            ResolvedTags tags;
            try
            {
                tags = await ResolveTagFiltersAsync(queryModel.TagFilters);
            }
            catch (UnknownTagsException)
            {
                return new(Array.Empty<DisplayItemDto>(), queryModel.Pagination.Skip, queryModel.Pagination.Take, 0);
            }

            query = ApplyTagFilters(query, tags);

            var displayItemKeys = query
                .GroupBy(a => new
                {
                    IsGroup = a.GroupId.HasValue,
                    Id = a.GroupId ?? a.Id,
                })
                .Select(g => new
                {
                    g.Key.Id,
                    g.Key.IsGroup,
                    CreationTime = g.Min(x => x.CreationTime)
                });


            var pageDisplayItemKeys = await displayItemKeys
                .OrderByDescending(x => x.CreationTime)
                .ThenBy(x => x.Id)
                .Skip(queryModel.Pagination.Skip)
                .Take(queryModel.Pagination.Take)
                .Select(x => new DisplayItemKey(x.Id, x.IsGroup, x.CreationTime))
                .ToListAsync();

            List<DisplayItemDto> displayItems = await ResolveDisplayItemsAsync(pageDisplayItemKeys);

            return new(displayItems, queryModel.Pagination.Skip, queryModel.Pagination.Take, await displayItemKeys.CountAsync());
        }


        public async Task<PagedData<DisplayItemDto>> ListGroupAssetsAsync(ListGroupAssetsQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new ListGroupAssetsQueryValidator().ValidateAndThrowAsync(query);

            IQueryable<Asset> groupAssetsQuery = context.Assets
                .Where(x => x.GroupId == query.GroupId);

            int totalItems = groupAssetsQuery.Count();

            Asset[] takenAssets = await groupAssetsQuery
                .OrderBy(x => x.GroupPosition)
                .Skip(query.Pagination.Skip)
                .Take(query.Pagination.Take)
                .ToArrayAsync();


            return new PagedData<DisplayItemDto>(
                takenAssets.Select(x => ToDisplayItemDto(x)),
                query.Pagination.Skip,
                query.Pagination.Take,
                totalItems
            );
        }

        private async Task<ResolvedTags> ResolveTagFiltersAsync(TagFilters tagFilters)
        {
            string[] tags = tagFilters.Tags
                .Concat(tagFilters.ExcludeTags)
                .Distinct()
                .ToArray();

            Tag[] existingTags = await context.Tags
                .AsNoTracking()
                .Where(x => tags.Contains(x.Name))
                .ToArrayAsync();

            string[] invalidTags = tags
                .Except(
                    existingTags
                    .Select(x => x.Name))
                .ToArray();

            if (invalidTags.Length > 0)
            {
                throw new UnknownTagsException("One or more tags are undefined", invalidTags);
            }

            int[] includeTagIds = existingTags
                .Where(x => tagFilters.Tags.Contains(x.Name) && !tagFilters.ExcludeTags.Contains(x.Name))
                .Select(x => x.Id)
                .ToArray();

            int[] excludeTagIds = existingTags
                .Where(x => tagFilters.ExcludeTags.Contains(x.Name))
                .Select(x => x.Id)
                .ToArray();

            return new(includeTagIds, excludeTagIds);
        }

        private IQueryable<Asset> ApplyTagFilters(IQueryable<Asset> query, ResolvedTags tags)
        {
            if (tags.IncludeTagIds.Length + tags.ExcludeTagIds.Length > 0)
            {
                query = query.Where(asset =>
                    tags.IncludeTagIds
                        .All(tagId => context.AssetTags.Any(at =>
                            at.AssetId == asset.Id &&
                            at.TagId == tagId))
                    &&
                    tags.ExcludeTagIds
                        .All(tagId => !context.AssetTags.Any(at =>
                            at.AssetId == asset.Id &&
                            at.TagId == tagId)));
            }

            return query;
        }

        private async Task<List<DisplayItemDto>> ResolveDisplayItemsAsync(IReadOnlyCollection<DisplayItemKey> keys)
        {
            int[] assetIds = keys
                .Where(x => !x.IsGroup)
                .Select(x => x.Id)
                .ToArray();

            int[] groupIds = keys
                .Where(x => x.IsGroup)
                .Select(x => x.Id)
                .ToArray();

            var assets = await context.Assets
                .Where(x => assetIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.CreationTime,
                    x.ImportTime,
                })
                .ToDictionaryAsync(x => x.Id);

            var groups = await context.AssetGroups
                .Where(x => groupIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.Title,
                    x.CreationTime,
                    x.ImportTime,
                    Count = x.Assets.Count(),
                })
                .ToDictionaryAsync(x => x.Id);


            List<DisplayItemDto> displayItems = new List<DisplayItemDto>(keys.Count);
            foreach (var itemKey in keys)
            {
                if (itemKey.IsGroup)
                {
                    var group = groups[itemKey.Id];

                    displayItems.Add(new DisplayItemDto
                    {
                        Type = DisplayItemType.Group,
                        Id = group.Id,
                        Title = group.Title,
                        Count = group.Count,
                        CreationTime = group.CreationTime,
                        ImportTime = group.ImportTime,
                    });
                }
                else
                {
                    var asset = assets[itemKey.Id];

                    displayItems.Add(new DisplayItemDto
                    {
                        Type = DisplayItemType.Asset,
                        Id = asset.Id,
                        CreationTime = asset.CreationTime,
                        ImportTime = asset.ImportTime,
                    });
                }
            }

            // TODO:? group unfolding logic

            return [.. displayItems.OrderByDescending(x => x.CreationTime)];
        }

        private record DisplayItemKey(int Id, bool IsGroup, DateTime CreationTime);

        private record ResolvedTags(
            int[] IncludeTagIds,
            int[] ExcludeTagIds
        );

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
