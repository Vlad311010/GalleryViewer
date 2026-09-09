namespace App.Interfaces.Services
{
    public interface IPreviewCreationService
    {
        Task<string> StageCreatePreviewAsync(string galleryRoot, string assetDataRef);
    }
}
