using System.Collections.Generic;
using Gameplay.Levels.Enums;
using UnityEngine;

namespace Gameplay.Levels.Data
{
    [CreateAssetMenu(fileName = "LevelSectionsContainer", menuName = "Gameplay/Levels/SectionsContainer")]
    public class SectionsContainer : ScriptableObject
    {
        [SerializeField] private List<LevelSection> _sections;

        public LevelSection GetSection(int index)
        {
            return _sections[index];
        }
    }
}
