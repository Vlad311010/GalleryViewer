namespace App.Models.Commands
{
    public record CreateTagCommand(string Name, string Category, int? CanonicalId);
}
