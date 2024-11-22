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
        public UniTask Initialize(CancellationTokenSource cts)
        {
            return UniTask.CompletedTask;
        }

        public T LoadJsonDataAsset<T>() where T: class
        {
            var assetName = typeof(T).Name;

            var path = Constants.ResourcesPaths.DATA + assetName;
            var json = Resources.Load<TextAsset>(path);
        
            if (json == null)
            {
                Debug.LogError($"Path not found: {assetName}");
            }
            
            T levelData = JsonConvert.DeserializeObject<T>(json.text);
            
            if (levelData == null)
            {
                Debug.LogError($"Json cant be serialized: {assetName}");
            }
        
            return levelData;
        }

        public void Dispose()
        {
        }
    }
}
