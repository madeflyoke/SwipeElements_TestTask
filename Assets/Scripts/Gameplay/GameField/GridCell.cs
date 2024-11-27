using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Blocks;
using Gameplay.Blocks.Enums;
using UnityEngine;

namespace Gameplay.GameField
{
    public class GridCell : MonoBehaviour //general idea -> lock while busy
    {
        public BlockType RelatedBlockType => IsEmpty ? BlockType.NONE : CurrentBlock.Type;
        public bool BlockInsideAndReady => IsEmpty == false && IsBusy == false;
        
        public bool IsEmpty => CurrentBlock == null;
        public Vector2Int Coord { get; private set; }
        public Block CurrentBlock { get; private set; }
        public bool IsBusy { get; private set; }

        private int _relatedSortingOrder;
        private CancellationTokenSource _cts;

        public void Initialize(Vector2Int coord, int relatedSortingOrder)
        {
            Coord = coord;
            _cts = new CancellationTokenSource();
            _relatedSortingOrder = relatedSortingOrder;
        }

        public async UniTask SetBlock(Block block, bool animated)
        {
            CurrentBlock = block; //null is expected also

            if (block!=null)
            {
                block.SetSortingOrder(_relatedSortingOrder);
                
                if (animated)
                {
                    IsBusy = true;
                    block.MoveTo(transform.position, () =>
                    {
                        CurrentBlock.transform.SetParent(transform);
                        IsBusy = false;
                    });

                    await UniTask.WaitUntil(() => IsBusy == false, cancellationToken: _cts.Token)
                        .SuppressCancellationThrow();
                }
                else
                {
                    CurrentBlock.transform.SetParent(transform);
                    CurrentBlock.transform.localPosition = Vector3.zero;
                }
            }
        }

        public async UniTask DestroyRelatedBlock()
        {
            if (CurrentBlock != null)
            {
                IsBusy = true;
                CurrentBlock.BlockDestroyed += OnCurrentBlockDestroyed;
                CurrentBlock.StartDestroyingBlock();
                await UniTask.WaitUntil(() => IsBusy == false, cancellationToken: _cts.Token)
                    .SuppressCancellationThrow();
            }
        }

        private void OnCurrentBlockDestroyed()
        {
            CurrentBlock = null;
            IsBusy = false;
        }

        private void OnDisable()
        {
            _cts?.Cancel();
        }
    }
}