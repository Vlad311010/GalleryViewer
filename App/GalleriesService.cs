using App.Dto;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace App
{
    public class GalleriesService(AssetsCatalogContext context)
    {
        public async Task<GalleryDto> CreateAndSaveAsync(GalleryDtoCreate dto)
        {
            Gallery entity = new Gallery
            {
                Name = dto.Name,
                Path = dto.Path
            };


            Gallery createdEntity = (await context.Galleries.AddAsync(entity)).Entity;
            await context.SaveChangesAsync();

            return new GalleryDto(createdEntity.Id, createdEntity.Name, createdEntity.Path);
        }

        public async Task<GalleryDto> GetByNameAsync(string galleryName)
        {
            Gallery? entity = await context.Galleries.SingleOrDefaultAsync(x => x.Name == galleryName);
            if (entity == null)
            {
                throw new Exception("TODO: not found exception");
            }

            return new GalleryDto(entity.Id, entity.Name, entity.Path);
        }

        public async Task<IEnumerable<GalleryDto>> ListAsync()
        {
            IEnumerable<Gallery> galleries = await context.Galleries.ToListAsync();
            return galleries.Select(x => new GalleryDto(x.Id, x.Name, x.Path));
        }



    }
}
