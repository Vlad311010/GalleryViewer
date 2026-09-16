using App.Enums;
using App.Exceptions;
using App.Interfaces.Services;
using App.Models.Commands;
using App.Models.Dtos.Asset;
using App.Models.Dtos.Group;
using App.Models.Queries;
using App.Validators;
using Data.Context;
using Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Extensions;
using Shared.Models;

namespace App.Services
{
    public class GroupsService(AssetsCatalogContext context, ILogger<GroupsService> logger) : IGroupService
    {
        public async Task<AssetGroupDto> CreateGroup(CreateAssetGroupCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            await new CreateAssetGroupCommandValidator().ValidateAndThrowAsync(command);

            DateTime importTime = DateTime.UtcNow;
            DateTime creationTime = command.CreationTimeOverride.HasValue ? command.CreationTimeOverride.Value : importTime;

            AssetGroup entity = new AssetGroup()
            {
                CoverAssetIdx = 0,
                GalleryId = command.GalleryId,
                Title = string.IsNullOrWhiteSpace(command.GroupName) ? null : command.GroupName,
                CreationTime = creationTime,
                ImportTime = importTime,
                PhysicalRelativePath = command.PhysicalPath
            };

            entity = (await context.AssetGroups.AddAsync(entity)).Entity;
            await context.SaveChangesAsync();

            logger.Info("Created group {id}", ApplicationArea.Service, entity.Id);

            return new AssetGroupDto(entity.Id, entity.GalleryId, entity.CoverAssetIdx, entity.Title, entity.PhysicalRelativePath);
        }

        public async Task<AssetGroupDto?> GetPhysicalGroup(PhysicalAssetGroupQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new PhysicalAssetGroupQueryValidator().ValidateAndThrowAsync(query);

            return await context.AssetGroups
                .Where(x => x.GalleryId == query.GalleryId && x.PhysicalRelativePath == query.PhysicalPath)
                .Select(x => new AssetGroupDto(
                    x.Id,
                    x.GalleryId,
                    x.CoverAssetIdx,
                    x.Title,
                    x.PhysicalRelativePath
                )).SingleOrDefaultAsync();
        }


        public bool IsSynchronized(AssetGroupSynchronizationQuery query, out List<AssetSynchronizationDto> outOfSyncFiles)
        {
            ArgumentNullException.ThrowIfNull(query);

            new AssetGroupSynchronizationQueryValidator().ValidateAndThrow(query);

            var groupAssets = context.Assets
                .Where(x => x.GroupId == query.GroupId)
                .Select(x => new
                {
                    x.Id,
                    x.RelativePath
                })
                .ToArray();


            var filePaths = query.Files.ToHashSet();
            var existingAssetPaths = groupAssets.Select(x => x.RelativePath).ToHashSet();
            outOfSyncFiles =
            [
                .. filePaths
                    .Except(existingAssetPaths)
                    .Select(path => new AssetSynchronizationDto(
                        SyncMismatchType.OnlyFileSystem,
                        null,
                        path)),

                .. groupAssets
                    .Where(x => !filePaths.Contains(x.RelativePath))
                    .Select(x => new AssetSynchronizationDto(
                        SyncMismatchType.OnlyDb,
                        x.Id,
                        x.RelativePath))
            ];

            return !outOfSyncFiles.Any();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns>Last position index</returns>
        public async Task<int> StageNormalizePositionsAsync(AssetGroupQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new AssetGroupQueryValidator().ValidateAndThrowAsync(query);

            AssetGroup? group = await context.AssetGroups
                .Include(x => x.Assets)
                .Where(x => x.Id == query.GroupId)
                .SingleOrDefaultAsync();

            EntityNotFoundException<AssetGroup>.ThrowIfNull(group, query.GroupId);


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

        public async Task SetPositionsAsync(SetAssetsPositionsCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            await new SetAssetsPositionsCommandValidator().ValidateAndThrowAsync(command);

            AssetGroup? group = await context.AssetGroups
                .Include(x => x.Assets)
                .Where(x => x.Id == command.GroupId)
                .SingleOrDefaultAsync();

            EntityNotFoundException<AssetGroup>.ThrowIfNull(group, command.GroupId);

            Asset[] assets = [.. group.Assets];
            Asset previewAsset = assets
                .Single(x => x.GroupPosition!.Value == group.CoverAssetIdx);

            IEnumerable<AssetPosition> positions = Normalize(command.Positions);
            Dictionary<int, int> positionsById = command.Positions.ToDictionary(x => x.Id, x => x.Position);
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
                command.GroupId,
                positionsById.Count);

        }

        public async Task SetCover(SetGroupCoverCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            await new SetGroupCoverCommandValidator().ValidateAndThrowAsync(command);

            AssetGroup? group = await context.AssetGroups.FindAsync(command.GroupId);
            EntityNotFoundException<AssetGroup>.ThrowIfNull(group, command.GroupId);

            Asset? asset = await context.Assets.FindAsync(command.AssetId);
            EntityNotFoundException<Asset>.ThrowIfNull(asset, command.AssetId);

            if (asset.GroupId != group.Id)
            {
                throw new EntityAssociationException<Asset, AssetGroup>(command.AssetId, command.GroupId, $"Asset '{command.AssetId}' is not associated with group '{command.GroupId}'.");
            }

            group.CoverAssetIdx = asset.GroupPosition!.Value;

            await context.SaveChangesAsync();

            logger.LogInformation(
                "Cover asset updated for group {GroupId}. Cover asset is {CoverAssetId}",
                command.GroupId,
                asset.Id);
        }

        public async Task<int> AssetsCountAsync(AssetGroupQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new AssetGroupQueryValidator().ValidateAndThrowAsync(query);

            AssetGroup? group = await context.AssetGroups
                .AsNoTracking()
                .Where(x => x.Id == query.GroupId)
                .SingleOrDefaultAsync();

            EntityNotFoundException<AssetGroup>.ThrowIfNull(group, query.GroupId);

            return await context.AssetGroups
                .Where(x => x.Id == query.GroupId)
                .Select(x => x.Assets.Count)
                .SingleOrDefaultAsync();
        }

        public async Task<AssetGroupDtoWithAssetPositions> GetByIdAsync(AssetGroupQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new AssetGroupQueryValidator().ValidateAndThrowAsync(query);

            AssetGroup? group = await context.AssetGroups.FindAsync(query.GroupId);
            EntityNotFoundException<AssetGroup>.ThrowIfNull(group, query.GroupId);

            AssetPosition[] positions = await context.Assets
                .Where(x => x.GroupId == query.GroupId)
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
