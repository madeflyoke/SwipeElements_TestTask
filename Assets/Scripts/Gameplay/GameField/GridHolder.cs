using System;
using System.Collections.Generic;
using EasyButtons;
using Gameplay.Blocks;
using Services;
using Services.InputService;
using Services.InputService.Enums;
using UnityEngine;
using Utility;
using Zenject;

namespace Gameplay.GameField
{
    public class GridHolder : MonoBehaviour
    {
        public GridCell[,] Cells { get; private set; }
        private GridController _gridController;

        public void Create(int width, int height, float cellSize)
        {
            Cells = new GridCell[width,height];
            
            for (int xCoord = 0; xCoord < width; xCoord++)
            {
                for (int yCoord = 0; yCoord < height; yCoord++)
                {
                    var coord = new Vector2Int(xCoord, yCoord);
                    var halfCellSize = cellSize / 2f;

                    Vector2 offset = new Vector2(
                        (width - 1) *halfCellSize, -halfCellSize -transform.position.y);
                
                    Vector2 worldPos = coord;
                    worldPos *= cellSize;
                    worldPos -= offset;

                    GridCell cell = new GameObject($"Cell_{xCoord}:{yCoord}").AddComponent<GridCell>();
                    cell.transform.position = worldPos;
                    cell.transform.SetParent(transform);
                    cell.Initialize(coord);

                    Cells[xCoord, yCoord] = cell;
                }
            }

            _gridController = ProjectContext.Instance.Container.Instantiate<GridController>(new object[]{Cells});
        }

        [Button]
        private void Match()
        {
            _gridController.HandleMatches();
        }

        [Button]
        private void Fall()
        {
            _gridController.HandleFalling();
        }
        
        public void OnDisable()
        {
            _gridController?.Dispose();
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            
        }

#endif
    }
}
