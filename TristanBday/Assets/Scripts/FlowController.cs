using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FlowController : MonoBehaviour
{
    private const string CAPTION_END = "Happy birthday <3 uwu";
    
    [SerializeField] private PlayerController _playerController;

    [Header("End Game")]
    [SerializeField] private GameObject _linesRoot;
    [SerializeField] private CameraFollow _cameraFollow;
    [SerializeField] private Transform _endGameCameraPoint;
    [SerializeField] private float _cameraZoomOutLerpTime = 4f;

    [Header("Skip to End")]
    [SerializeField] private Limb _limb;
    [SerializeField] private Collider _lastHold;
    
    [Header("UI")]
    [SerializeField] private Toggle _menuToggle;
    [SerializeField] private Toggle _helpToggle;

    private Action<bool> _lastHoldReachedHandle;
    private Action<Collider> _checkpointReachedHandle;
    private Collider _lastCheckpointCollider;

    private void Start()
    {
        EventBus.Register(EventHooks.LastHoldReached, _lastHoldReachedHandle = OnLastHoldReached);
        EventBus.Register(EventHooks.CheckpointHoldReached, _checkpointReachedHandle = OnCheckpointReached);
    }

    private void OnDestroy()
    {
        EventBus.Unregister(EventHooks.LastHoldReached, _lastHoldReachedHandle);
        EventBus.Unregister(EventHooks.CheckpointHoldReached, _checkpointReachedHandle);
    }

    public void SkipToEnd()
    {
        _playerController.DetachAllHolds();
        _playerController.transform.position = _lastHold.transform.position;
        _limb.AttachToHold(_lastHold);
    }

    public void GoToLastCheckpoint()
    {
        if (_lastCheckpointCollider != null)
        {
            _playerController.DetachAllHolds();
            _playerController.transform.position = _lastCheckpointCollider.transform.position;
            _limb.AttachToHold(_lastCheckpointCollider);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(0); 
    }

    public void SetHelpToggle(bool isOn)
    {
        _helpToggle.isOn = isOn;
        
        // only allow one popup at a time
        if (isOn && _menuToggle.isOn)
        {
            _menuToggle.isOn = false;
        }
    }
    
    public void SetMenuToggle(bool isOn)
    {
        _menuToggle.isOn = isOn;
        
        // only allow one popup at a time
        if (isOn && _helpToggle.isOn)
        {
            _helpToggle.isOn = false;
        }
    }

    public void ToggleCamera()
    {
        bool shouldZoomOut = _cameraFollow.enabled;
        if (shouldZoomOut)
        {
            StartCoroutine(LerpOut());
        }
        else
        {
            _cameraFollow.enabled = true;
        }
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        Debug.Log($"[{this.GetType().ToString()}] TOGGLE FULLSCREEN: {Screen.fullScreen}");
    }

    private void OnCheckpointReached(Collider checkpointCollider)
    {
        // Only save checkpoint collider if it is higher than the current one
        if (_lastCheckpointCollider != null
            && _lastCheckpointCollider.transform.position.y > checkpointCollider.transform.position.y)
        {
            return;
        }
        _lastCheckpointCollider = checkpointCollider;   
    }

    private void OnLastHoldReached(bool _)
    {
        _linesRoot.SetActive(true);
        StartCoroutine(EndGame());
    }

    IEnumerator EndGame()
    {
        yield return StartCoroutine(LerpOut());
        EventBus.Trigger(EventHooks.ShowCaptionText, (CAPTION_END, -1f));
    }
    
    IEnumerator LerpOut()
    {
        _cameraFollow.enabled = false;
        Camera mainCamera = Camera.main;
        var originalPosition = mainCamera.transform.position;
        float elapsedTime = 0;
        while (elapsedTime < _cameraZoomOutLerpTime
               && Vector3.Distance(mainCamera.transform.position, _endGameCameraPoint.position) > 0.1f)
        {
            mainCamera.transform.position = Vector3.Lerp(originalPosition, _endGameCameraPoint.position,
                (elapsedTime/_cameraZoomOutLerpTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.position = _endGameCameraPoint.position;
    }
}
