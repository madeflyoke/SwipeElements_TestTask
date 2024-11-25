using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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

        public T LoadJsonDataAsset<T>() where T: class
        {
            var assetName = typeof(T).Name;

            var path = Constants.ResourcesPaths.DATA + assetName;
            if (_cachedAssets.ContainsKey(path))
            {
                return _cachedAssets[path] as T;
            }
            
            var json = Resources.Load<TextAsset>(path);
        
            if (json == null)
            {
                Debug.LogError($"Path not found: {assetName}");
                return null;
            }
            
            T jsonData = JsonConvert.DeserializeObject<T>(json.text);
            Resources.UnloadAsset(json);
            
            if (jsonData == null)
            {
                Debug.LogError($"Json cant be serialized: {assetName}");
                return null;
            }
            
            _cachedAssets.Add(path, jsonData);
            return jsonData;
        }

        public void Dispose()
        {
        }
    }
}
