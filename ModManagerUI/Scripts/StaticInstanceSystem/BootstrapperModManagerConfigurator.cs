using Bindito.Core;

namespace ModManagerUI.StaticInstanceSystem
{
    [Context("Bootstrapper")]
    public class BootstrapperModManagerConfigurator : Configurator
    {
        protected override void Configure()
        {
            Bind<StaticInstanceLoader>().AsSingleton().AsExported();
        }
    }
}