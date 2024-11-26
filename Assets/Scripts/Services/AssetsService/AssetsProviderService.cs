using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Levels.Data;
using Gameplay.Levels.Enums;
using Newtonsoft.Json;
using Services.Interfaces;
using UnityEngine;
using Utility;

namespace Services.AssetsService
{
    public class AssetsProviderService : IService
    {
        private Dictionary<string, object> _cachedAssets;

        public UniTask Initialize(CancellationTokenSource cts)
        {
            _cachedAssets = new Dictionary<string, object>();
            return UniTask.CompletedTask;
        }

        public SectionDataContainer LoadLevelsSectionDataContainer(LevelSection section)
        {
            var postfix = section.ToString();
            var dataName = StringHelpers.BuildDataNameByType<SectionDataContainer>(postfix);
            return LoadJsonDataAsset<SectionDataContainer>(dataName);
        }

        public SectionsContainer LoadLevelSectionsContainer()
        {
            return LoadConfigAsset<SectionsContainer>(nameof(SectionsContainer));
        }

        private T LoadJsonDataAsset<T>(string dataName) where T: class
        {
            var path = Path.Combine(Constants.ResourcesPaths.DATA, dataName);
        
            if (_cachedAssets.ContainsKey(path))
            {
                return _cachedAssets[path] as T;
            }
            
            var json = Resources.Load<TextAsset>(path);

            if (json == null)
            {
                Debug.LogError($"Path not found: {path}");
                return null;
            }
            
            T jsonData = JsonConvert.DeserializeObject<T>(json.text);
            Resources.UnloadAsset(json);

            if (jsonData == null)
            {
                Debug.LogError($"Json cant be serialized: {path}");
            }
            
            _cachedAssets.Add(path, jsonData);
            return jsonData;
        }

        private T LoadConfigAsset<T>(string optionalName="", bool withUnload = true) where T: ScriptableObject
        {
            var path = Path.Combine(Constants.ResourcesPaths.CONFIGS, optionalName);

            if (_cachedAssets.ContainsKey(path))
            {
                return _cachedAssets[path] as T;
            }
            
            var config = Resources.Load<T>(path);
            
            if (config == null)
            {
                Debug.LogError($"Path not found: {path}");
                return null;
            }
            
            if (withUnload)
            {
                Resources.UnloadAsset(config);
            }
            else
            {
                _cachedAssets.Add(path, config);
            }

            return config;
        }

        public void Dispose()
        {
        }
    }
}
