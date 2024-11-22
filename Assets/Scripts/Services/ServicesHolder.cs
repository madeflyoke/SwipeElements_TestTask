using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services.AssetsService;
using Services.Interfaces;
using UnityEngine;
using Zenject;

namespace Services
{
    public class ServicesHolder : IDisposable
    {
        private Dictionary<Type,IService> _services;
        private CancellationTokenSource _cts;
        private bool _isInitialized;
        private readonly DiContainer _container;

        public ServicesHolder(DiContainer container)
        {
            _container = container;
        }

        public async UniTask InitializeServices(Action onInitialized = null)
        {
            if (_isInitialized)
            {
                return;
            }
            
            TService AddService<TService>() where TService: IService
            {
                var instance = _container.Instantiate<TService>();
                _services.Add(typeof(TService), instance);
                return instance;
            }
            
            _services = new Dictionary<Type, IService>();

            #region Services

            AddService<AssetsProviderService>();

            #endregion
            
            _cts = new CancellationTokenSource();

            foreach (var service in _services)
            {
#if UNITY_EDITOR
                Debug.Log($"Service {service.Value} started initialization...");
#endif
                await service.Value.Initialize(_cts);
#if UNITY_EDITOR
                Debug.Log($"Service {service.Value} initialized");
#endif
            }
            
            onInitialized?.Invoke();
            _isInitialized = true;
        }
        
        public TService GetService<TService>() where TService: IService
        {
            return (TService) _services[typeof(TService)];
        }

        public void Dispose()
        {
            _cts?.Cancel();
        }
    }
}
