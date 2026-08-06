using App.Dto;
using App.Enum;
using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace App
{
    public class MediaService(AssetsCatalogContext context)
    {
        public async Task<MediaDto> GetAssetPreviewAsync(MediaFetchDto mediaRequest)
        {
            FileInfo previeFileInfo = null;
            switch (mediaRequest.ItemType)
            {
                case DisplayItemType.Asset:
                    previeFileInfo = await GetAssetPreviewPathAsync(mediaRequest.ItemId);
                    break;
                case DisplayItemType.Group:
                    previeFileInfo = await GetGroupPreviewPathAsync(mediaRequest.ItemId);
                    break;
            }

            if (previeFileInfo == null || string.IsNullOrWhiteSpace(previeFileInfo.Path) || !File.Exists(previeFileInfo.Path))
            {
                // TODO: return not found preview image
            }

            return new MediaDto(
                new FileStream(previeFileInfo.Path, FileMode.Open, FileAccess.Read, FileShare.Read),
                previeFileInfo.MimeType
            );
        }

        public record MediaDto(Stream MediaStream, string MimeType);

        private async Task<FileInfo> GetAssetPreviewPathAsync(int id)
        {
            Asset? asset = await context.Assets.SingleOrDefaultAsync(x => x.Id == id);
            if (asset == null)
            {
                throw new Exception("TODO: NotFoundException");
            }

            return new FileInfo(asset.PreviewPath, asset.MimeType);
        }


        private async Task<FileInfo> GetGroupPreviewPathAsync(int id)
        {
            var fileInfo = await context.AssetGroups
                .Where(g => g.Id == id)
                .Select(g =>
                    g.Assets
                        .Where(a => a.GroupPosition == g.CoverAssetIdx)
                        .Select(a => new FileInfo
                        (
                            a.PreviewPath,
                            a.MimeType
                        ))
                        .Single()
                )
                .SingleOrDefaultAsync();

            if (fileInfo == null)
            {
                throw new Exception("TODO: NotFoundException");
            }

            return fileInfo;
        }

        private record FileInfo(string? Path, string MimeType);
    }
}
