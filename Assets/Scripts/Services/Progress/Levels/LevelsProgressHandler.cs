using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gameplay.Blocks.Enums;
using Gameplay.Levels.Enums;
using Newtonsoft.Json;
using Services.AssetsService;
using Signals;
using UnityEngine;
using Zenject;

namespace Services.Progress.Levels
{
    /// <summary>
    /// PlayerPrefs with jsons in it, divided by sections
    /// </summary>
    public partial class LevelsProgressHandler : IDisposable //maybe too complicated logic (decomposition?)
    {
        private const string SEPARATOR = "|";
        
        private const string LAST_OPENED_SECTION = "LastOpenedSection";
        private const string LAST_PLAYED_LEVEL_DATA = "LastLevelData";
        private const string LEVELS_SECTION_DATA_CONTAINER_KEY = "LevelsDataContainerKey";
        
        private readonly AssetsProviderService _assetsProviderService;
        private SectionProgressDataContainer _currentSectionProgressDataContainer;
        
        private LevelProgressDataExtended _cachedCurrentLevelProgressData;
        private int _currentLevelId; //not good idea to put full level data here

        private StringBuilder _stringBuilder;
        private readonly SignalBus _signalBus;

        public LevelsProgressHandler(DiContainer diContainer, SignalBus signalBus)
        {
            _signalBus = signalBus;
            _assetsProviderService = diContainer.Resolve<ServicesHolder>().GetService<AssetsProviderService>();
            SubscribeSignals();
            Initialize();
        }
        
        private void Initialize()
        {
            _stringBuilder = new StringBuilder();

            var lastSection = LoadLastOpenedSection();
            if (lastSection==LevelSection.NONE)
            {
                HandleFirstSession();
            }
            else
            {
                SetCurrentSectionDataContainer(lastSection);
            }
            _cachedCurrentLevelProgressData = LoadLastPlayedLevelData();
        }
        
        public bool IsLevelCompleted(LevelSection section, int levelId)
        {
            var sectionContainer = LoadSectionProgressDataContainer(section);
            return sectionContainer.LevelsProgressDataMap[levelId].IsCompleted;;
        }

        public bool IsSectionCompleted(LevelSection section)
        {
            var sectionContainer = LoadSectionProgressDataContainer(section);
            return sectionContainer.SectionCompleted;
        }

        public void SaveLastOpenedSection(LevelSection section)
        {
            PlayerPrefs.SetInt(LAST_OPENED_SECTION, (int)section);
        }

        private LevelSection LoadLastOpenedSection()
        {
            return (LevelSection) PlayerPrefs.GetInt(LAST_OPENED_SECTION,0);
        }
        
        private void HandleFirstSession()
        {
            var firstSection = _assetsProviderService.LoadLevelSectionsContainer().GetSection(0);
            SetCurrentSectionDataContainer(firstSection);
            SaveLastOpenedSection(firstSection);
            
            var dataContainer = _assetsProviderService.LoadLevelsSectionDataContainer(firstSection);
            SaveCurrentLevelExtendedData(_currentSectionProgressDataContainer.Section,0, dataContainer.GetLevelData(0).BlocksData);
        }

        //Operating only with current section, when new section required - loading or creating it 
        private void SetCurrentSectionDataContainer(LevelSection targetSection)
        {
            if (_currentSectionProgressDataContainer!=null && _currentSectionProgressDataContainer.Section==targetSection)
            {
                return;
            }
            
            var key = GetSectionContainerKey(targetSection);
            if (PlayerPrefs.HasKey(key)==false)
            {
                var dataContainer = _assetsProviderService.LoadLevelsSectionDataContainer(targetSection);
                var progressDatas = new List<LevelProgressData>();
                for (int i = 0; i < dataContainer.Data.Count; i++)
                {
                    progressDatas.Add(new LevelProgressData(){LevelId = i});
                }

                var progressDataContainers = new SectionProgressDataContainer(progressDatas,targetSection);
                
                _currentSectionProgressDataContainer = progressDataContainers;
                SaveCurrentSectionProgressDataContainer();
            }
            else
            {
                _currentSectionProgressDataContainer =LoadSectionProgressDataContainer(targetSection);
            }
        }
        
        private void SaveCurrentSectionProgressDataContainer()
        {
            SaveInternal(GetSectionContainerKey(_currentSectionProgressDataContainer.Section), _currentSectionProgressDataContainer);
        }

        private SectionProgressDataContainer LoadSectionProgressDataContainer(LevelSection section)
        {
            return LoadInternal<SectionProgressDataContainer>(GetSectionContainerKey(section));
        }
        
        private string GetSectionContainerKey(LevelSection section)
        {
            var sectionId = (int)section;
            _stringBuilder.Clear();
            _stringBuilder
                .Append(LEVELS_SECTION_DATA_CONTAINER_KEY)
                .Append(SEPARATOR)
                .Append(sectionId);
            return _stringBuilder.ToString();
        }

        private void SaveInternal(string key, object value)
        {
            var json = JsonConvert.SerializeObject(value);
            var jsonEncoded = EncodeBase64(json);
            PlayerPrefs.SetString(key, jsonEncoded);
            PlayerPrefs.Save();
        }

        private T LoadInternal<T>(string key)
        {
            var json = PlayerPrefs.GetString(key);
            var jsonDecoded = DecodeBase64(json);
            return JsonConvert.DeserializeObject<T>(jsonDecoded);
        }
        
        private string EncodeBase64(string input) //little bit obfuscating
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(plainTextBytes);
        }
        
        private string DecodeBase64(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
    
    /// <summary>
    /// Current level part
    /// </summary>
    public partial class LevelsProgressHandler
    {
        private void OnGameFieldChanged(GameFieldChangedSignal signal)
        {
            SaveCurrentLevelExtendedData(_currentSectionProgressDataContainer.Section, _currentLevelId, signal.GridBlocksState);
        }
        
        private void OnLevelStarted(LevelStartedSignal signal)
        {
            SetCurrentSectionDataContainer(signal.RelatedSection);
            _currentLevelId = signal.LevelData.LevelId;
            SaveCurrentLevelExtendedData(signal.RelatedSection, _currentLevelId, signal.LevelData.BlocksData);
        }
        
        private void OnLevelCompleted()
        {
            SaveCurrentLevelCompleted();
        }
        
        private void SaveCurrentLevelCompleted()
        {
            _cachedCurrentLevelProgressData.IsCompleted = true;
            SaveCachedCurrentLevelData();
            
            _currentSectionProgressDataContainer.LevelsProgressDataMap[_currentLevelId].IsCompleted = true;
            SaveCurrentSectionProgressDataContainer();
        }
        
        private void SaveCurrentLevelExtendedData(LevelSection section, int levelId, BlockType[,] gridState)
        {
            if (_cachedCurrentLevelProgressData==null||
                _cachedCurrentLevelProgressData.RelatedSection!=section ||
                _cachedCurrentLevelProgressData.LevelId!=levelId)
            {
                _cachedCurrentLevelProgressData = new LevelProgressDataExtended();
                var uniqueKey =  _assetsProviderService.LoadLevelsSectionDataContainer(section).GetUniqueKey(levelId);
                _cachedCurrentLevelProgressData.UniqueKey = uniqueKey;
            }
            
            _cachedCurrentLevelProgressData.RelatedSection = section;
            _cachedCurrentLevelProgressData.LevelId = levelId;
            _cachedCurrentLevelProgressData.GridState = gridState;
            _cachedCurrentLevelProgressData.IsStarted = true;

            SaveCachedCurrentLevelData();
        }
        
        public LevelProgressDataExtended LoadLastPlayedLevelData()
        {
            var lastPlayerData = LoadInternal<LevelProgressDataExtended>(LAST_PLAYED_LEVEL_DATA);
            var container = _assetsProviderService.LoadLevelsSectionDataContainer(_currentSectionProgressDataContainer.Section);
            var originalLevelUniqueKey =container.GetUniqueKey(lastPlayerData.LevelId);

            if (originalLevelUniqueKey!=lastPlayerData.UniqueKey)
            {
                Debug.LogWarning("Last level data changed, set actual level...");
                var originalLevel = container.GetLevelData(lastPlayerData.LevelId);
                _cachedCurrentLevelProgressData = null;
                SaveCurrentLevelExtendedData(lastPlayerData.RelatedSection,lastPlayerData.LevelId, originalLevel.BlocksData);
            }
            
            lastPlayerData = LoadInternal<LevelProgressDataExtended>(LAST_PLAYED_LEVEL_DATA);

            return lastPlayerData;
        }

        private void SaveCachedCurrentLevelData()
        {
            SaveInternal(LAST_PLAYED_LEVEL_DATA, _cachedCurrentLevelProgressData);
        }
        
        private void SubscribeSignals()
        {
            _signalBus.Subscribe<GameFieldChangedSignal>(OnGameFieldChanged);
            _signalBus.Subscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
        }
        
        private void UnsubscribeSignals()
        {
            _signalBus.TryUnsubscribe<GameFieldChangedSignal>(OnGameFieldChanged);
            _signalBus.TryUnsubscribe<LevelStartedSignal>(OnLevelStarted);
            _signalBus.TryUnsubscribe<LevelCompletedSignal>(OnLevelCompleted);
        }
        
        public void Dispose()
        {
            UnsubscribeSignals();
        }
    }
}
