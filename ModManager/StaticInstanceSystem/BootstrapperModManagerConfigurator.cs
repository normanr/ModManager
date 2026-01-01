using Bindito.Core;

namespace ModManager.StaticInstanceSystem
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