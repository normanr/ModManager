using Bindito.Core;
using ModManager.LoggingSystem;
using ModManager.ManifestValidatorSystem;
using ModManager.MapSystem;
using Timberborn.GameExitSystem;
using Timberborn.SliderToggleSystem;

namespace ModManagerUI
{
    [Context("MainMenu")]
    public class ModManagerConfigurator : IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition)
        {
            containerDefinition.MultiBind<IManifestValidator>().To<MapManifestValidator>().AsSingleton();
            containerDefinition.Bind<ManifestValidatorService>().AsSingleton();
            
            containerDefinition.Bind<ModManagerRegisterer>().AsSingleton();
            containerDefinition.Bind<ModManagerPanel>().AsSingleton();
            containerDefinition.Bind<GoodbyeBoxFactory>().AsSingleton();
            containerDefinition.Bind<ModFullInfoController>().AsSingleton();
            containerDefinition.Bind<SliderToggleButtonFactory>().AsSingleton();
            containerDefinition.Bind<SliderToggleFactory>().AsSingleton();
            containerDefinition.Bind<UpdateableModRegistry>().AsSingleton();
            containerDefinition.Bind<IModManagerLogger>().To<ModManagerLogger>().AsSingleton();
            containerDefinition.Bind<MainMenuButtonAdder>().AsSingleton();
        }
    }
}