using App.Dto.Asset;
using App.Dto.Group;
using App.Enum;
using App.Exceptions;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace App
{
    public class GroupsService(AssetsCatalogContext context)
    {
        /// <summary>
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="assets"></param>
        /// <param name="creationTimeOverride">If present will be a source of creation time. Used to override creation time of groups created of physical directoies</param>
        /// <returns></returns>
        public async Task<AssetGroupDto> CreateGroupAndSaveAsync(AssetGroupDtoCreate dto)
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
        /// <returns>Llast position index</returns>
        public async Task<int> NormalizePositionAsync(int groupId)
        {
            AssetGroup group = await context.AssetGroups
                .Include(x => x.Assets)
                .Where(x => x.Id == groupId)
                .SingleAsync();

            Asset[] assets = [.. group.Assets];
            Asset? previewAsset = assets
                .Where(x => x.GroupPosition!.Value == group.CoverAssetIdx)
                .SingleOrDefault();

            assets = [.. assets.OrderBy(x => x.GroupPosition)];
            for (int i = 0; i < assets.Length; i++)
            {
                assets[i].GroupPosition = i;
            }

            group.CoverAssetIdx = previewAsset == null ? 0 : previewAsset.GroupPosition!.Value;
            return assets.Length;
        }

        public async Task<int> AssetsCount(int groupId)
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
    }
}
