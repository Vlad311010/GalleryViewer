using App.Models.Dtos.Media;
using App.Models.Queries;

namespace App.Interfaces.Services
{
    public interface IMediaService
    {
        Task<MediaDto> GetAssetMediaAsync(AssetQuery assetRequest);
        Task<string> GetAssetMimeTypeAsync(AssetQuery assetRequest);
        Task<MediaDto> GetPreviewAsync(PreviewQuery mediaRequest);
    }
}
