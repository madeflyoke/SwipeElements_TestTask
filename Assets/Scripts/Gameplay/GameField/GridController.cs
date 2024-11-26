using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Blocks;
using Gameplay.Blocks.Enums;
using Services;
using Services.Input;
using Services.Input.Enums;
using Services.Progress;
using Services.Progress.Levels;
using Signals;
using UnityEngine;
using Utility;
using Zenject;

namespace Gameplay.GameField
{
    public class GridController : IDisposable
    {
        private readonly GridCell[,] _cells; //cells never moved, we control actual Blocks inside of them
        private int _xMax => _cells.GetLength(0);
        private int _yMax=> _cells.GetLength(1);
        
        private readonly Camera _camera;
        private InputService _inputService;
        private GridCell _lastPerformedCell; //used to track last cell that started animations to receive callbacks when its finished

        private int _aliveBlocksCount;
        private SignalBus _signalBus;
        
        public GridController(GridCell[,] cells)
        {
            _cells = cells;
            _camera = Camera.main;
        }
        
        [Inject]
        public void Construct(ServicesHolder servicesHolder, SignalBus signalBus)
        {
            _inputService = servicesHolder.GetService<InputService>();
            _inputService.SwipePerformed += OnInputSwipePerformed;
            _signalBus = signalBus;
        }

        public void OnGameFieldReady()
        {
            _cells.ForEach((e,x,y) =>
            {
                if (e.RelatedBlockType!=BlockType.NONE)
                {
                    _aliveBlocksCount++;
                }
            });
        }
        
        private void OnSwap()
        {
            _lastPerformedCell.BlockMovementFinished -= OnSwap;

            if (TryBlocksFalling()==false)
            {
                HandleMatches();
            }
        }
        
        private void HandleMatches()
        {
            var matchCells = FindMatchedCells();
            if (matchCells.Count>0)
            {
                var lastCell = matchCells[^1];
                
                _lastPerformedCell.BlockDestroyFinished -= OnBlocksDestroyed;
                _lastPerformedCell = lastCell;
                _lastPerformedCell.BlockDestroyFinished += OnBlocksDestroyed;
                
                matchCells.ForEach(cell =>
                {
                    Mathf.Clamp(--_aliveBlocksCount, 0,int.MaxValue);
                    cell.Clear();
                });
            }
            else
            {
                CallOnGameFieldChanged(); //no matches normalize point 
            }
        }
        
        private void OnBlocksDestroyed()
        {
            _lastPerformedCell.BlockDestroyFinished -= OnBlocksDestroyed;
            if (TryBlocksFalling()==false)
            {
                CallOnGameFieldChanged(); //no falling normalize point
                TryCallOnGameplayCompleted();
            }
        }

#region FALLING

        private bool TryBlocksFalling()
        {
            var fallingPerformed = false;
            for (int xCoord = 0; xCoord < _xMax; xCoord++)
            {
                var lowestEmptyCellY = FindLowestEmptyCellY(xCoord);
                var lowestFilledCellY = FindLowestFilledCellY(xCoord, lowestEmptyCellY);

                if (lowestEmptyCellY==-1 || lowestFilledCellY==-1)
                {
                    continue;
                }
        
                var offset = lowestFilledCellY - lowestEmptyCellY;
                
                for (int yCoord = lowestFilledCellY; yCoord < _yMax; yCoord++)
                {
                    var fromCell = _cells[xCoord, yCoord];
                    if (fromCell.IsEmpty)
                    {
                        continue;
                    }
                    var toCell = _cells[xCoord, yCoord - offset];
                    SwapCells(fromCell,toCell);
                    fallingPerformed = true;
                }
            }

            return fallingPerformed;
        }
        
        private int FindLowestEmptyCellY(int x)
        {
            for (int y = 0; y < _yMax; y++)
            {
                if (_cells[x, y].IsEmpty)
                    return y;
            }
            return -1;
        }

        private int FindLowestFilledCellY(int x, int startY)
        {
            for (int y = startY + 1; y < _yMax; y++)
            {
                if (_cells[x, y].IsEmpty==false)
                    return y;
            }
            return -1;
        }

#endregion

        
#region SWIPE AND SWAP

         private void OnInputSwipePerformed(Vector3 screenStartPos, SwipeDirection swipeDirection)
        {
            var worldPos = (Vector2) _camera.ScreenToWorldPoint(screenStartPos);
            var hit = Physics2D.Raycast(worldPos,Vector2.zero, 1f ,layerMask:~Constants.Layers.BLOCK);

            if (hit.collider!=null)
            {
                var block = hit.collider.GetComponent<Block>();
                SwipeBlockLogic(block, swipeDirection);
            }
        }

        private void SwipeBlockLogic(Block fromBlock, SwipeDirection direction)
        {
            var fromCell = _cells.Find((x) => x.CurrentBlock == fromBlock);
            if (fromCell == null)
            {
                Debug.LogError("Invalid cell!");
                return;
            }
            
            var toCoord = fromCell.Coord;
            ProcessCoordByDirection(ref toCoord, direction);
            
            if (TryGetNotBusyCell(toCoord, out GridCell toCell) 
                && (direction==SwipeDirection.UP && toCell.IsEmpty)==false)
            {
                SwapCells(fromCell, toCell);
            }
        }

        private void SwapCells(GridCell cell1, GridCell cell2)
        {
            var cell1Block = cell1.CurrentBlock;
            var cell2Block = cell2.CurrentBlock;
            
            if (_lastPerformedCell!=null)
            {
                _lastPerformedCell.BlockMovementFinished -= OnSwap;
            }
            _lastPerformedCell = cell2;
            _lastPerformedCell.BlockMovementFinished += OnSwap;
            
            cell1.SetBlock(cell2Block, true);
            cell2.SetBlock(cell1Block, true);
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

        private bool TryGetNotBusyCell(Vector2Int coord, out GridCell cell)
        {
            cell = null;
            if (_cells.IsInBounds(coord.x,coord.y))
            {
                var targetCell =_cells[coord.x, coord.y];
                if (targetCell.IsBusy==false)
                {
                    cell = targetCell;
                    return true;
                }
            }

            return false;
        }

#endregion
       
        
#region MATCH CHECK

        private List<GridCell> FindMatchedCells()
        {
            var matches = new List<GridCell>();
            var visited = new bool[_xMax, _yMax];

            _cells.ForEach((cell, x, y) =>
            {
                if (visited[x, y]==false && cell.IsEmpty==false)
                {
                    var match = FloodFillMatch(new Vector2Int(x,y), _cells[x, y].RelatedBlockType, visited);
                        
                    if (match!=null&&match.Count >= Constants.BLOCKS_MATCH_COUNT)
                    {
                        matches.AddRange(match);
                    }
                }
            });
            
            return matches;
        }
        
        //check for Flood fill algorithm (kind of)
        private List<GridCell> FloodFillMatch(Vector2Int coord, BlockType blockType, bool[,] visited)
        {
            var matches = new List<GridCell>();
            var queue = new Queue<Vector2Int>(); //tuple?
    
            queue.Enqueue(coord);
    
            while (queue.Count > 0)
            {
                var targetElementCoord = queue.Dequeue();
                var targetX = targetElementCoord.x;
                var targetY = targetElementCoord.y;
        
                if (_cells.IsInBounds(targetX,targetY)==false ||
                    visited[targetX, targetY] ||
                    _cells[targetX,targetY].IsEmpty ||
                    _cells[targetX,targetY].RelatedBlockType != blockType)
                    continue;

                visited[targetX, targetY] = true;
                matches.Add(_cells[targetX,targetY]);
                
                queue.Enqueue(new Vector2Int(targetX+1, targetY)); //heavy instances?
                queue.Enqueue(new Vector2Int(targetX-1, targetY));
                queue.Enqueue(new Vector2Int(targetX, targetY+1));
                queue.Enqueue(new Vector2Int(targetX, targetY-1));
            }

            matches = GetContinuousMatches(matches);
            return matches;
        }
        
        private List<GridCell> GetContinuousMatches(List<GridCell> matches)
        {
            if (matches.GroupBy(cell=>cell.Coord.x).Any(group=>group.Count()>=Constants.BLOCKS_MATCH_COUNT) ||
                matches.GroupBy(cell=>cell.Coord.y).Any(group=>group.Count()>=Constants.BLOCKS_MATCH_COUNT))
            {
                return matches;
            }

            return null;
        }
        
        //Default recursion (maybe)
        
        // private void MatchRecursive(int x, int y, BlockType blockType, List<GridCell> match, bool[,] visited) 
        // {
        //     if (x < 0 || x >= _xMax || 
        //         y < 0 || y >= _yMax || 
        //         visited[x, y] ||
        //         _cells[x,y].RelatedBlockType==BlockType.NONE||
        //         _cells[x, y].RelatedBlockType != blockType)
        //         return;
        //
        //     visited[x, y] = true;
        //     match.Add(_cells[x,y]);
        //     
        //     MatchRecursive(x + 1, y, blockType, match, visited);
        //     MatchRecursive(x - 1, y, blockType, match, visited);
        //     MatchRecursive(x, y + 1, blockType, match, visited); 
        //     MatchRecursive(x, y - 1, blockType, match, visited); 
        // }


#endregion

        private void CallOnGameFieldChanged()
        {
            var gridBlocksData = new BlockType[_cells.GetLength(0), _cells.GetLength(1)];
            
            _cells.ForEach((e, x, y) =>
            {
                gridBlocksData[x, y] = e.RelatedBlockType;
            });
            
            _signalBus.Fire(new GameFieldChangedSignal(gridBlocksData));
        }

        private void TryCallOnGameplayCompleted()
        {
            if (_aliveBlocksCount<=0)
            {
                _signalBus.Fire(new LevelCompletedSignal());
            }
        }
        

        public void Dispose()
        {
            _inputService.SwipePerformed -= OnInputSwipePerformed;
            if (_lastPerformedCell!=null)
            {
                _lastPerformedCell.BlockMovementFinished -= OnSwap;
                _lastPerformedCell.BlockDestroyFinished -= OnBlocksDestroyed;
                _lastPerformedCell = null;
            }
        }
    }
}
