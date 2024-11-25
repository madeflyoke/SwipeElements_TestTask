using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Levels.Enums;

namespace Gameplay.Levels.Data
{
    [Serializable]
    public class SectionDataContainer
    {
        public LevelSection Section;
        public readonly List<LevelData> Data;

        public SectionDataContainer(List<LevelData> data)
        {
            Data = data;
        }

        public SectionDataContainer()
        {
            Data = new List<LevelData>();
        }

        public LevelData GetLevelData(int levelId)
        {
            return Data.FirstOrDefault(x => x.LevelId == levelId);
        }
        
#if UNITY_EDITOR
        public LevelData AddNextLevelData_Editor()
        {
            var newData = new LevelData(Data.Count);
            Data.Add(newData);
            return newData;
        }
        
        public void AddLevelData_Editor(LevelData data)
        {
            if (Data.Any(x=>x.LevelId==data.LevelId))
            {
               var index =Data.IndexOf(Data.FirstOrDefault(x => x.LevelId == data.LevelId));
               Data[index] = data;
            }
            else
            {
                Data.Add(data);
            }
        }
#endif
    }
}
