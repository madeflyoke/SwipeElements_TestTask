using EasyButtons;
using Lean.Pool;
using UnityEngine;

namespace Gameplay.GameField.Background
{
    public class BalloonsSpawner : MonoBehaviour
    {
        [SerializeField] private BalloonsConfig _balloonsConfig;
        [SerializeField] private Transform _balloonsParent;
        private Camera _cam;
        private float _screenBoundsOffset = 0.2f;

        private void Awake()
        {
            _cam = Camera.main;
            transform.position = _cam.transform.position;
        }
        
        [Button]
        public Balloon SpawnRandomBalloon()
        {
            var targetBalloonSprite = _balloonsConfig.BalloonsVariantsSprites[Random.Range(0, _balloonsConfig.BalloonsVariantsSprites.Count)];

            var spriteWidth = targetBalloonSprite.bounds.size.x;
            var spriteHeight = targetBalloonSprite.bounds.size.y;
            
            var targetBounds = GetBoundsForTarget(spriteWidth, spriteHeight);
            var randomY = Random.Range(targetBounds.min.y, targetBounds.max.y);
            
            var minX = targetBounds.min.x + spriteWidth/2;
            var maxX = targetBounds.max.x - spriteWidth/2;
            
            var randomSide = Random.Range(0, 2);
            var targetFromX = randomSide == 0 ? minX : maxX;
            var targetToX = randomSide == 0 ? maxX : minX;
            
            var fromPosition = new Vector3(targetFromX, randomY, 0f);

            randomY = _balloonsConfig.StraightDirection
                ? randomY
                : Random.Range(targetBounds.min.y, targetBounds.max.y);
            
            var toPosition = new Vector3(targetToX, randomY, 0f);
            
            var speed = Random.Range(_balloonsConfig.MinSpeed, _balloonsConfig.MaxSpeed);
            
            var balloon = LeanPool.Spawn(_balloonsConfig.Prefab, fromPosition, Quaternion.identity, _balloonsParent);
            balloon.SetData(targetBalloonSprite, new Balloon.BalloonData()
            {
                AutoDespawn = true,
                Destination = toPosition,
                Speed = speed,
                PathSineAmplitude = _balloonsConfig.PathSineAmplitude,
                PathSineFrequency = _balloonsConfig.PathSineFrequency
            });
            return balloon;
        }

        public void Clear()
        {
            for (int i = _balloonsParent.childCount-1; i >=0; i--)
            {
                LeanPool.Despawn(_balloonsParent.GetChild(i));
            }
        }
        
        //if sprites will be different size, but actually i'd just take far-away-enough point and spawn there
        private Bounds GetBoundsForTarget(float spriteWidth, float spriteHeight) 
        {
            var spriteDoubleWidth = (spriteWidth) * 2f;
            var spriteDoubleHeight = (spriteHeight) * 2f;
            
            var height = _cam.orthographicSize * 2f;
            var width = height * _cam.aspect;

            var finalHeight = height - spriteDoubleHeight - _screenBoundsOffset;
            var finalWidth = width + spriteDoubleWidth +_screenBoundsOffset;

            return new Bounds(_cam.transform.position, new Vector3(finalWidth, finalHeight));
        }
    }
}
