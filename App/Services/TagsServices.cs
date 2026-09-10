using App.Commands;
using App.Dtos.Filter;
using App.Dtos.Tag;
using App.Exceptions;
using App.Interfaces.Services;
using App.Mappers;
using App.Validators;
using Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Enums;
using Shared.Extensions;
using Shared.Models;

namespace App.Services
{
    public class TagsServices(AssetsCatalogContext context, ILogger<TagsServices> logger) : ITagsService
    {
        public async Task<TagDto> Get(int id)
        {
            Tag? entity = await context.Tags.FindAsync(id);
            EntityNotFoundException<Tag>.ThrowIfNull(entity, id);

            return entity.ToTagDto();
        }

        public async Task<IEnumerable<TagDtoSearch>> SearchAsync(string searchValue, int take)
        {
            string searchValueClean = searchValue.Replace(Constants.TAG_SPACE_CHARACTER, Constants.SPACE_CHARACTER);
            string[] searchKeys = searchValue.Split(Constants.TAG_SPACE_CHARACTER, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var tags = await context.Tags
                .AsNoTracking()
                /*.Where(x => searchKeys.Any(k =>
                    x.Name.Contains(k))
                )*/
                .Where(x => searchKeys.Any(k => // searchKey match againts every separate word in tag.
                    x.Name.StartsWith(k) ||
                    x.Name.Contains(Constants.SPACE_CHARACTER + k))
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
                .OrderByDescending(x => x.Name.StartsWith(searchValueClean))
                .ThenByDescending(x => x.OccurrencesCount)
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

        public async Task<TagDtoInfo> Create(CreateTagCommand command)
        {
            await new CreateTagCommandValidator().ValidateAndThrowAsync(command);

            TagCategory? tagCategory = await context.TagCategories
                .AsNoTracking()
                .Where(x => x.Name == command.Category)
                .SingleOrDefaultAsync();

            EntityNotFoundException<TagCategory>.ThrowIfNull(tagCategory, command.Category);

            if (await context.Tags.Where(x => x.Name == command.Name).AnyAsync())
            {
                throw new EntityAlreadyExistsException<Tag>(command.Name);
            }

            Tag entity = new Tag
            {
                Name = NormalizeTag(command.Name),
                CategoryId = tagCategory.Id,
                CanonicalId = command.CanonicalId,
            };

            context.Tags.Add(entity);

            await context.SaveChangesAsync();

            logger.Info("Created tag {name} id:{id}, category:{categoryId}", ApplicationArea.Service,
                entity.Name,
                entity.Id,
                entity.CategoryId);

            return new TagDtoInfo
            {
                Id = entity.Id,
                Name = entity.Name,
                Category = tagCategory.Name,
                CanonicalId = null
            };
        }

        public async Task<PagedData<TagDtoInfo>> ListAsync(PaginationDto paginationDto)
        {
            var tags = await context.Tags
                .AsNoTracking()
                .OrderBy(x => x.Name) // TODO:? implement orderby parameter
                .Skip(paginationDto.Skip)
                .Take(paginationDto.Take)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    CategoryName = x.Category.Name,
                    CananicalId = x.CanonicalId,
                    CanonicalName = x.Canonical != null ? x.Canonical.Name : "null",
                    OccurrencesCount = context.AssetTags.Count(at => at.TagId == x.Id || at.Tag.CanonicalId == x.Id),
                })
                .ToArrayAsync();


            int totalCount = await context.Tags.CountAsync();
            TagDtoInfo[] seletedTags = [.. tags.Select(x => new TagDtoInfo
            {
                Id = x.Id,
                Name = x.Name,
                Category = x.CategoryName,
                Occurrences = x.OccurrencesCount,
                CanonicalId = x.CananicalId,
                CanonicalName = x.CanonicalName
            })];

            return new PagedData<TagDtoInfo>(seletedTags, paginationDto.Skip, paginationDto.Take, totalCount);
        }

        private static string NormalizeTag(string tag)
        {
            return tag.Trim().ToLower().Replace(Constants.TAG_SPACE_CHARACTER, Constants.SPACE_CHARACTER);
        }
    }
}
