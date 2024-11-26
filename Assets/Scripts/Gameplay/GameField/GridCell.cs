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
        private int _relatedSortingOrder;
        
        public void Initialize(Vector2Int coord, int relatedSortingOrder)
        {
            Coord = coord;
            _relatedSortingOrder = relatedSortingOrder;
        }

        public void SetBlock(Block block, bool animated)
        {
            if (block==null)
            {
                CurrentBlock = block; //null is expected also
            }
            else
            {
                block.SetSortingOrder(_relatedSortingOrder);
                if (animated)
                {
                    IsBusy = true;
                    block.MoveTo(transform.position, () =>
                    {
                        CurrentBlock = block;
                        CurrentBlock.transform.SetParent(transform);
                        
                        IsBusy = false;
                        BlockMovementFinished?.Invoke();
                    });
                }
                else
                {
                    CurrentBlock = block; 
                    CurrentBlock.transform.SetParent(transform);
                    CurrentBlock.transform.localPosition = Vector3.zero;
                }
            }
        }

        public void Clear()
        {
            if (CurrentBlock!=null)
            {
                IsBusy = true;
                CurrentBlock.BlockDestroyed += OnCurrentBlockDestroyed;
                CurrentBlock.StartDestroyingBlock();
            }
        }

        private void OnCurrentBlockDestroyed()
        {
            CurrentBlock = null;
            IsBusy = false;
            BlockDestroyFinished?.Invoke();
        }
    }
}
