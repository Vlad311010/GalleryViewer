namespace App.Commands
{
    public record CreateTagCommand(string Name, string Category, int? CanonicalId);
}
