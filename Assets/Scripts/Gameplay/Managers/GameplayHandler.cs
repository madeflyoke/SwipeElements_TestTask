using System;
using EasyButtons;
using Gameplay.Levels.Data;
using Gameplay.Levels.Enums;
using Services;
using Services.AssetsService;
using Services.Progress;
using Services.Progress.Levels;
using Signals;
using UnityEngine;
using Zenject;

namespace Gameplay.Managers
{
    public class GameplayHandler : MonoBehaviour
    {
        private SignalBus _signalBus;
        private LevelsProgressHandler _levelsProgressHandler;
        private AssetsProviderService _assetsProviderService;
        
        private LevelData _currentLevelData;
        private LevelSection _currentSection;

        [Inject]
        public void Construct(SignalBus signalBus, ServicesHolder servicesHolder)
        {
            _signalBus = signalBus;
            _assetsProviderService = servicesHolder.GetService<AssetsProviderService>();
            _levelsProgressHandler = servicesHolder.GetService<ProgressService>().LevelsProgressHandler;
        }

        private void SetLevel(LevelSection section, int levelId)
        {
            _currentSection = section;
            _currentLevelData = _assetsProviderService.LoadLevelsSectionDataContainer(section).GetLevelData(levelId);
            _signalBus.Fire(new GameplayStartedSignal(_currentSection, _currentLevelData));
        }

        [Button]
        public void SetLevelCompleted()
        {
            _levelsProgressHandler.SaveLevelCompleted(_currentSection, _currentLevelData.LevelId);
        }

        [Button]
        public void GetLevelCompleted()
        {
            Debug.LogWarning(_levelsProgressHandler.IsLevelCompleted(_currentSection, _currentLevelData.LevelId));
        }
        
        public void Start()
        {
            SetLevel(LevelSection.TROPICAL_GREEN,0);
        }
    }
}
