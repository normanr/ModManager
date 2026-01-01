namespace ModManager.PlayerPrefsSystem
{
    public record ModPlayerPrefs
    {
        internal string? EnabledKey;
        internal int Enabled;
        internal string? PriorityKey;
        internal int Priority;
    }
}