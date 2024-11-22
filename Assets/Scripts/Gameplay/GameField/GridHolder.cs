using System.Collections.Generic;
using Gameplay.Blocks;
using UnityEngine;

namespace Gameplay.GameField
{
    public class GridHolder : MonoBehaviour
    {
        public Dictionary<Vector2Int, GridCell> Cells { get; set; }

        public void Create(int width, int height, float cellSize)
        {
            Cells = new Dictionary<Vector2Int, GridCell>();
            
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
                    
                    Cells.Add(coord,cell);
                }
            }
        }
        
        public void SetBlock(Block block, Vector2Int coord)
        {
            Cells[coord].SetBlock(block);
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            
        }

#endif
    }
}
