using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Context
{
    public class TagCategoriesConfiguration : IEntityTypeConfiguration<TagCategory>
    {
        public void Configure(EntityTypeBuilder<TagCategory> builder)
        {
            builder.HasData(
                new TagCategory
                {
                    Id = 1,
                    Name = "author"
                },
                new TagCategory
                {
                    Id = 2,
                    Name = "character"
                },
                new TagCategory
                {
                    Id = 3,
                    Name = "source"
                },
                new TagCategory
                {
                    Id = 4,
                    Name = "general"
                }
            );
        }
    }
}
