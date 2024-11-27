using System;
using Gameplay.Blocks.Enums;
using Newtonsoft.Json;
using Utility;

namespace Gameplay.Levels.Data
{
    [Serializable]
    public class LevelData
    {
        [JsonIgnore] public int GridWidth => BlocksData.GetLength(0);
        [JsonIgnore] public int GridHeight => BlocksData.GetLength(1);
        
        public int LevelId;
        public BlockType[,] BlocksData;
        
        public LevelData(int levelId)
        {
            LevelId = levelId;
            BlocksData = new BlockType[0,0];
        }

        public LevelData()
        {
            BlocksData = new BlockType[0,0];
        }

        public LevelData Copy()
        {
            return new LevelData()
            {
                LevelId = this.LevelId,
                BlocksData = this.BlocksData
            };
        }

#if UNITY_EDITOR
        public void AddData_Editor(int xCoord, int yCoord, BlockType type)
        {
            BlockType[,] newArray = null;
            
            void CopyData()
            {
                BlocksData.ForEach((x, y) =>
                {
                    newArray[x, y] = BlocksData[x, y];
                });
                
                BlocksData = newArray;
            }
            
            if (xCoord >= GridWidth) //flexible expand
            {
                newArray = new BlockType[xCoord + 1, GridHeight];
                CopyData();
            }

            if (yCoord >= GridHeight)
            {
                newArray = new BlockType[GridWidth, yCoord + 1];
                CopyData();
            }

            BlocksData[xCoord, yCoord] = type;
        }
        
        public void ShiftBlocks_Editor(int shiftX, int shiftY)
        {
            var newBlocksData = new BlockType[GridWidth, GridHeight];

            for (var x = 0; x < GridWidth; x++)
            {
                for (var y = 0; y < GridHeight; y++)
                {
                    var newX = x + shiftX;
                    var newY = y + shiftY;

                    if (newX >= 0 && newX < GridWidth && newY >= 0 && newY < GridHeight)
                    {
                        newBlocksData[newX, newY] = BlocksData[x, y];
                    }
                }
            }

            BlocksData = newBlocksData;
        }
        

        public void ClearData_Editor()
        {
            BlocksData = new BlockType[0, 0];
        }
#endif
    }
}
