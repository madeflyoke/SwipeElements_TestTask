using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GameField.Background
{
    [CreateAssetMenu(fileName = "BalloonsConfig", menuName = "Gameplay/Balloons/BalloonsConfig")]
    public class BalloonsConfig : ScriptableObject
    {
        [field: SerializeField] public Balloon Prefab { get; private set; }
        [field: SerializeField] public List<Sprite> BalloonsVariantsSprites { get; private set; }
        
        
        [field:SerializeField] public int MaxBalloonsCount { get; private set; }
        [field:SerializeField] public int SpawnDelaySeconds { get; private set; }

        [field: SerializeField] public bool StraightDirection { get; private set; } = true;
        [field:SerializeField] public float MinSpeed { get; private set; }
        [field:SerializeField] public float MaxSpeed { get; private set; }
        [field:SerializeField] public float PathSineAmplitude { get; private set; }
        [field:SerializeField] public float PathSineFrequency { get; private set; }
        
        
        
        
        
    }
}
