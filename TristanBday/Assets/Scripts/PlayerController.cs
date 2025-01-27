using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Limb[] _limbs;
    [SerializeField] private float _noHoldDragLimit = 2f;
    [SerializeField] private float _floorHeightDragThreshold = 0.6f;
    private bool _limbsCanDrag = true;
    private bool _dragCountingDown = false;

    private void Reset()
    {
        _limbs = GetComponentsInChildren<Limb>();
    }

    private void Update()
    {
        if (GetHoldCount() > 0 && !_limbsCanDrag)
        {
            SetDrag(true);
        }
        else if (IsOnTheFloor() && !_limbsCanDrag)
        {
            SetDrag(true);
        }
        else if (!IsOnTheFloor() && GetHoldCount() == 0 && _limbsCanDrag && !_dragCountingDown)
        {
            _dragCountingDown = true;
            StartCoroutine(WaitToStopDrag());
        }
    }

    public void DetachAllHolds()
    {
        foreach (var limb in _limbs)
        {
            limb.DetachFromHold();
        }
    }

    private void SetDrag(bool canDrag)
    {
        foreach (var limb in _limbs)
        {
            limb.CanDrag = canDrag;
        }

        _limbsCanDrag = canDrag;
    }

    private int GetHoldCount()
    {
        int holdCount = 0;
        foreach (var limb in _limbs)
        {
            holdCount += limb.IsHolding ? 1 : 0;
        }

        return holdCount;
    }

    private IEnumerator WaitToStopDrag()
    {
        yield return new WaitForSeconds(_noHoldDragLimit);
        SetDrag(false);
        _dragCountingDown = false;
    }

    private bool IsOnTheFloor()
    {
        return transform.position.y < _floorHeightDragThreshold;
    }
}
