using App.Dto;
using App.Enum;
using App.Exceptions;
using App.Extensions;
using App.Utils;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace App
{
    public class AssetsService(AssetsCatalogContext context)
    {
        private static AssetDto ToAssetDto(Asset asset) // move to extensions
        {
            ArgumentNullException.ThrowIfNull(asset);

            return new AssetDto(
                asset.Id,
                asset.GalleryId,
                asset.RelativePath,
                asset.PreviewPath,
                asset.CreationTime,
                asset.ImportTime
            );
        }

        public bool TryGetByHash(string md5Hash, [NotNullWhen(true)] out AssetDto assetDto)
        {
            Asset? asset = context.Assets.FirstOrDefault(x => x.Hash == md5Hash);
            if (asset == null)
            {
                assetDto = null!;
                return false;
            }

            assetDto = ToAssetDto(asset);
            return true;
        }

        public async Task<AssetDto> CreateAssetAsync(AssetDtoCreate dto)
        {
            Gallery? targetGallery = await context.Galleries.FindAsync(dto.GalleryId);
            if (targetGallery == null)
            {
                throw new ArgumentException("TODO: custom exception");
            }

            string assetFilePath = Path.Combine(targetGallery.Path, dto.RelativePath);
            if (!File.Exists(assetFilePath))
            {
                throw new FileNotFoundException("Asset file not found", dto.RelativePath);
            }

            /// As file creation time is reseted during copy, so modified time may be better source of true of when file landed in file system.
            /// Just for safety measure smallest of two values is taken as creationTime.
            DateTime creationTime = File.GetCreationTimeUtc(assetFilePath);
            DateTime modifiedTime = File.GetLastWriteTimeUtc(assetFilePath);
            creationTime = creationTime > modifiedTime ? modifiedTime : creationTime;
            DateTime currentTime = DateTime.UtcNow;
            string hash = await Md5Hash.ComputeAsync(assetFilePath);

            Asset entity = new()
            {
                GalleryId = dto.GalleryId,
                RelativePath = dto.RelativePath,
                MimeType = dto.RelativePath.ToMimeType(),
                Hash = hash,
                CreationTime = creationTime,
                ImportTime = currentTime,
                PreviewPath = dto.PreviewPath,

                GroupId = dto.groupId,
                GroupPosition = dto.groupPosition
            };

            entity = (await context.Assets.AddAsync(entity)).Entity;
            return ToAssetDto(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            int deleted = await context.Assets
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync();

            return deleted > 0;
        }

        public async Task<int> DeleteRangeAsync(IEnumerable<int> ids)
        {
            ArgumentNullException.ThrowIfNull(ids);
            if (ids.Count() == 0)
            {
                return 0;
            }

            int deleted = await context.Assets
                .Where(x => ids.Contains(x.Id))
                .ExecuteDeleteAsync();

            return deleted;
        }


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

        public async Task<bool> Exists(int galleryId, string relativePath)
        {
            return await context.Assets
                .AnyAsync(x =>
                x.GalleryId == galleryId
                && x.RelativePath == relativePath);
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

        public record AssetSynchronizationDto(SyncMismatchType Type, int? id, string RelativePath);


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
