using System;
using Gameplay.Levels.Data;
using Services;
using Services.AssetsService;
using Signals;
using UnityEngine;
using Zenject;

namespace Gameplay.Managers
{
    public class GameplayHandler : MonoBehaviour
    {
        private SignalBus _signalBus;
        private LevelsDataContainer _levelsDataContainer;
        private LevelData _currentLevelData;
        
        [Inject]
        public void Construct(SignalBus signalBus, ServicesHolder servicesHolder)
        {
            _signalBus = signalBus;
            _levelsDataContainer = servicesHolder.GetService<AssetsProviderService>().LoadJsonDataAsset<LevelsDataContainer>();
        }

        public void SetLevel(int levelId)
        {
            _currentLevelData = _levelsDataContainer.GetLevelData(levelId);
            _signalBus.Fire(new GameplayStartedSignal(_currentLevelData));
        }
        
        public void Start()
        {
            SetLevel(0);
        }
    }
}
