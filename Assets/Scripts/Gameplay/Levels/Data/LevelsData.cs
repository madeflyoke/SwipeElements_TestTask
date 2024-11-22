using System;
using System.Collections.Generic;
using System.Linq;

namespace Gameplay.Levels.Data
{
    [Serializable]
    public class LevelsData
    {
        public readonly List<GridBlocksLevelData> Data;

        public LevelsData(List<GridBlocksLevelData> data)
        {
            Data = data;
        }

        public LevelsData()
        {
            Data = new List<GridBlocksLevelData>();
        }

        public GridBlocksLevelData GetLevelData(int levelId)
        {
            return Data.FirstOrDefault(x => x.LevelId == levelId);
        }
        
        public GridBlocksLevelData AddNextLevelData()
        {
            var newData = new GridBlocksLevelData(Data.Count);
            Data.Add(newData);
            return newData;
        }
        
        public void AddLevelData(GridBlocksLevelData data)
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
