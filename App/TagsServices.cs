using App.Dto.Tag;
using App.Exceptions;
using App.Mappers;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace App
{
    public class TagsServices(AssetsCatalogContext context)
    {
        public async Task<TagDto> Get(int id)
        {
            Tag? entity = await context.Tags.FindAsync(id);
            EntityNotFoundException<Tag>.ThrowIfNull(entity, id);

            return entity.ToTagDto();
        }

        public async Task<IEnumerable<TagDtoSearch>> SearchAsync(string searchValue, int take)
        {
            string[] searchKeys = searchValue.Split(Constants.TAG_SPACE_CHARACTER, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var tags = await context.Tags
                .AsNoTracking()
                .Where(x => searchKeys.Any(k =>
                    x.Name.Contains(k))
                )
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    Category = context.TagCategories.Where(c => c.Id == x.CategoryId).Single().Name,
                    IsCanonical = !x.CanonicalId.HasValue,
                    OccurrencesCount = context.AssetTags.Count(at => at.TagId == x.Id || at.Tag.CanonicalId == x.Id),
                    CanonicalName = x.Canonical == null ? null : x.Canonical.Name
                })
                .OrderByDescending(x => x.OccurrencesCount)
                .ThenBy(x => x.Name)
                .Take(take)
                .ToArrayAsync();

            return tags.Select(x =>
                new TagDtoSearch
                {
                    Id = x.Id,
                    Name = x.Name,
                    Category = x.Category,
                    Occurrences = x.OccurrencesCount,
                    IsCanonical = x.IsCanonical,
                    CanonicalName = x.CanonicalName
                }
            ).ToArray();
        }

    }
}
