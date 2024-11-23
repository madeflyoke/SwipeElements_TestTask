using System;
using System.Collections.Generic;
using System.Linq;

namespace Gameplay.Levels.Data
{
    [Serializable]
    public class LevelsDataContainer
    {
        public readonly List<LevelData> Data;

        public LevelsDataContainer(List<LevelData> data)
        {
            Data = data;
        }

        public LevelsDataContainer()
        {
            Data = new List<LevelData>();
        }

        public LevelData GetLevelData(int levelId)
        {
            return Data.FirstOrDefault(x => x.LevelId == levelId);
        }
        
        public LevelData AddNextLevelData()
        {
            var newData = new LevelData(Data.Count);
            Data.Add(newData);
            return newData;
        }
        
        public void AddLevelData(LevelData data)
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
    }
}
