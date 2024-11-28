using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gameplay.Levels.Enums;
using Utility;

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
            var originalData = Data.FirstOrDefault(x => x.LevelId == levelId);
         
            return originalData?.Copy();
        }
        
        public string GetUniqueKey(int levelId)
        {
            var level = GetLevelData(levelId);
            StringBuilder sb = new StringBuilder();
            sb
                .Append(level.LevelId)
                .Append((int) Section);

            level.BlocksData.ForEach((e,x,y) =>
            {
                sb.Append((int) e)
                    .Append(x)
                    .Append(y);
            });

            return sb.ToString();
        }

        public bool IsLastLevel(int index)
        {
            return Data.Count - 1 == index;
        }
        
#if UNITY_EDITOR
        public LevelData AddNextLevelData_Editor()
        {
            var newData = new LevelData(Data.Count);
            Data.Add(newData);
            return newData;
        }

        public void RemoveLevelData_Editor(LevelData levelData)
        {
            if (Data.Any(x=>x.LevelId==levelData.LevelId))
            {
                Data.Remove(levelData);
                for (int i = 0; i < Data.Count; i++)
                {
                    Data[i].LevelId = i;
                }
            }
        }
#endif
    }
}
