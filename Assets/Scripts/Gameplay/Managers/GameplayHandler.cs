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

        private void OnEnable()
        {
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
        }
        
        private void OnDisable()
        {
            _signalBus.Unsubscribe<LevelCompletedSignal>(OnLevelCompleted);
        }
        
        public void Start() //once per launch
        {
            var lastPlayedLevel = _levelsProgressHandler.LoadLastPlayedLevelData();
            _currentSection = lastPlayedLevel.RelatedSection;
            _currentLevelData = lastPlayedLevel.ConvertToLevelData();
                
            if (lastPlayedLevel.IsStarted && lastPlayedLevel.IsCompleted==false) //continue last started but not completed level
            {
                SetLevel(_currentLevelData,lastPlayedLevel.RelatedSection);
            }
            else //level selector choice for
            { 
                SetNextLevel();
            }
        }
        
        private void SetLevel(LevelData levelData, LevelSection levelSection)
        {
            _currentSection = levelSection;
            _currentLevelData = levelData;
            _signalBus.Fire(new LevelStartedSignal(_currentSection, _currentLevelData));
        }

        private void OnLevelCompleted()
        {
    //        SetNextLevel();
        }

        [Button]
        private void SetNextLevel()
        {
            var currentSection = _assetsProviderService.LoadLevelsSectionDataContainer(_currentSection);
            var nextLevelId = _currentLevelData.LevelId+1;
            
            if (nextLevelId>=currentSection.Data.Count)
            {
                nextLevelId = 0; //for now cycle loop
            }
            var levelData =currentSection.GetLevelData(nextLevelId);

            SetLevel(levelData,_currentSection);
        }
    }
}
