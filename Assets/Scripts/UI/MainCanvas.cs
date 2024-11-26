using System;
using Signals;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class MainCanvas : MonoBehaviour //temporary canvas
    {
        [Inject] private SignalBus _signalBus;
        
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _restartLevelButton;

        private void OnEnable()
        {
           _nextLevelButton.onClick.AddListener(()=>_signalBus.Fire(new CallOnNextLevelSignal()));
           _restartLevelButton.onClick.AddListener(()=>_signalBus.Fire(new CallOnRestartLevelSignal()));
        }

        private void OnDisable()
        {
            _nextLevelButton.onClick.RemoveAllListeners();
            _restartLevelButton.onClick.RemoveAllListeners();
        }
    }
}
