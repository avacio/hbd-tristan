using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FlowController : MonoBehaviour
{
    private const string CAPTION_END = "Happy birthday <3 uwu";
    [SerializeField] private GameObject _linesRoot;
    [SerializeField] private CameraFollow _cameraFollow;
    [SerializeField] private Transform _endGameCameraPoint;
    [SerializeField] private float _cameraZoomOutLerpTime = 4f;

    private Action<bool> _lastHoldReachedHandle;

    private void Start()
    {
        EventBus.Register(EventHooks.LastHoldReached, _lastHoldReachedHandle = OnLastHoldReached);
    }

    private void OnDestroy()
    {
        EventBus.Unregister(EventHooks.LastHoldReached, _lastHoldReachedHandle);
    }

    private void OnLastHoldReached(bool _)
    {
        Debug.Log($"[{this.GetType().ToString()}] on last hold reached");
        _linesRoot.SetActive(true);
        _cameraFollow.enabled = false;
        StartCoroutine(LerpOut());
    }
    
    IEnumerator LerpOut()
    {
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
        
        EventBus.Trigger(EventHooks.ShowCaptionText, (CAPTION_END, -1f));
    }
}
