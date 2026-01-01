using Timberborn.Modding;

namespace ModManager.StaticInstanceSystem
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