using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0, 1, -5);

    void Update ()
    {
        transform.position = _target.transform.position + _offset;
    }
}
