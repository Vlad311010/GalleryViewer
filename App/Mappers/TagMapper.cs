using App.Dtos.Tag;
using Data.Entities;

namespace App.Mappers
{
    public static class TagMapper
    {
        public static TagDto ToTagDto(this Tag entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new TagDto()
            {
                Id = entity.Id,
                Name = entity.Name,
                CategoryId = entity.CategoryId,
                CanonicalId = entity.CanonicalId,
            };

        }
    }
}
