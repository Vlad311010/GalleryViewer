using App.Exceptions;
using App.Interfaces.Services;
using App.Models.Commands;
using App.Models.Dtos.Gallery;
using App.Models.Queries;
using App.Validators;
using Data.Context;
using Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Extensions;

namespace App.Services
{
    public class GalleriesService(AssetsCatalogContext context, ILogger<GalleriesService> logger) : IGalleriesService
    {
        public async Task<GalleryDto> Create(GalleryCreateCommand command)
        {
            ArgumentNullException.ThrowIfNull(command);

            await new GalleryCreateCommandValidator().ValidateAndThrowAsync(command);

            Gallery entity = new Gallery
            {
                Name = command.Name.ToLower(),
                Path = command.Path
            };

            Gallery createdEntity = (await context.Galleries.AddAsync(entity)).Entity;
            await context.SaveChangesAsync();

            logger.Info(
                "Created gallery id:{id} name:{name} path:{path}", ApplicationArea.Service,
                entity.Id,
                entity.Name,
                entity.Path);

            return new GalleryDto(createdEntity.Id, createdEntity.Name, createdEntity.Path, createdEntity.CoverSourceId);
        }

        public async Task<GalleryDto?> GetByNameAsync(GalleryByNameQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            await new GalleryByNameQueryValidator().ValidateAndThrowAsync(query);

            Gallery? entity = await context.Galleries.SingleOrDefaultAsync(x => x.Name == query.GalleryName);
            return entity == null ? null : new GalleryDto(entity.Id, entity.Name, entity.Path, entity.CoverSourceId);
        }

        public async Task<IEnumerable<GalleryDto>> ListAsync()
        {
            IEnumerable<Gallery> galleries = await context.Galleries
                .OrderBy(x => x.Id)
                .ToListAsync();
            return galleries.Select(x => new GalleryDto(x.Id, x.Name, x.Path, x.CoverSourceId));
        }

        public async Task StageUpdatePreviewAssetAsync(int galleryId, int coverSourceId)
        {
            Gallery? entity = await context.Galleries.FindAsync(galleryId);
            EntityNotFoundException<Gallery>.ThrowIfNull(entity, galleryId);


            entity.CoverSourceId = coverSourceId;
        }
    }
}
