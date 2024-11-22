using System;
using Signals;
using UnityEngine;
using Zenject;

namespace Gameplay.Managers
{
    public class GameplayHandler : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        
        public void Awake()
        {
            _signalBus.Fire<GameplayStartedSignal>();
        }
    }
}
