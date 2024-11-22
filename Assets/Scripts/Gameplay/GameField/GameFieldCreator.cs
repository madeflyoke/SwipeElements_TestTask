using System.Linq;
using EasyButtons;
using Gameplay.Blocks.Enums;
using Gameplay.Factories;
using Gameplay.Levels.Data;
using Services;
using UnityEngine;
using Utility;
using Zenject;

namespace Gameplay.GameField
{
    public class GameFieldCreator : MonoBehaviour
    {
        [SerializeField] private BlocksSpawner _blocksSpawner;
        [SerializeField] private GridHolder _gridHolder;
        [SerializeField] private Transform _groundPoint;
        private GridBlocksLevelData _levelData;

        [Inject]
        public void Construct(ServicesHolder servicesHolder)
        {
#if UNITY_EDITOR
            _levelData = EditorResourcesManager.LoadJsonDataAsset<LevelsData>().GetLevelData(0);
#else
            _levelData = servicesHolder.GetService<AssetsProviderService>().LoadJsonDataAsset<LevelsData>().GetLevelData(0);
#endif
        }
        
        [Button]
        public void Initialize(GridBlocksLevelData levelData)
        {
            _gridHolder.Create(_levelData.GridWidth, _levelData.GridHeight, Constants.CELL_SIZE);
            
            CreateBlocks();
            SetGroundedOffset();
            AdjustSize();
        }

        private void CreateBlocks()
        {
            foreach (var kvp in _gridHolder.Cells)
            {
                var blockType = _levelData.BlocksData[kvp.Key.x, kvp.Key.y];
                if (blockType!=BlockType.NONE)
                {
                    var block = _blocksSpawner.Spawn(blockType, kvp.Value.transform);
                    kvp.Value.SetBlock(block);
                }
            }
        }

        private void SetGroundedOffset()
        {
            var gridPos = _gridHolder.transform.position;
            gridPos.y = _groundPoint.position.y;
            _gridHolder.transform.position = gridPos;
        }

        private void AdjustSize()
        {
            var cam = Camera.main;
             
            var screenLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
            
            var targetX = screenLeft.x + .1f;
        
            var leftCellX = _gridHolder.Cells.Values.Min(x => x.transform.position.x);
            leftCellX -= Constants.CELL_SIZE / 2;
             
            var scale = _gridHolder.transform.localScale.x * (targetX / leftCellX);
            _gridHolder.transform.localScale = Vector3.one*scale;
        }
        
        #if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (_levelData==null || _gridHolder.Cells==null)
            {
                return;
            }

            Gizmos.color = Color.green;
            foreach (var kvp in _gridHolder.Cells)
            {
                Gizmos.DrawSphere(kvp.Value.transform.position,0.1f);
            }
        }
        
        #endif
    }
}
