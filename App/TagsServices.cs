using App.Dto.Tag;
using App.Exceptions;
using App.Mappers;
using Data.Entities;

namespace App
{
    public class TagsServices(AssetsCatalogContext context)
    {
        public async Task<TagDto> GetTag(int id)
        {
            Tag? entity = await context.Tags.FindAsync(id);
            EntityNotFoundException<Tag>.ThrowIfNull(entity, id);

            return entity.ToTagDto();
        }

        public async Task<TagDto> SeachTag(string str, int take) // ordering
        {
            Tag? entity = await context.Tags.FindAsync(id);
            EntityNotFoundException<Tag>.ThrowIfNull(entity, id);

            return entity.ToTagDto();
        }

    }
}
