namespace ModManagerUI.EventSystem
{
    public class ModManagerPanelRefreshEvent
    {
        public readonly bool Force;

        public ModManagerPanelRefreshEvent(bool Force = false)
        {
            this.Force = Force;
        }
    }
}