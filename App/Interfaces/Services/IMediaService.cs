using App.Models.Dtos.Media;
using App.Models.Queries;

namespace App.Interfaces.Services
{
    public interface IMediaService
    {
        Task<MediaDto> GetAssetMediaAsync(AssetQuery assetRequest);
        Task<string> GetAssetMimeType(AssetQuery assetRequest);
        Task<MediaDto> GetPreviewAsync(MediaQuery mediaRequest);
    }
}
