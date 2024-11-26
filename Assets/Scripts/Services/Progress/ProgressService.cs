using System.Threading;
using Cysharp.Threading.Tasks;
using Services.Interfaces;
using Services.Progress.Levels;
using Zenject;

namespace Services.Progress
{
    public class ProgressService : IService
    {
        [Inject] private DiContainer _diContainer;
        public LevelsProgressHandler LevelsProgressHandler { get; private set; }
        
        public UniTask Initialize(CancellationTokenSource cts)
        {
            LevelsProgressHandler = _diContainer.Instantiate<LevelsProgressHandler>(new object[]{_diContainer});
            return UniTask.CompletedTask;
        }

        public void Dispose()
        {
            
        }
    }
}
