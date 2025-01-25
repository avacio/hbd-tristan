using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Limb[] _limbs;

    private void Reset()
    {
        _limbs = GetComponentsInChildren<Limb>();
    }
}
