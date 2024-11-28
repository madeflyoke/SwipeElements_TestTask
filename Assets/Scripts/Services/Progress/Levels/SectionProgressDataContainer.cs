using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Levels.Enums;
using Newtonsoft.Json;

namespace Services.Progress.Levels
{
    [Serializable]
    public class SectionProgressDataContainer
    {
        [JsonIgnore] public bool SectionCompleted => LevelsProgressData.All(x => x.IsCompleted);
        
        public LevelSection Section;
        public List<LevelProgressData> LevelsProgressData;
        
        [JsonIgnore] public Dictionary<int, LevelProgressData> LevelsProgressDataMap =>
            _levelsProgressDataMap ??=LevelsProgressData.ToDictionary(x => x.LevelId, z => z);
        [JsonIgnore] private Dictionary<int, LevelProgressData> _levelsProgressDataMap;

        public SectionProgressDataContainer(List<LevelProgressData> levelProgressDatas, LevelSection section)
        {
            Section = section;
            LevelsProgressData = levelProgressDatas;
        }
        
        public SectionProgressDataContainer(){}
    }
}
