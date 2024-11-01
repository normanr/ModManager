using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Modio.Filters;
using ModManager.AddonSystem;
using ModManager.ManifestValidatorSystem;
using ModManager.ModIoSystem;
using ModManagerUI.Components.ModManagerPanel;
using ModManagerUI.EventSystem;
using Timberborn.AssetSystem;
using Timberborn.CoreUI;
using Timberborn.ExperimentalModeSystem;
using Timberborn.Localization;
using Timberborn.Modding;
using Timberborn.SingletonSystem;
using Timberborn.TooltipSystem;
using UnityEngine;
using UnityEngine.UIElements;
using EventBus = ModManagerUI.EventSystem.EventBus;
using Image = UnityEngine.UIElements.Image;
using Mod = Modio.Models.Mod;
using TextField = UnityEngine.UIElements.TextField;

namespace ModManagerUI.UiSystem
{
    public class ModManagerPanel : IPanelController, ILoadableSingleton, IUpdatableSingleton
    {
        private static readonly string ModesBoxUxmlPath = "ui/views/mods/modsbox";
        
        public static bool CheckForHighestInsteadOfLive;
        public static bool ModsWereChanged = false;
        
        public static InstalledAddonRepository InstalledAddonRepository = null!;
        public static ModFullInfoController ModFullInfoController = null!;
        public static VisualElementLoader VisualElementLoader = null!;
        public static ITooltipRegistrar TooltipRegistrar = null!;
        public static ModRepository ModRepository = null!;
        public static IAssetLoader AssetLoader = null!;
        public static PanelStack PanelStack = null!;
        public static ModLoader ModLoader = null!;
        public static ILoc Loc = null!;
        
        private readonly VisualElementInitializer _visualElementInitializer;
        private readonly ExperimentalMode _experimentalMode;
        private readonly DialogBoxShower _dialogBoxShower;

        private VisualElement? _root;
        private ModsScrollView? _modsRoot;
        private Label _loading = null!;
        private Label _error = null!;
        private TextField _search = null!;
        private TagsWrapper _tagsWrapper = null!;
        private ShowMoreButton _showMoreButton = null!;
        private UpdateAllWrapper? _updateAll;
        
        private Toggle _updateBehaviour = null!;
        private Image _updateBehaviourInfo = null!;

        public static readonly uint ModsPerPage = 25;
        public static ModManagerPanel Instance = null!;
        public Action OpenOptionsDelegate { get; private set; } = null!;

        private IAsyncEnumerable<IReadOnlyList<Mod>>? _getModsTask;
        
        private static CancellationTokenSource _cancellationTokenSource = new();
        private static CancellationToken _token = _cancellationTokenSource.Token;
        
        private static CancellationTokenSource _cancellationTokenSource2 = new();
        private static CancellationToken _token2 = _cancellationTokenSource2.Token;
        
        public ModManagerPanel(
            ModFullInfoController modFullInfoController,
            ITooltipRegistrar tooltipRegistrar,
            ModRepository modRepository,
            IAssetLoader assetLoader,
            PanelStack panelStack,
            ModLoader modLoader,
            ILoc loc,
            
            VisualElementInitializer visualElementInitializer,
            InstalledAddonRepository? installedAddonRepository,
            VisualElementLoader visualElementLoader,
            ExperimentalMode experimentalMode,
            DialogBoxShower dialogBoxShower)
        {
            InstalledAddonRepository = installedAddonRepository;
            ModFullInfoController = modFullInfoController;
            TooltipRegistrar = tooltipRegistrar;
            ModRepository = modRepository;
            AssetLoader = assetLoader;
            PanelStack = panelStack;
            ModLoader = modLoader;
            Loc = loc;
            
            _visualElementInitializer = visualElementInitializer;
            VisualElementLoader = visualElementLoader;
            _experimentalMode = experimentalMode;
            _dialogBoxShower = dialogBoxShower;

            Instance = this;
        }

        public void Load()
        {
            OpenOptionsDelegate = OpenOptionsPanel;
            CheckForHighestInsteadOfLive = _experimentalMode.IsExperimental;

            var asset = AssetLoader.Load<VisualTreeAsset>(ModesBoxUxmlPath);
            _root = LoadVisualElement(asset);

            var modsBox = _root.Q<VisualElement>("ModsBox");
            modsBox.style.marginBottom = new Length(1, LengthUnit.Percent);
            modsBox.style.marginTop = new Length(1, LengthUnit.Percent);
            _root.Q<NineSliceVisualElement>().style.flexGrow = 0;

            _modsRoot = ModsScrollView.Create(_root.Q<ScrollView>("Mods"));
            _loading = _root.Q<Label>("Loading");
            _error = _root.Q<Label>("Error");
            _tagsWrapper = new TagsWrapper(_root.Q<VisualElement>("TagsWrapper"));
            _tagsWrapper.Initialize();

            // var versionStatusTagOption = RadioButtonTagOption.Create(typeof(VersionStatus));
            // new VersionStatusRadioButtonGroup(root.Q<RadioButtonGroup>("VersionStatusOptions"), versionStatusTagOption, this).Initialize();
            
            var installedTagOption = RadioButtonTagOption.Create(typeof(InstalledOptions));
            new InstalledRadioButtonGroup(_root.Q<RadioButtonGroup>("Options"), installedTagOption).Initialize(true);
            
            var enabledTagOption = RadioButtonTagOption.Create(typeof(EnabledOptions));
            new EnabledRadioButtonGroup(_root.Q<RadioButtonGroup>("EnabledOptions"), enabledTagOption).Initialize(true);

            _showMoreButton = ShowMoreButton.Create(_root.Q<Button>("ShowMore"));

            SortingButtonsManager.AddNew(_root.Q<Button>("MostDownloaded"), "MostDownloaded", ModFilter.Downloads.Desc());
            SortingButtonsManager.AddNew(_root.Q<Button>("Newest"), "Newest", ModFilter.DateAdded.Desc());
            SortingButtonsManager.AddNew(_root.Q<Button>("TopRated"), "TopRated", ModFilter.Rating.Desc());
            SortingButtonsManager.AddNew(_root.Q<Button>("LastUpdated"), "LastUpdated", ModFilter.DateUpdated.Desc());

            var enableAll = _root.Q<Button>("EnableAll");
            enableAll.RegisterCallback<ClickEvent>(_ => InstalledAddonRepository.All().ToList().ForEach(manifest => EnableController.ChangeState(manifest, true)));
            var disableAll = _root.Q<Button>("DisableAll");
            disableAll.RegisterCallback<ClickEvent>(_ => InstalledAddonRepository.All().ToList().ForEach(manifest => EnableController.ChangeState(manifest, false)));
            
            _updateAll = UpdateAllWrapper.Create(_root.Q<VisualElement>("UpdateAllWrapper"), () => UpdateableModRegistry.UpdateAvailable!);
            
            _updateBehaviour = _root.Q<Toggle>("UpdateBehaviour");
            _updateBehaviour.SetValueWithoutNotify(CheckForHighestInsteadOfLive);
            _updateBehaviour.RegisterValueChangedCallback(UpdateBehaviourToggleChanged);
            _updateBehaviourInfo = _root.Q<Image>("UpdateBehaviourInfo");
            TooltipRegistrar.Register(_updateBehaviourInfo, Loc.T("Mods.UpdateBehaviourTooltip"));

            _root.Q<Button>("Close").RegisterCallback<ClickEvent>(_ => OnUICancelled());
            _root.Q<Button>("SearchButton").RegisterCallback<ClickEvent>(_ => EventBus.Instance.PostEvent(new ModManagerPanelRefreshEvent()));
            _search = _root.Q<TextField>("Search");
            _search.isDelayed = true;
            _search.RegisterValueChangedCallback(_ => EventBus.Instance.PostEvent(new ModManagerPanelRefreshEvent()));
            
            EventBus.Instance.Register(this);
        }
        
        public VisualElement GetPanel()
        {
            EventBus.Instance.PostEvent(new ModManagerPanelRefreshEvent());
            return _root!;
        }

        public void UpdateSingleton()
        {
            _modsRoot?.Update();
        }
        
        private VisualElement LoadVisualElement(VisualTreeAsset visualTreeAsset)
        {
            var visualElement = visualTreeAsset.CloneTree().ElementAt(0);
            _visualElementInitializer.InitializeVisualElement(visualElement);
            return visualElement;
        }

        private void UpdateBehaviourToggleChanged(ChangeEvent<bool> changeEvent)
        {
            CheckForHighestInsteadOfLive = changeEvent.newValue;
            EventBus.Instance.PostEvent(new ModManagerPanelRefreshEvent());
        }

        private void OpenOptionsPanel()
        {
            PanelStack.HideAndPush(this);
        }

        public bool OnUIConfirmed()
        {
            return false;
        }

        public void OnUICancelled()
        {
            if (ModsWereChanged)
            {
                _dialogBoxShower
                    .Create()
                    .SetLocalizedMessage("Mods.ModsChanged")
                    .SetConfirmButton(GameRestarter.QuitOrRestart, Loc.T(SteamChecker.IsRestartCompatible() ? "Mods.Restart" : "Mods.Quit"))
                    .SetCancelButton(() => PanelStack.Pop(this), Loc.T("Mods.Stay"))
                    .Show();
            }
            else
            {
                PanelStack.Pop(this);
            }
        }

        [OnEvent]
        public void OnModManagerPanelRefreshEvent(ModManagerPanelRefreshEvent modManagerPanelRefreshEvent)
        {
            Refresh();
        }
        
        [OnEvent]
        public async void OnShowMoreModsEvent(ShowMoreModsEvent showMoreModsEvent)
        {
            await ShowMods();
        }
        
        [OnEvent]
        public void OnModsBoxFullInfoOpenedEvent(ModsBoxFullInfoOpenedEvent modsBoxFullInfoOpenedEvent)
        {
            _root.ToggleDisplayStyle(false);
        }
        
        [OnEvent]
        public void OnModsBoxFullInfoClosedEvent(ModsBoxFullInfoClosedEvent modsBoxFullInfoClosedEvent)
        {
            _root.ToggleDisplayStyle(true);
        }
        
        [OnEvent]
        public async Task OnModsRetrieved(ModsRetrievedEvent modsRetrievedEvent)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _token = _cancellationTokenSource.Token;
            try
            { 
                await FillTheWrapper(modsRetrievedEvent.Mods, modsRetrievedEvent.Token);
            }
            catch (OperationCanceledException ex)
            {
                Debug.LogWarning($"Async operation was cancelled: {ex.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
                ShowError(e);
            }
        }
        
        private async Task ShowMods()
        {
            _loading.ToggleDisplayStyle(true);
            _error.ToggleDisplayStyle(false);

            // _cancellationTokenSource2.Cancel();
            _cancellationTokenSource2 = new CancellationTokenSource();
             var token = _cancellationTokenSource2.Token;
            var filter = FilterController.Create(_search, _tagsWrapper.Root);
            _getModsTask ??= ModIo.ModsClient.Search(filter).ToPagedEnumerable();
            EventBus.Instance.PostEvent(new ModsRetrievedEvent(await _getModsTask.FirstAsync(token), token));
        }
        
        private async void Refresh()
        {
            try
            {
                _getModsTask = null;
                _modsRoot?.Root.Clear();
                await ShowMods();
            }
            catch (OperationCanceledException ex)
            {
                Debug.Log($"Async operation was cancelled: {ex.Message}");
            }
        }

        private Task FillTheWrapper(IReadOnlyCollection<Mod> mods, CancellationToken token)
        {
            _loading.ToggleDisplayStyle(false);
            foreach (var mod in mods)
            {
                if (token.IsCancellationRequested)
                    continue;
                var modCard = ModCardRegistry.Get(mod) ?? ModCard.Create(mod);
                _modsRoot?.Root.Add(modCard.Root);
                modCard.Refresh();
            }

            return Task.CompletedTask;
        }

        private void ShowError(Exception e)
        {
            _loading.ToggleDisplayStyle(false);
            _error.ToggleDisplayStyle(true);
            _error.text = e.Message;
        }
    }
}