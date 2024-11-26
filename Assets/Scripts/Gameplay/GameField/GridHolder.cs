using UnityEngine;
using Zenject;

namespace Gameplay.GameField
{
    public class GridHolder : MonoBehaviour
    {
        public GridCell[,] Cells { get; private set; }
        private GridController _gridController;
        
        private Transform _gridParent;
        private Vector3 _originalScale;
        private Vector3 _originalPosition;
        
        public void Create(int width, int height, float cellSize)
        {
            _originalScale = transform.localScale;
            _originalPosition = transform.position;
            
            Cells = new GridCell[width,height];
            _gridParent = new GameObject("GridParent").transform;
            _gridParent.SetParent(transform);
            
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
                    cell.transform.SetParent(_gridParent.transform);
                    cell.Initialize(coord);

                    Cells[xCoord, yCoord] = cell;
                }
            }

            _gridController = ProjectContext.Instance.Container.Instantiate<GridController>(new object[]{Cells});
        }
        
        public void OnGameFieldReady()
        {
            _gridController.OnGameFieldReady();
        }
        
        public void SetAdjustedPosition(Vector3 position)
        {
            transform.position =position;
        }
        
        public void SetAdjustedScale(Vector3 scale)
        {
            transform.localScale = scale;
        }

        public void ResetGrid()
        {
            _gridController?.Dispose();
            _gridController = null;
            Cells = null;
            if (_gridParent!=null)
            {
                _gridParent.gameObject.SetActive(false);
                Destroy(_gridParent.gameObject);
            }

            transform.position =_originalPosition;
            transform.localScale = _originalScale;
        }
        
        public void OnDisable()
        {
            _gridController?.Dispose();
        }
    }
}
