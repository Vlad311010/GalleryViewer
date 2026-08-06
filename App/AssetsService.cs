using App.Dto;
using App.Extensions;
using App.Utils;
using Data.Entities;

namespace App
{
    public class AssetsService(AssetsCatalogContext context)
    {

        public async Task<AssetDto> CreateAssetAsync(AssetDtoCreate dto)
        {
            Asset entity = await ConstructAsset(dto);

            entity = (await context.Assets.AddAsync(entity)).Entity;
            return new AssetDto(
                entity.Id,
                entity.GalleryId,
                entity.RelativePath,
                entity.PreviewPath,
                entity.CreationTime,
                entity.ImportTime
            );
        }

        public async Task<AssetDto> CreateAssetAsync(AssetDtoCreate dto, int groupId, int groupPosition)
        {
            Asset entity = await ConstructAsset(dto);
            entity.GroupId = groupId;
            entity.GroupPosition = groupPosition;

            entity = (await context.Assets.AddAsync(entity)).Entity;

            return new AssetDto(
                entity.Id,
                entity.GalleryId,
                entity.RelativePath,
                entity.PreviewPath,
                entity.CreationTime,
                entity.ImportTime
            );
        }


        /// <summary>
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="assets"></param>
        /// <param name="creationTimeOverride">If present will be a source of creation time. Used to override creation time of groups created of physical directoies</param>
        /// <returns></returns>
        public async Task<AssetGroupDto> CreateGroupAndSaveAsync(string? groupName, DateTime? creationTimeOverride = null)
        {
            DateTime importTime = DateTime.UtcNow;
            DateTime creationTime = creationTimeOverride.HasValue ? creationTimeOverride.Value : importTime;
            AssetGroup entity = new AssetGroup()
            {
                CoverAssetIdx = 0,
                Title = string.IsNullOrWhiteSpace(groupName) ? null : groupName,
                CreationTime = creationTime,
                ImportTime = importTime
            };

            entity = (await context.AssetGroups.AddAsync(entity)).Entity;
            await context.SaveChangesAsync();

            return new AssetGroupDto(entity.Id, entity.CoverAssetIdx, entity.Title);
        }

        private async Task<Asset> ConstructAsset(AssetDtoCreate dto)
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

            return new Asset
            {
                GalleryId = dto.GalleryId,
                RelativePath = dto.RelativePath,
                MimeType = dto.RelativePath.ToMimeType(),
                Hash = hash,
                CreationTime = creationTime,
                ImportTime = currentTime,
                PreviewPath = dto.PreviewPath
            };
        }
    }
}
