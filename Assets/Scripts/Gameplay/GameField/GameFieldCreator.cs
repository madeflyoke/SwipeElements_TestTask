using System;
using System.Linq;
using EasyButtons;
using Gameplay.Blocks.Enums;
using Gameplay.Factories;
using Gameplay.Levels.Data;
using Signals;
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
        private LevelData _levelData;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Awake()
        {
            _signalBus.Subscribe<LevelStartedSignal>(Initialize);
        }
        
        private void Initialize(LevelStartedSignal signal)
        {
            TryResetGameField();
                
            _levelData = signal.LevelData;
            _gridHolder.Create(_levelData.GridWidth, _levelData.GridHeight, Constants.CELL_SIZE);
            
            CreateBlocks();
            SetGroundedOffset();
            AdjustSize();
            
            _gridHolder.OnGameFieldReady();
        }

        private void CreateBlocks()
        {
            _gridHolder.Cells.ForEach((i,x, y) =>
            {
                var blockType = _levelData.BlocksData[x, y];
                if (blockType!=BlockType.NONE)
                {
                    var block = _blocksSpawner.Spawn(blockType, i.transform);
#pragma warning disable CS4014
                    i.SetBlock(block, false);
#pragma warning restore CS4014
                }
            });
        }

        private void SetGroundedOffset()
        {
            var gridPos = _gridHolder.transform.position;
            gridPos.y = _groundPoint.position.y;
            _gridHolder.SetAdjustedPosition(gridPos);
        }

        private void AdjustSize()
        {
            var cam = Camera.main;
             
            var screenLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, 0));
            
            var targetX = screenLeft.x + .1f;
        
            var leftCellX = _gridHolder.Cells.ToList().Min(x => x.transform.position.x);
            leftCellX -= Constants.CELL_SIZE / 2;
             
            var scale = _gridHolder.transform.localScale.x * (targetX / leftCellX);
            _gridHolder.SetAdjustedScale(Vector3.one*scale);
        }

        private void TryResetGameField()
        {
            if (_gridHolder)
            {
                _gridHolder.ResetGrid();
            }
        }

        private void OnDisable()
        {
            _signalBus.TryUnsubscribe<LevelStartedSignal>(Initialize);
            _signalBus.TryUnsubscribe<LevelCompletedSignal>(TryResetGameField);
        }


#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (_levelData==null || _gridHolder.Cells==null)
            {
                return;
            }

            Gizmos.color = Color.green;
            foreach (var cell in _gridHolder.Cells)
            {
                Gizmos.DrawSphere(cell.transform.position,0.1f);
            }
        }
        
        #endif
    }
}
