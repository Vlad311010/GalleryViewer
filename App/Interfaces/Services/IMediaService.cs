using App.Dtos.Media;

namespace App.Interfaces.Services
{
    public interface IMediaService
    {
        Task<MediaDto> GetAssetMediaAsync(AssetMediaDtoFetch assetRequest);
        Task<string> GetAssetMimeType(AssetMediaDtoFetch assetRequest);
        Task<MediaDto> GetAssetPreviewAsync(MediaDtoFetch mediaRequest);
    }
}
