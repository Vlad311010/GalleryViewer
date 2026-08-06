using App.Dto;
using App.Extensions;
using Data.Entities;
using System.Security.Cryptography;

namespace App
{
    public class AssetsService(AssetsCatalogContext context)
    {
        private async Task<Asset> CreateAssetWithoutGroupAsync(AssetDtoCreate dto)
        {
            Gallery? targetGallery = context.Galleries.SingleOrDefault(x => x.Id == dto.GalleryId);
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

            string hash = await ComputeMd5HashAsync(assetFilePath);
            return new Asset
            {
                GalleryId = dto.GalleryId,
                RelativePath = dto.RelativePath,
                MimeType = dto.RelativePath.ToMimeType(),
                Hash = hash,
                CreationTime = creationTime,
                ImportTime = currentTime
            };
        }

        public async Task<AssetDto> AddAssetAsync(AssetDtoCreate dto)
        {
            Asset entity = await CreateAssetWithoutGroupAsync(dto);

            entity = (await context.Assets.AddAsync(entity)).Entity;
            return new AssetDto(
                entity.Id,
                entity.GalleryId,
                entity.RelativePath,
                entity.CreationTime,
                entity.ImportTime
            );
        }


        public async Task<AssetGroupDto> CreateGroupAsync(string? groupName)
        {
            AssetGroup entity = new AssetGroup()
            {
                CoverAssetIdx = 0,
                Title = string.IsNullOrWhiteSpace(groupName) ? null : groupName
            };

            entity = (await context.AssetGroups.AddAsync(entity)).Entity;

            return new AssetGroupDto(entity.Id, entity.CoverAssetIdx, entity.Title);
        }

        /// <summary>
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="assets"></param>
        /// <param name="creationTimeOverride">If present will be a source of creation time. Used to override creation time of groups created of physical directoies</param>
        /// <returns></returns>
        public async Task<AssetGroupDto> CreateGroupAsync(string? groupName, IList<AssetDtoCreate> assets, DateTime? creationTimeOverride = null)
        {
            DateTime importTime = DateTime.UtcNow;
            DateTime creationTime = creationTimeOverride.HasValue ? creationTimeOverride.Value : importTime;
            AssetGroup entity = new AssetGroup()
            {
                CoverAssetIdx = 0,
                Title = string.IsNullOrWhiteSpace(groupName) ? null : groupName,
                CreationTime = creationTime,
                ImportTime = importTime,
            };

            context.AssetGroups.Add(entity);
            entity = (await context.AssetGroups.AddAsync(entity)).Entity;


            for (int i = 0; i < assets.Count(); i++)
            {
                Asset assetEntity = await CreateAssetWithoutGroupAsync(assets[i]);
                assetEntity.Group = entity;
                assetEntity.GroupPosition = i;

                await context.Assets.AddAsync(assetEntity);
            }

            return new AssetGroupDto(entity.Id, entity.CoverAssetIdx, entity.Title);
        }

        private async static Task<string> ComputeMd5HashAsync(string filePath) // TODO: extract to helper
        {
            using var md5 = MD5.Create();
            await using var stream = File.OpenRead(filePath);

            byte[] hash = await md5.ComputeHashAsync(stream);

            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
