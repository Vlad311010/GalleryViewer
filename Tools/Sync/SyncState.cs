using App.Dto.Group;

namespace Tools.Sync
{

    public readonly record struct GallerySyncState(
        string GalleryName,
        int TotalFilesToProcess,
        int CreatedAssets = 0,
        int SkippedAssets = 0,
        int DeletedAssets = 0,
        int CreatedGroups = 0,
        string? LastAsset = null,
        string? LastGroup = null
    );

    public sealed class SyncState
    {
        private readonly Dictionary<string, GallerySyncState> galleriesSyncData = new Dictionary<string, GallerySyncState>();
        public string[] GalleriesToProcess { get; private set; } = [];

        public int GalleriesSynchronized { get; private set; } = 0;

        public GallerySyncState this[string key]
        {
            get
            {
                return galleriesSyncData[key];
            }
        }

        public Dictionary<string, GallerySyncState>.KeyCollection Keys => galleriesSyncData.Keys;

        public void SetGalleries(IEnumerable<string> galleries)
        {
            GalleriesToProcess = [.. galleries];
        }


        public void TrackGallery(string galleryName, int totalFilesToProccess)
        {
            galleriesSyncData[galleryName] = new GallerySyncState(galleryName, totalFilesToProccess);
        }

        public void Apply(SyncEvent e)
        {
            if (!galleriesSyncData.TryGetValue(e.GalleryName, out GallerySyncState data))
            {
                return;
            }

            switch (e.Type)
            {
                case SyncEventType.GallerySyncStarted:
                    GalleriesSynchronized = GalleriesSynchronized + e.updateValue;
                    break;

                case SyncEventType.AssetCreated:
                    galleriesSyncData[e.GalleryName] = data with
                    {
                        CreatedAssets = data.CreatedAssets + e.updateValue,
                        LastGroup = e.Group?.Title ?? e.Group?.PhysicalPath ?? e.Group?.Id.ToString() ?? "-",
                        LastAsset = e.Asset
                    };

                    break;

                case SyncEventType.AssetSkipped:
                    galleriesSyncData[e.GalleryName] = data with
                    {
                        SkippedAssets = data.SkippedAssets + e.updateValue,
                        LastGroup = e.Group?.Title ?? e.Group?.PhysicalPath ?? e.Group?.Id.ToString() ?? "-",
                        LastAsset = e.Asset
                    };
                    break;

                case SyncEventType.AssetDeleted:
                    galleriesSyncData[e.GalleryName] = data with
                    {
                        DeletedAssets = data.DeletedAssets + e.updateValue,
                        LastGroup = e.Group?.Title ?? e.Group?.PhysicalPath ?? e.Group?.Id.ToString() ?? "-",
                        LastAsset = e.Asset
                    };
                    break;

                case SyncEventType.GroupCreated:
                    galleriesSyncData[e.GalleryName] = data with
                    {
                        CreatedGroups = data.CreatedGroups + e.updateValue,
                        LastGroup = e.Group?.Title ?? e.Group?.PhysicalPath ?? e.Group?.Id.ToString() ?? "-",
                        LastAsset = e.Asset ?? data.LastAsset
                    };
                    break;

                case SyncEventType.GroupSyncSkipped:
                    galleriesSyncData[e.GalleryName] = data with
                    {
                        SkippedAssets = data.SkippedAssets + e.updateValue,
                        LastGroup = e.Group?.Title ?? e.Group?.PhysicalPath ?? e.Group?.Id.ToString() ?? "-",
                        LastAsset = e.Asset
                    };
                    break;
            }
        }
    }

    public sealed record SyncEvent(
        SyncEventType Type,
        string GalleryName,
        AssetGroupDto? Group = null,
        string? Asset = null,
        int updateValue = 1
    );

    public enum SyncEventType
    {
        FileScanFinished,
        GalleryProcessingStarted,
        GallerySyncStarted,
        GalleryCreated,
        GroupCreated,
        GroupSyncStarted,
        GroupSyncSkipped,
        AssetCreated,
        AssetSkipped,
        AssetDeleted,
        GalleryCompleted
    }
}
