using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gameplay.Blocks.Enums;
using Gameplay.Levels.Enums;
using Newtonsoft.Json;
using Services.AssetsService;
using UnityEngine;
using Zenject;

namespace Services.Progress.Levels
{
    public class LevelsProgressHandler
    {
     //   private const string LAST_LEVEL_DATA = "LastLevelData";
        private const string LEVELS_SECTION_DATA_CONTAINER_KEY = "LevelsDataContainerKey";
        
        private readonly AssetsProviderService _assetsProviderService;
        private SectionProgressDataContainer _currentSectionProgressDataContainer;
        
        public LevelsProgressHandler(DiContainer diContainer)
        {
            _assetsProviderService = diContainer.Resolve<ServicesHolder>().GetService<AssetsProviderService>();
        }

        public void SaveLevelCompleted(LevelSection section, int levelId)
        {
            ValidateCurrentSectionDataContainer(section);
            _currentSectionProgressDataContainer.LevelsProgressData.FirstOrDefault(x => x.LevelId == levelId) //TODO map?
                .IsCompleted = true;
            SaveCurrentSectionProgressDataContainer();
        }

        public bool IsLevelCompleted(LevelSection section, int levelId)
        {
            ValidateCurrentSectionDataContainer(section);
            return _currentSectionProgressDataContainer.LevelsProgressData.FirstOrDefault(x => x.LevelId == levelId).IsCompleted;
        }
        
        private void SaveCurrentSectionProgressDataContainer()
        {
            var levelsJson = JsonConvert.SerializeObject(_currentSectionProgressDataContainer);
            var levelsJsonEncoded = EncodeBase64(levelsJson);
            PlayerPrefs.SetString(GetSectionContainerKey(_currentSectionProgressDataContainer.Section), levelsJson);
        }

        private SectionProgressDataContainer LoadSectionProgressDataContainer(LevelSection section)
        {
            var levelsJson = PlayerPrefs.GetString(GetSectionContainerKey(section));
            return JsonConvert.DeserializeObject<SectionProgressDataContainer>(levelsJson); 
            var levelsJsonEncoded = EncodeBase64(levelsJson);
        }
        
        //Operating only with current section, when new section required - loading or creating it 
        private void ValidateCurrentSectionDataContainer(LevelSection targetSection)
        {
            if (_currentSectionProgressDataContainer!=null && _currentSectionProgressDataContainer.Section==targetSection)
            {
                return;
            }
            
            var key = GetSectionContainerKey(targetSection);
            if (PlayerPrefs.HasKey(key)==false)
            {
                var progressDataContainers = new SectionProgressDataContainer();
                progressDataContainers.Section = targetSection;

                var dataContainer = _assetsProviderService.LoadLevelsSectionDataContainer(targetSection);
                var progressDatas = new List<LevelProgressData>();
                for (int i = 0; i < dataContainer.Data.Count; i++)
                {
                    progressDatas.Add(new LevelProgressData(){LevelId = i});
                }

                progressDataContainers.LevelsProgressData = progressDatas;
                _currentSectionProgressDataContainer = progressDataContainers;
                SaveCurrentSectionProgressDataContainer();
            }
            else
            {
                _currentSectionProgressDataContainer =LoadSectionProgressDataContainer(targetSection);
            }
        }
        
        private string GetSectionContainerKey(LevelSection section)
        {
            var sectionId = (int)section;
            return LEVELS_SECTION_DATA_CONTAINER_KEY + "_" + sectionId;
        }

        public void SaveGridBlockStateData(BlockType[,] gridBlocksData)
        {
        //    _levelProgressDataMap[levelId].LevelData.BlocksData = gridBlocksData;
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
}
