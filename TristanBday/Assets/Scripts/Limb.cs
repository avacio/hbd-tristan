using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TouchPhase = UnityEditor.DeviceSimulation.TouchPhase;

[RequireComponent(typeof(Collider))]
public class Limb : MonoBehaviour
// public class Limb : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private const string LAST_HOLD_SUFFIX = "Last";
    
    [SerializeField] private InputActionAsset _inputActionAsset;
    
    [Header("Drag Control")]
    public float maxDistance = 100.0f; 
    
    public float spring = 50.0f; 
    public float damper = 5.0f; 
    public float drag = 10.0f; 
    public float angularDrag = 5.0f; 
    public float distance = 0.2f; 
    public bool attachToCenterOfMass = false;

    public bool CanDrag = true;
    
    private SpringJoint springJoint; 
    private Rigidbody _rb;
    private Joint _attachedHold;

    // /// <summary>
    // /// If the limb is holding onto something
    // /// </summary>
    public bool IsHolding => _attachedHold != null;
    
    private Action<Joint> _holdReleasedHandle;
    private InputAction _inputAction;
    // private bool _isInputHeldDown = false;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        EventBus.Register(EventHooks.HoldReleased, _holdReleasedHandle = OnHoldReleased);
        _inputAction = _inputActionAsset.actionMaps[1]["Click"];
        // _inputAction.started += OnInputStarted;
        // _inputAction.performed += OnInputPerformed;
    }

    private void OnDestroy()
    {
        EventBus.Unregister(EventHooks.HoldReleased, _holdReleasedHandle);
        // _inputAction.started -= OnInputStarted;
        // _inputAction.performed -= OnInputPerformed;
    }
    
    // public void OnPointerDown(PointerEventData eventData)
    // {
    //     _isInputHeldDown = true;
    //     Debug.Log($"[{this.GetType().ToString()}] STARTED input held down: {_isInputHeldDown}");
    // }
    //
    // public void OnPointerUp(PointerEventData eventData)
    // {
    //     _isInputHeldDown = false;
    //     Debug.Log($"[{this.GetType().ToString()}] PERFORMED input held down: {_isInputHeldDown}");
    // }
    //
    // private void OnInputStarted(InputAction.CallbackContext context)
    // {
    //     _isInputHeldDown = true;
    //     Debug.Log($"[{this.GetType().ToString()}] STARTED input held down: {_isInputHeldDown}");
    // }
    //
    // private void OnInputPerformed(InputAction.CallbackContext context)
    // {
    //     _isInputHeldDown = false;
    //     Debug.Log($"[{this.GetType().ToString()}] PERFORMED input held down: {_isInputHeldDown}");
    // }

    private bool IsInputHeldDown()
    {
        return Input.GetMouseButtonDown(0) || Input.touchCount > 0;
    }
    
    void Update() 
    { 
        if (!CanDrag || (!Input.GetMouseButtonDown(0) && Input.touchCount == 0)) return;
        // If the touch did not start this frame then return
        if (Input.touchCount > 0 && Input.GetTouch(0) is Touch firstTouch &&
            (int) firstTouch.phase > (int) TouchPhase.Began) return;
        
        Camera mainCamera = FindCamera(); 
        
        RaycastHit hit; 
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out hit, maxDistance)) return;
        if (!hit.rigidbody || hit.rigidbody.isKinematic) 
            return;
        
        //Initialise the enter variable
        float enter = 0.0f;

        if (!springJoint) 
        { 
            GameObject go = new GameObject("Rigidbody dragger"); 
            Rigidbody body = go.AddComponent<Rigidbody>(); 
            body.isKinematic = true; 
            springJoint = go.AddComponent<SpringJoint>(); 
        }

        springJoint.transform.position = hit.point; 
        if (attachToCenterOfMass) 
        { 
            Vector3 anchor = transform.TransformDirection(hit.rigidbody.centerOfMass) + hit.rigidbody.transform.position; 
            anchor = springJoint.transform.InverseTransformPoint(anchor); 
            springJoint.anchor = anchor; 
        } 
        else 
        { 
            springJoint.anchor = Vector3.zero; 
        } 
        
        springJoint.spring = spring; 
        springJoint.damper = damper; 
        springJoint.maxDistance = distance; 
        springJoint.connectedBody = hit.rigidbody; 
        
        StartCoroutine(DragObject(hit.distance)); 
    } 
    
    IEnumerator DragObject(float distance)
    {
        if (springJoint && !springJoint.connectedBody) yield break;

        float oldDrag            = springJoint.connectedBody.drag; 
        float oldAngularDrag     = springJoint.connectedBody.angularDrag; 
        springJoint.connectedBody.drag             = this.drag; 
        springJoint.connectedBody.angularDrag     = this.angularDrag; 
        Camera cam = FindCamera(); 
        
        while ((Input.GetMouseButton(0) || Input.touchCount > 0) && CanDrag) 
        { 
            Ray ray = cam.ScreenPointToRay(Input.mousePosition); 
            springJoint.transform.position = ray.GetPoint(distance); 
            yield return null; 
        } 
        
        if (springJoint.connectedBody) 
        { 
            springJoint.connectedBody.drag            = oldDrag; 
            springJoint.connectedBody.angularDrag     = oldAngularDrag; 
            springJoint.connectedBody                 = null; 
        } 
    } 
    
    Camera FindCamera() 
    { 
        if (GetComponent<Camera>()) 
            return GetComponent<Camera>(); 
        else 
            return Camera.main; 
    } 
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[{this.GetType().ToString()}] [{name}] trigger enter: {other.name}");
        AttachToHold(other);

        if (other.name.EndsWith(LAST_HOLD_SUFFIX))
        {
            Debug.Log($"[{this.GetType().ToString()}] lasthold reached");
            EventBus.Trigger(EventHooks.LastHoldReached, true);
        }
    }

    private void AttachToHold(Collider other)
    {
        if (other.GetComponent<Joint>() is Joint holdJoint)
        {
            holdJoint.connectedBody = _rb;
            _attachedHold = holdJoint;
        }
    }

    private void OnHoldReleased(Joint joint)
    {
        if (joint == _attachedHold)
        {
            DetachFromHold();
        }
    }

    private void DetachFromHold()
    {
        if (_attachedHold != null)
        {
            _attachedHold.connectedBody = null;
            _attachedHold = null;
        }
    }
}
