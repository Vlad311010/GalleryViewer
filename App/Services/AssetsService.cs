using App.Dto.Asset;
using App.Dto.Tag;
using App.Exceptions;
using App.Extensions;
using App.Mappers;
using App.Utils;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace App.Services
{
    public class AssetsService(AssetsCatalogContext context, ILogger<AssetsService> logger)
    {
        public bool TryGetByHash(string md5Hash, [NotNullWhen(true)] out AssetDto assetDto)
        {
            Asset? asset = context.Assets.FirstOrDefault(x => x.Hash == md5Hash);
            if (asset == null)
            {
                assetDto = null!;
                return false;
            }

            assetDto = asset.ToAssetDto();
            return true;
        }

        public async Task<AssetDto?> GetByPathAsync(int galleryId, string relativePath)
        {
            Asset? asset = await context.Assets
                .SingleOrDefaultAsync(x => x.GalleryId == galleryId && x.RelativePath == relativePath);

            return asset?.ToAssetDto();
        }

        public async Task<AssetGroupInfoDto?> GetAssetGroupInfo(int assetId)
        {
            Asset? asset = await context.Assets.FindAsync(assetId);

            if (asset != null && asset.GroupId.HasValue)
            {
                int[] groupAssets = await context.Assets
                    .Where(x => x.GroupId == asset.GroupId)
                    .OrderBy(x => x.GroupPosition)
                    .Select(x => x.Id)
                    .ToArrayAsync();

                return asset?.ToAssetGroupInfoDto(groupAssets);
            }
            else
            {
                return asset?.ToAssetGroupInfoDto(null);
            }
        }

        public async Task<AssetDto> CreateAssetAsync(AssetDtoCreate dto)
        {
            Gallery? targetGallery = await context.Galleries.FindAsync(dto.GalleryId);
            EntityNotFoundException<Gallery>.ThrowIfNull(targetGallery, dto.GalleryId);

            string assetFilePath = Path.Combine(targetGallery.Path, dto.RelativePath);
            if (!File.Exists(assetFilePath))
            {
                throw new MediaNotFoundException("Asset file not found", assetFilePath);
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

            logger.Info($"Asset {entity.Id} created", ApplicationArea.Service);
            return entity.ToAssetDto();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            int deleted = await context.Assets
                .Where(x => x.Id == id)
                .ExecuteDeleteAsync();

            if (deleted > 0)
            {
                logger.Info("Asset {id} deleted", ApplicationArea.Service, id);
            }

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

            logger.Info("Asset {ids} deleted", ApplicationArea.Service, string.Join(',', ids));

            return deleted;
        }

        public async Task<bool> Exists(int galleryId, string relativePath)
        {
            return await context.Assets
                .AnyAsync(x =>
                x.GalleryId == galleryId
                && x.RelativePath == relativePath);
        }


        public async Task<AssetTagsDto> GetAssetTags(int assetId)
        {
            Asset? asset = await context.Assets.FindAsync(assetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, assetId);


            int[] tagIds = await context.AssetTags
               .Where(at => at.AssetId == asset.Id)
               .Select(at => at.TagId)
               .ToArrayAsync();


            Dictionary<string, TagDtoInfo[]> assetTags = await context.Tags
                .Where(t => tagIds.Contains(t.Id))
                .Select(t => new TagDtoInfo
                {
                    Id = t.Id,
                    Name = t.Name,
                    Category = t.Category.Name,
                    CanonicalId = t.CanonicalId,
                    CanonicalName = t.Canonical != null ? t.Canonical.Name : null,
                    Occurrences = context.AssetTags.Count(at => at.TagId == t.Id || at.Tag.CanonicalId == t.Id),
                })
                .GroupBy(t => t.Category)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.OrderBy(x => x.Name).ToArray());



            return new AssetTagsDto(assetTags);
        }


        public async Task AddTags(int assetId, string[] tags)
        {
            Asset? asset = await context.Assets.FindAsync(assetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, assetId);

            tags = tags
                .Distinct()
                .ToArray();

            var existingTags = await context.Tags
                .Where(x => tags.Contains(x.Name))
                .Select(x => new { x.Id, x.Name })
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

            int[] existingTagIds = [.. existingTags.Select(x => x.Id)];

            int[] duplicatedTags = await context.AssetTags
                .Where(x => x.AssetId == assetId && existingTagIds.Contains(x.TagId))
                .Select(x => x.TagId)
                .ToArrayAsync();

            int[] tagsToAdd = [.. existingTagIds.Where(x => !duplicatedTags.Contains(x))];

            foreach (var tag in tagsToAdd)
            {
                context.AssetTags.Add(new AssetTag { AssetId = assetId, TagId = tag });
            }

            logger.Info(
                "Added {TagCount} tags to asset {AssetId}", ApplicationArea.Service,
                tagsToAdd.Count(),
                assetId);

            await context.SaveChangesAsync();
        }

        public async Task RemoveTag(int assetId, string tag)
        {
            Asset? asset = await context.Assets.FindAsync(assetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, assetId);

            Tag? tagEntity = await context.Tags
                .AsNoTracking()
                .Where(x => x.Name == tag)
                .SingleOrDefaultAsync();
            EntityNotFoundException<Tag>.ThrowIfNull(tagEntity, tag);

            context.AssetTags.Remove(new AssetTag { AssetId = assetId, TagId = tagEntity.Id });

            logger.Info(
                "Removed {TagId} tag from asset{AssetId}", ApplicationArea.Service,
                tagEntity.Id,
                assetId);

            await context.SaveChangesAsync();
        }
    }
}
