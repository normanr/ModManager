using System.Collections.Generic;

namespace ModManagerUI.UIComponents.ModManagerPanel
{
    public interface IFilterIdsProvider
    {
        List<uint> ProvideFilterIds(out bool isNotList);

        bool HasTagSelected();
    }
}