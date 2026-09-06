namespace App.Interfaces.Services
{
    public interface IPreviewCreationService
    {
        Task<string> CreatePreviewAsync(string galleryRoot, string assetDataRef);
    }
}
