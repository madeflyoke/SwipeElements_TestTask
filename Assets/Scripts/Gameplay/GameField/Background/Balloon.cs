using System;
using Lean.Pool;
using UnityEngine;

namespace Gameplay.GameField.Background
{
    public class Balloon : MonoBehaviour
    {
        public event Action<Balloon> OutOfScreen;
        
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private BalloonData _balloonData;
        private bool _canMove;

        public void SetData(Sprite sprite, BalloonData balloonData)
        {
            _spriteRenderer.sprite = sprite;
            _balloonData = balloonData;
        }
        
        public void SetMovementState(bool value)
        {
            _canMove = value;
        }

        private void Update()
        {
            if (_canMove)
            {
                if (Mathf.Abs(_balloonData.Destination.x - transform.position.x) <= 0.02f)
                {
                    SetMovementState(false);
                    OutOfScreen?.Invoke(this);
                    if (_balloonData.AutoDespawn)
                    {
                        LeanPool.Despawn(gameObject);
                    }
                }
                
                var direction = (_balloonData.Destination - transform.position);
                direction.z = 0;
                transform.position += direction.normalized * _balloonData.Speed * Time.deltaTime;
                
                var posY = Mathf.Sin(Time.time * _balloonData.PathSineFrequency) 
                           * _balloonData.PathSineAmplitude * Time.deltaTime;
                transform.position += Vector3.up*posY;
            }
        }
        
        private void OnDisable()
        {
            OutOfScreen = null;
        }

        public struct BalloonData
        {
            public float Speed;
            public Vector3 Destination;
            public bool AutoDespawn;
            public float PathSineAmplitude;
            public float PathSineFrequency;
        }
    }
}
