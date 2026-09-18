using System.Diagnostics;

namespace Tools.Sync
{

    public readonly record struct GallerySyncState(
        string GalleryName,
        int? TotalFilesToProcess,
        int CreatedAssets = 0,
        int SkippedAssets = 0,
        int DeletedAssets = 0,
        int CreatedGroups = 0,
        string? LastAsset = null,
        TimeSpan? ElapsedTime = null
    );

    public record SyncState(
        int GalleriesSynchronized,
        IReadOnlyDictionary<string, GallerySyncState> State
    )
    {
        public IEnumerable<string> Galleries => State.Keys;
    }


    public class SyncStateTracker
    {
        private readonly Dictionary<string, GallerySyncState> galleries = new();
        private readonly Dictionary<string, long> galleryStartTimestamps = new();
        private int galleriesSynchronized = 0;



        public SyncStateTracker(IEnumerable<string> galleries)
        {
            foreach (var gallery in galleries)
            {
                this.galleries[gallery] = new GallerySyncState(gallery, null);
            }
        }

        public GallerySyncState this[string galleryName]
        {
            get => galleries[galleryName];
        }

        public void UpdateGalleriesFilesCount(string galleryName, int totalFilesToProcess)
        {
            galleries[galleryName] = galleries[galleryName] with { TotalFilesToProcess = totalFilesToProcess };
        }

        public SyncState Apply(SyncEvent e)
        {
            if (galleries.TryGetValue(e.GalleryName, out var data))
            {
                data = e.Type switch
                {
                    SyncEventType.GalleryProcessingStarted =>
                        data,

                    SyncEventType.GalleryProcessingFinished =>
                        data with { ElapsedTime = Stopwatch.GetElapsedTime(galleryStartTimestamps[e.GalleryName]) },

                    SyncEventType.AssetCreated =>
                        data with
                        {
                            CreatedAssets = data.CreatedAssets + e.UpdateValue,
                            LastAsset = e.Asset
                        },

                    SyncEventType.AssetSkipped =>
                        data with
                        {
                            SkippedAssets = data.SkippedAssets + e.UpdateValue,
                            LastAsset = e.Asset
                        },

                    SyncEventType.AssetDeleted =>
                        data with
                        {
                            DeletedAssets = data.DeletedAssets + e.UpdateValue,
                            LastAsset = e.Asset
                        },

                    SyncEventType.GroupCreated =>
                        data with
                        {
                            CreatedGroups = data.CreatedGroups + e.UpdateValue,
                            LastAsset = e.Asset ?? data.LastAsset
                        },

                    SyncEventType.GroupSyncSkipped =>
                        data with
                        {
                            SkippedAssets = data.SkippedAssets + e.UpdateValue,
                            LastAsset = e.Asset
                        },

                    _ => data
                };

                galleries[e.GalleryName] = data;
            }


            if (e.Type == SyncEventType.GalleryProcessingStarted)
            {
                galleryStartTimestamps[e.GalleryName] = Stopwatch.GetTimestamp();
            }
            else if (e.Type == SyncEventType.GalleryProcessingFinished)
            {
                galleriesSynchronized += e.UpdateValue;
            }

            return StateSnapshot();
        }

        private SyncState StateSnapshot()
        {
            return new SyncState(
                galleriesSynchronized,
                new Dictionary<string, GallerySyncState>(galleries));
        }

    }
}
