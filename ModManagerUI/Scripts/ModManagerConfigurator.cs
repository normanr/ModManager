using Bindito.Core;
using ModManager.ManifestValidatorSystem;
using ModManager.MapSystem;
using Timberborn.GameExitSystem;
using Timberborn.SliderToggleSystem;

namespace ModManagerUI
{
    [Context("MainMenu")]
    public class ModManagerConfigurator : Configurator
    {
        protected override void Configure()
        {
            MultiBind<IManifestValidator>().To<MapManifestValidator>().AsSingleton();
            Bind<ManifestValidatorService>().AsSingleton();
            
            Bind<ModManagerRegisterer>().AsSingleton();
            Bind<ModManagerPanel>().AsSingleton();
            Bind<GoodbyeBoxFactory>().AsSingleton();
            Bind<ModFullInfoController>().AsSingleton();
            Bind<SliderToggleButtonFactory>().AsSingleton();
            Bind<SliderToggleFactory>().AsSingleton();
            Bind<UpdateableModRegistry>().AsSingleton();
            Bind<MainMenuButtonAdder>().AsSingleton();
        }
    }
}