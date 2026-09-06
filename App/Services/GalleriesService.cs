using App.Dto.Gallery;
using App.Exceptions;
using App.Interfaces.Services;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Enums;
using Shared.Extensions;

namespace App.Services
{
    public class GalleriesService(AssetsCatalogContext context, ILogger<GalleriesService> logger) : IGalleriesService
    {
        public async Task<GalleryDto> CreateAndSaveAsync(GalleryDtoCreate dto)
        {
            Gallery entity = new Gallery
            {
                Name = dto.Name.ToLower(),
                Path = dto.Path
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

        public async Task<GalleryDto?> GetByNameAsync(string galleryName)
        {
            Gallery? entity = await context.Galleries.SingleOrDefaultAsync(x => x.Name == galleryName);
            return entity == null ? null : new GalleryDto(entity.Id, entity.Name, entity.Path, entity.CoverSourceId);
        }

        public async Task<IEnumerable<GalleryDto>> ListAsync()
        {
            IEnumerable<Gallery> galleries = await context.Galleries
                .OrderBy(x => x.Id)
                .ToListAsync();
            return galleries.Select(x => new GalleryDto(x.Id, x.Name, x.Path, x.CoverSourceId));
        }

        public async Task SetPreviewAssetAsync(int galleryId, int coverSourceId)
        {
            Gallery? entity = await context.Galleries.FindAsync(galleryId);
            EntityNotFoundException<Gallery>.ThrowIfNull(entity, galleryId);


            entity.CoverSourceId = coverSourceId;
            await context.SaveChangesAsync();
        }
    }
}
