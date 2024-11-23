using System;
using System.Collections.Generic;
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

        private InputService _inputService;
        private Camera _camera;

        [Inject]
        public void Construct(ServicesHolder servicesHolder)
        {
            _inputService = servicesHolder.GetService<InputService>();
            _inputService.SwipePerformed += OnSwipePerformed;
            _camera = Camera.main;
        }

        public void OnDisable()
        {
            _inputService.SwipePerformed -= OnSwipePerformed;
        }
        
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
        }
        
        private void OnSwipePerformed(Vector3 screenStartPos, SwipeDirection swipeDirection)
        {
            var worldPos = (Vector2) _camera.ScreenToWorldPoint(screenStartPos);
            var hit = Physics2D.Raycast(worldPos,Vector2.zero, 1f ,layerMask:~Constants.Layers.BLOCK);

            if (hit.collider!=null)
            {
                var block = hit.collider.GetComponent<Block>();
                SwipeBlockLogic(block, swipeDirection);
                Debug.LogWarning(hit.transform.gameObject.name);
            }
        }

        private void SwipeBlockLogic(Block fromBlock, SwipeDirection direction)
        {
            var fromCell = Cells.Find((x) => x.CurrentBlock == fromBlock);
            if (fromCell == null)
            {
                Debug.LogError("Invalid cell!");
                return;
            }

            var toCoord = fromCell.Coord;
            ProcessCoordByDirection(ref toCoord, direction);
            
            if (TryGetCell(toCoord, out GridCell toCell))
            {
                SwapCells(fromCell, toCell);
                Debug.LogWarning($"Swapped from {fromCell.Coord.x}{fromCell.Coord.y} to {toCoord.x}{toCoord.y}");
            }
        }

        private void SwapCells(GridCell cell1, GridCell cell2)
        {
            var cell1Block = cell1.CurrentBlock;
            var cell2Block = cell2.CurrentBlock;
            
            cell1.SetBlock(cell2Block);
            cell2.SetBlock(cell1Block);
        }
        
        private void ProcessCoordByDirection(ref Vector2Int sourceCoord, SwipeDirection direction)
        {
            switch (direction)
            {
                case SwipeDirection.LEFT:
                    sourceCoord.x -= 1;
                    break;
                case SwipeDirection.RIGHT:
                    sourceCoord.x += 1;
                    break;
                case SwipeDirection.UP:
                    sourceCoord.y += 1;
                    break;
                case SwipeDirection.DOWN:
                    sourceCoord.y -= 1;
                    break;
            }
        }

        private bool TryGetCell(Vector2Int coord, out GridCell cell)
        {
            cell = null;
            if (Cells.IsInBounds(coord.x,coord.y))
            {
                cell = Cells[coord.x, coord.y];
                return true;
            }

            return false;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            
        }

#endif
    }
}
