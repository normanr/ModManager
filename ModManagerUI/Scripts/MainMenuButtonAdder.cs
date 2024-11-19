using Timberborn.CoreUI;
using Timberborn.Localization;
using Timberborn.MainMenuPanels;
using Timberborn.SingletonSystem;
using UnityEngine.UIElements;

namespace ModManagerUI
{
    public class MainMenuButtonAdder : ILoadableSingleton
    {
        private const string ModBrowserLocKey = "Mods.Browser";
        
        private readonly MainMenuPanel _mainMenuPanel;
        private readonly ILoc _loc;

        public MainMenuButtonAdder(MainMenuPanel mainMenuPanel, ILoc loc)
        {
            _mainMenuPanel = mainMenuPanel;
            _loc = loc;
        }

        public void Load()
        {
            VisualElement root = _mainMenuPanel._root.Query("MainMenuPanel");
            var button = new LocalizableButton
            {
                text = _loc.T(ModBrowserLocKey)
            };
            button.AddToClassList("menu-button");
            button.AddToClassList("menu-button--stretched");
            button.clicked += () => ModManagerPanel.Instance.OpenOptionsDelegate();
            root.Insert(7, button);
        }
    }
}