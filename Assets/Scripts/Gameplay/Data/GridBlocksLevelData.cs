using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Enums;
using UnityEngine;

namespace Gameplay.Data
{
    [Serializable]
    public class GridBlocksLevelData
    {
        public int LevelId;
        public BlockType[,] BlocksDatas;
        
        public GridBlocksLevelData(int levelId)
        {
            LevelId = levelId;
            BlocksDatas = new BlockType[0,0];
        }

        public GridBlocksLevelData()
        {
            
        }
        
        public void AddData(int xPos, int yPos, BlockType type)
        {
            BlockType[,] newArray = null;
            
            void CopyData()
            {
                for (int x = 0; x < BlocksDatas.GetLength(0); x++)
                {
                    for (int y = 0; y < BlocksDatas.GetLength(1); y++)
                    {
                        newArray[x, y] = BlocksDatas[x, y];
                    }
                }

                BlocksDatas = newArray;
            }
            
            if (xPos >= BlocksDatas.GetLength(0))
            {
                newArray = new BlockType[xPos + 1, BlocksDatas.GetLength(1)];
                CopyData();
            }

            if (yPos >= BlocksDatas.GetLength(1))
            {
                newArray = new BlockType[BlocksDatas.GetLength(0), yPos + 1];
                CopyData();
            }

            BlocksDatas[xPos, yPos] = type;
        }

        public void ClearData()
        {
            BlocksDatas = new BlockType[0, 0];
        }
    }
}
