using Timberborn.Modding;

namespace ModManagerUI.StaticInstanceSystem
{
    public class StaticInstanceLoader
    {
        public StaticInstanceLoader(ModLoader modLoader)
        {
            ModLoader = modLoader;
        }

        public static ModLoader ModLoader { get; set; } = null!;
    }
}