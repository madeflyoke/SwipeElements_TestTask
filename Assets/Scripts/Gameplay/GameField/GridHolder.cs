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
        public Dictionary<Vector2Int, GridCell> Cells { get; private set; }
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
        
        private void OnSwipePerformed(Vector3 screenStartPos, SwipeDirection swipeDirection)
        {
            var worldPos = (Vector2) _camera.ScreenToWorldPoint(screenStartPos);
            var hit = Physics2D.Raycast(worldPos,Vector2.zero, 1f ,layerMask:~Constants.Layers.BLOCK);

            if (hit.collider!=null)
            {
                Debug.LogWarning(hit.transform.gameObject.name);
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
