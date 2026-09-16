using App.Commands;
using App.Exceptions;
using App.Extensions;
using App.Interfaces.Services;
using App.Mappers;
using App.Models.Commands;
using App.Models.Dtos.Asset;
using App.Models.Dtos.Tag;
using App.Models.Queries;
using App.Utils;
using App.Validators;
using Data.Context;
using Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace App.Services
{
    public class AssetsService(AssetsCatalogContext context, IMediaAccessorService mediaAccessorService, ILogger<AssetsService> logger) : IAssetService
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

        public async Task<AssetGroupInfoDto?> GetAssetGroupInfo(AssetGroupInfoQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new AssetGroupInfoQueryValidator().ValidateAndThrowAsync(query);

            Asset? asset = await context.Assets.FindAsync(query.AssetId);

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

        public async Task<AssetDto> StageCreateAssetAsync(AssetCreateCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            await new AssetCreateCommandValidator().ValidateAndThrowAsync(command);

            Gallery? targetGallery = await context.Galleries.FindAsync(command.GalleryId);
            EntityNotFoundException<Gallery>.ThrowIfNull(targetGallery, command.GalleryId);

            string assetFilePath = Path.Combine(targetGallery.Path, command.RelativePath);
            if (!mediaAccessorService.Exists(assetFilePath))
            {
                throw new MediaNotFoundException("Asset file not found", assetFilePath);
            }

            /// As file creation time is reseted during copy, so modified time may be better source of true of when file landed in file system.
            DateTime modifiedTime = mediaAccessorService.GetLastModifiedTime(assetFilePath);
            DateTime currentTime = DateTime.UtcNow;

            using Stream assetData = mediaAccessorService.GetMediaData(assetFilePath);
            string hash = await Md5Hash.ComputeAsync(assetData);
            Asset entity = new()
            {
                GalleryId = command.GalleryId,
                RelativePath = command.RelativePath,
                MimeType = command.RelativePath.ToMimeType(),
                Hash = hash,
                CreationTime = modifiedTime,
                ImportTime = currentTime,
                PreviewPath = command.PreviewPath,

                GroupId = command.GroupId,
                GroupPosition = command.GroupPosition
            };

            entity = (await context.Assets.AddAsync(entity)).Entity;

            logger.Info($"Created asset for {assetFilePath}", ApplicationArea.Service);
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

        public async Task<bool> ExistsAsync(int galleryId, string relativePath)
        {
            return await context.Assets
                .AnyAsync(x =>
                    x.GalleryId == galleryId
                    && x.RelativePath == relativePath
                );
        }


        public async Task<AssetTagsDto> GetAssetTags(AssetTagsQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new AssetTagsQueryValidator().ValidateAndThrowAsync(query);

            Asset? asset = await context.Assets.FindAsync(query.AssetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, query.AssetId);


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


        public async Task AddTags(AssetAddTagsCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            await new AssetAddTagsCommandValidator().ValidateAndThrowAsync(command);

            Asset? asset = await context.Assets.FindAsync(command.AssetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, command.AssetId);

            string[] tags = command.Tags
                .Distinct()
                .ToArray();

            var definedTags = await context.Tags
                .Where(x => tags.Contains(x.Name))
                .Select(x => new { x.Id, x.Name })
                .ToArrayAsync();

            string[] invalidTags = tags
                .Except(
                    definedTags
                    .Select(x => x.Name))
                .ToArray();

            if (invalidTags.Length > 0)
            {
                throw new UnknownTagsException("One or more tags are undefined", invalidTags);
            }

            int[] definedTagIds = [.. definedTags.Select(x => x.Id)];

            int[] duplicatedTags = await context.AssetTags
                .Where(x => x.AssetId == command.AssetId && definedTagIds.Contains(x.TagId))
                .Select(x => x.TagId)
                .ToArrayAsync();

            int[] tagsToAdd = [.. definedTagIds.Where(x => !duplicatedTags.Contains(x))];

            foreach (var tag in tagsToAdd)
            {
                context.AssetTags.Add(new AssetTag { AssetId = command.AssetId, TagId = tag });
            }

            logger.Info(
                "Added {TagCount} tags to asset {AssetId}", ApplicationArea.Service,
                tagsToAdd.Count(),
                command.AssetId);

            await context.SaveChangesAsync();
        }

        public async Task RemoveTag(AssetRemoveTagCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            await new AssetRemoveTagCommandValidator().ValidateAndThrowAsync(command);

            Asset? asset = await context.Assets.FindAsync(command.AssetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, command.AssetId);

            var tag = command.Tag.ToLowerInvariant();
            Tag? tagEntity = await context.Tags
                .AsNoTracking()
                .Where(x => x.Name == tag)
                .SingleOrDefaultAsync();
            EntityNotFoundException<Tag>.ThrowIfNull(tagEntity, tag);

            context.AssetTags.Remove(new AssetTag { AssetId = command.AssetId, TagId = tagEntity.Id });

            logger.Info(
                "Removed {TagId} tag from asset{AssetId}", ApplicationArea.Service,
                tagEntity.Id,
                command.AssetId);

            await context.SaveChangesAsync();
        }
    }
}
