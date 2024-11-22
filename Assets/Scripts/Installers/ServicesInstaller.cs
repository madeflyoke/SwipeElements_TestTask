using Services;
using Zenject;

namespace Installers
{
    public class ServicesInstaller : MonoInstaller
    {
        private ServicesHolder _servicesHolder;
        
        public override void InstallBindings()
        {
            _servicesHolder = new ServicesHolder(Container);
            Container.Bind<ServicesHolder>().FromInstance(_servicesHolder).AsSingle().NonLazy();
        }
    }
}
