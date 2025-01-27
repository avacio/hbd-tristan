using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoHook : MonoBehaviour
{
    [SerializeField] VideoPlayer _videoPlayer;
    [SerializeField] private string _streamingAssetsVideoPath;

    private void Reset()
    {
        if (_videoPlayer == null)
        {
            _videoPlayer = GetComponent<VideoPlayer>();
        }
    }

    void Start()
    {
        _videoPlayer.url = Path.Combine(Application.streamingAssetsPath, _streamingAssetsVideoPath);
        _videoPlayer.Prepare();
        _videoPlayer.Play();
    }
}
