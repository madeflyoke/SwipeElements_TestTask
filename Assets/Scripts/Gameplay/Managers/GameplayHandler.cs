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
            _signalBus.Subscribe<CallOnNextLevelSignal>(SetNextLevel);
            _signalBus.Subscribe<CallOnRestartLevelSignal>(RestartLevel);
        }
        
        private void OnDisable()
        {
            _signalBus.TryUnsubscribe<LevelCompletedSignal>(OnLevelCompleted);
            _signalBus.TryUnsubscribe<CallOnNextLevelSignal>(SetNextLevel);
            _signalBus.TryUnsubscribe<CallOnRestartLevelSignal>(RestartLevel);
        }
        
        public void Start()
        {
            var lastPlayedLevel = _levelsProgressHandler.LoadLastPlayedLevelData();
            _currentSection = lastPlayedLevel.RelatedSection;
            _currentLevelData = lastPlayedLevel.ConvertToLevelData();
                
            if (lastPlayedLevel.IsStarted && lastPlayedLevel.IsCompleted==false) //continue last started but not completed level
            {
                SetLevel(_currentLevelData,_currentSection);
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
            SetNextLevel();
        }

        private void RestartLevel()
        {
            var currentSection = _assetsProviderService.LoadLevelsSectionDataContainer(_currentSection);
            SetLevel(currentSection.GetLevelData(_currentLevelData.LevelId), _currentSection);
        }
        
        private void SetNextLevel()
        {
            var currentSectionDataContainer = _assetsProviderService.LoadLevelsSectionDataContainer(_currentSection);
            
            LevelData nextLevelData = null;
            
            //New section unlock - usually will be with animations and on the road map
            
            if (_levelsProgressHandler.IsSectionCompleted(currentSectionDataContainer.Section)) 
            {
                var newLevelSection = _assetsProviderService.LoadLevelSectionsContainer().GetNextSection(currentSectionDataContainer.Section);
                _currentSection = newLevelSection;
                _levelsProgressHandler.SaveLastOpenedSection(newLevelSection);
                nextLevelData = _assetsProviderService.LoadLevelsSectionDataContainer(newLevelSection).GetLevelData(0);
            }
            else //for now cycle loop
            {
                var nextLevelId = _currentLevelData.LevelId+1;
                nextLevelData =currentSectionDataContainer.GetLevelData(nextLevelId);
            }
            
            SetLevel(nextLevelData,_currentSection);
        }
    }
}
