namespace App.Models.Queries
{
    public record TagSearchQuery(string SearchKey, int Take = 5);
}
