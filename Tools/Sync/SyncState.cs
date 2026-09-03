namespace Tools.Sync
{

    public readonly record struct SyncStateData(
        int GalleriesSynchronized = 0,
        int CreatedAssets = 0,
        int SkippedAssets = 0,
        int DeletedAssets = 0,
        int CreatedGroups = 0,
        string? CurrentGallery = null,
        string? CurrentGroup = null,
        string? CurrentAsset = null
    );

    public sealed class SyncState
    {
        public SyncStateData Data { get; private set; } = new();

        public void Apply(SyncEvent e)
        {

            switch (e.Type)
            {
                case SyncEventType.GallerySyncStarted:
                    Data = Data with
                    {
                        GalleriesSynchronized = Data.GalleriesSynchronized + 1,
                        CurrentGallery = e.Gallery,
                        CurrentGroup = e.Group,
                        CurrentAsset = e.Asset
                    };
                    break;

                case SyncEventType.AssetCreated:
                    Data = Data with
                    {
                        CreatedAssets = Data.CreatedAssets + 1,
                        CurrentGallery = e.Gallery,
                        CurrentGroup = e.Group,
                        CurrentAsset = e.Asset
                    };
                    break;

                case SyncEventType.AssetSkipped:
                    Data = Data with
                    {
                        SkippedAssets = Data.SkippedAssets + 1,
                        CurrentGallery = e.Gallery,
                        CurrentGroup = e.Group,
                        CurrentAsset = e.Asset
                    };
                    break;

                case SyncEventType.AssetDeleted:
                    Data = Data with
                    {
                        DeletedAssets = Data.DeletedAssets + 1,
                        CurrentGallery = e.Gallery,
                        CurrentGroup = e.Group,
                        CurrentAsset = e.Asset
                    };
                    break;

                case SyncEventType.GroupCreated:
                    Data = Data with
                    {
                        CreatedGroups = Data.CreatedGroups + 1,
                        CurrentGallery = e.Gallery,
                        CurrentGroup = e.Group,
                        CurrentAsset = e.Asset
                    };
                    break;
            }
        }
    }

    public sealed record SyncEvent(
        SyncEventType Type,
        string? Gallery = null,
        string? Group = null,
        string? Asset = null
    );

    public enum SyncEventType
    {
        FileScanFinished,
        GalleryProcessingStarted,
        GallerySyncStarted,
        GalleryCreated,
        GroupCreated,
        GroupSyncStarted,
        AssetCreated,
        AssetSkipped,
        AssetDeleted,
        GalleryCompleted
    }
}
