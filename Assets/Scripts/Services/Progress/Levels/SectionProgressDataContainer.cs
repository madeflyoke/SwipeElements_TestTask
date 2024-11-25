using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Levels.Enums;
using Newtonsoft.Json;

namespace Services.Progress.Levels
{
    [Serializable]
    public class SectionProgressDataContainer
    {
        [JsonIgnore] public bool SectionCompleted => LevelsProgressData.All(x => x.IsCompleted);
        
        public LevelSection Section;
        public List<LevelProgressData> LevelsProgressData;
    }
}
