using System.Collections.Generic;
using Modio.Models;

namespace ModManagerUI.EventSystem
{
    public class UpdatableModsRetrievedEvent
    {
        public IDictionary<uint, File> UpdatableMods { get; }
        
        public UpdatableModsRetrievedEvent(IDictionary<uint, File> updatableMods)
        {
            UpdatableMods = updatableMods;
        }
    }
}