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

        public LevelSection GetNextSection(LevelSection currentSection)
        {
            var nextId = _sections.IndexOf(currentSection) + 1;
            return _sections[Mathf.Clamp(nextId,0, _sections.Count-1)];
        }
    }
}
