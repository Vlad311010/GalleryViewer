using App.Dto.Asset;
using App.Dto.Group;
using App.Enum;
using App.Exceptions;
using App.Interfaces.Services;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Extensions;
using Shared.Models;

namespace App.Services
{
    public class GroupsService(AssetsCatalogContext context, ILogger<GroupsService> logger) : IGroupService
    {
        /// <summary>
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="assets"></param>
        /// <param name="creationTimeOverride">If present will be a source of creation time. Used to override creation time of groups created of physical directoies</param>
        /// <returns></returns>
        public async Task<AssetGroupDto> CreateGroup(AssetGroupDtoCreate dto)
        {
            DateTime importTime = DateTime.UtcNow;
            DateTime creationTime = dto.CreationTimeOverride.HasValue ? dto.CreationTimeOverride.Value : importTime;

            AssetGroup entity = new AssetGroup()
            {
                CoverAssetIdx = 0,
                GalleryId = dto.GalleryId,
                Title = string.IsNullOrWhiteSpace(dto.GroupName) ? null : dto.GroupName,
                CreationTime = creationTime,
                ImportTime = importTime,
                PhysicalRelativePath = dto.PhysicalPath
            };

            entity = (await context.AssetGroups.AddAsync(entity)).Entity;
            await context.SaveChangesAsync();

            logger.Info("Created group {id}", ApplicationArea.Service, entity.Id);

            return new AssetGroupDto(entity.Id, entity.GalleryId, entity.CoverAssetIdx, entity.Title, entity.PhysicalRelativePath);
        }

        public async Task<AssetGroupDto?> GetPhysicalGroup(int galleryId, string physicalPath)
        {
            return await context.AssetGroups
                .Where(x => x.GalleryId == galleryId && x.PhysicalRelativePath == physicalPath)
                .Select(x => new AssetGroupDto(
                    x.Id,
                    x.GalleryId,
                    x.CoverAssetIdx,
                    x.Title,
                    x.PhysicalRelativePath
                )).SingleOrDefaultAsync();
        }


        public bool IsSynchronized(AssetGroupDto group, string[] files, out List<AssetSynchronizationDto> outOfSyncFiles)
        {
            var groupAssets = context.Assets
                .Where(x => x.GroupId == group.Id)
                .Select(x => new
                {
                    x.Id,
                    x.RelativePath
                })
                .ToArray();

            outOfSyncFiles = new List<AssetSynchronizationDto>();
            foreach (var relativePath in files)
            {
                if (!groupAssets.Any(x => x.RelativePath == relativePath))
                {
                    outOfSyncFiles.Add(new(SyncMismatchType.OnlyFileSystem, null, relativePath));
                }
            }

            foreach (var groupAsset in groupAssets)
            {
                if (!files.Any(x => x == groupAsset.RelativePath))
                {
                    outOfSyncFiles.Add(new(SyncMismatchType.OnlyDb, groupAsset.Id, groupAsset.RelativePath));
                }
            }

            return !outOfSyncFiles.Any();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>Last position index</returns>
        public async Task<int> StageNormalizePositionsAsync(int groupId)
        {
            AssetGroup? group = await context.AssetGroups
                .Include(x => x.Assets)
                .Where(x => x.Id == groupId)
                .SingleOrDefaultAsync();

            EntityNotFoundException<AssetGroup>.ThrowIfNull(group, groupId);


            Asset[] assets = [.. group.Assets];
            Asset previewAsset = assets
                .Single(x => x.GroupPosition!.Value == group.CoverAssetIdx);

            assets = [.. assets.OrderBy(x => x.GroupPosition)];
            for (int i = 0; i < assets.Length; i++)
            {
                assets[i].GroupPosition = i;
            }

            logger.Info("Group {id} asset positions normalized", ApplicationArea.Service, group.Id);

            group.CoverAssetIdx = previewAsset.GroupPosition!.Value;
            return assets.Length;
        }

        public async Task SetPositionsAsync(int groupId, IEnumerable<AssetPosition> positions)
        {
            AssetGroup? group = await context.AssetGroups
                .Include(x => x.Assets)
                .Where(x => x.Id == groupId)
                .SingleOrDefaultAsync();

            EntityNotFoundException<AssetGroup>.ThrowIfNull(group, groupId);

            Asset[] assets = [.. group.Assets];
            Asset previewAsset = assets
                .Single(x => x.GroupPosition!.Value == group.CoverAssetIdx);

            positions = Normalize(positions);
            Dictionary<int, int> positionsById = positions.ToDictionary(x => x.Id, x => x.Position);
            foreach (Asset asset in group.Assets)
            {
                if (positionsById.TryGetValue(asset.Id, out int position))
                {
                    asset.GroupPosition = position;
                }
            }

            group.CoverAssetIdx = previewAsset.GroupPosition!.Value;

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Asset positions updated for group {GroupId}. {AssetCount} assets reordered.",
                groupId,
                positionsById.Count);

        }

        public async Task SetCover(int groupId, int assetId)
        {
            AssetGroup? group = await context.AssetGroups.FindAsync(groupId);
            EntityNotFoundException<AssetGroup>.ThrowIfNull(group, groupId);

            Asset? asset = await context.Assets.FindAsync(assetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, assetId);

            if (asset.GroupId != group.Id)
            {
                throw new EntityAssociationException<Asset, AssetGroup>(assetId, groupId, $"Asset '{assetId}' is not associated with group '{groupId}'.");
            }

            group.CoverAssetIdx = asset.GroupPosition!.Value;

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Cover asset updated for group {GroupId}. Cover asset is {CoverAssetId}",
                groupId,
                asset.Id);
        }

        public async Task<int> AssetsCountAsync(int groupId)
        {
            AssetGroup? group = await context.AssetGroups
                .AsNoTracking()
                .Where(x => x.Id == groupId)
                .SingleOrDefaultAsync();

            EntityNotFoundException<AssetGroup>.ThrowIfNull(group, groupId);

            return await context.AssetGroups
                .Where(x => x.Id == groupId)
                .Select(x => x.Assets.Count)
                .SingleOrDefaultAsync();
        }

        public async Task<AssetGroupDtoWithAssetPositions> GetByIdAsync(int groupId)
        {
            AssetGroup? group = await context.AssetGroups.FindAsync(groupId);
            EntityNotFoundException<AssetGroup>.ThrowIfNull(group, groupId);

            AssetPosition[] positions = await context.Assets
                .Where(x => x.GroupId == groupId)
                .Select(x => new AssetPosition(x.Id, x.GroupPosition!.Value))
                .ToArrayAsync();

            return new AssetGroupDtoWithAssetPositions(
                group.Id,
                group.GalleryId,
                group.CoverAssetIdx,
                group.Title,
                group.PhysicalRelativePath,
                positions
            );
        }

        private static IEnumerable<AssetPosition> Normalize(IEnumerable<AssetPosition> positions)
        {
            return positions
                .OrderBy(x => x.Position)
                .Select((x, idx) => x with { Position = idx });
        }
    }
}
