namespace ModManagerUI.EventSystem
{
    public class ModDownloadProgressEvent
    {
        public uint ModId { get; }
        public float Progress { get; }

        public ModDownloadProgressEvent(uint modId, float progress)
        {
            ModId = modId;
            Progress = progress;
        }
    }
}