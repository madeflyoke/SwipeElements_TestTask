using System;
using DG.Tweening;
using Gameplay.Blocks;
using Gameplay.Blocks.Enums;
using UnityEngine;

namespace Gameplay.GameField
{
    public class GridCell : MonoBehaviour
    {
        public event Action BlockMovementFinished;
        public event Action BlockDestroyFinished;
        
        public BlockType RelatedBlockType => IsEmpty? BlockType.NONE : CurrentBlock.Type;
        public bool IsEmpty => CurrentBlock == null;
        public Vector2Int Coord { get; private set; }
        public Block CurrentBlock { get; private set; }
        public bool IsBusy { get; private set; }
        private Tween _movingTween;
        
        public void Initialize(Vector2Int coord)
        {
            Coord = coord;
        }

        public void SetBlock(Block block, bool animated)
        {
            CurrentBlock = block; //null is expected also
            if (block!=null)
            {
                if (animated)
                {
                    IsBusy = true;
                    block.SetBusy(true);
                    _movingTween = block.transform.DOMove(transform.position, 3f)
                        //.SetEase(Ease.OutElastic,amplitude:0.05f,period:0.25f) //TODO Config
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                    {
                        CurrentBlock.transform.SetParent(transform);
                        block.SetBusy(false);
                        IsBusy = false;
                        BlockMovementFinished?.Invoke();
                    });
                }
                else
                {
                    CurrentBlock.transform.SetParent(transform);
                    CurrentBlock.transform.localPosition = Vector3.zero;
                }
            }
        }

        public void Clear()
        {
            CurrentBlock.DestroyBlock();
            CurrentBlock = null;
            BlockDestroyFinished?.Invoke();
        }

        private void OnDisable()
        {
            _movingTween?.Kill();
        }
    }
}
