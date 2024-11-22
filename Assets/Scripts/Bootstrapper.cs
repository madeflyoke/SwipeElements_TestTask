using Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class Bootstrapper : MonoBehaviour
{
    [Inject] private ServicesHolder _servicesHolder;
        
    private async void Start()
    {
        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = false;
        await _servicesHolder.InitializeServices();

        SceneManager.LoadSceneAsync(1);
    }
        
    private void OnDestroy()
    {
        _servicesHolder?.Dispose();
    }
}