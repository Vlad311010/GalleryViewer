namespace Tools.Sync
{
    public sealed record SyncEvent(
        SyncEventType Type,
        string GalleryName,
        string? Asset = null,
        int UpdateValue = 1
    );
}
