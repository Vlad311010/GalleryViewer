namespace App.Models.Commands
{
    public record GalleryCreateCommand(
        string Name,
        string Path
    );
}
