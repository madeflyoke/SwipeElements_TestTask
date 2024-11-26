using Signals;
using Zenject;

namespace Installers
{
    public class SignalsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<LevelStartedSignal>();
            Container.DeclareSignal<LevelCompletedSignal>();
            Container.DeclareSignal<GameFieldChangedSignal>();
        }
    }
}
