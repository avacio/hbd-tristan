using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
// public class Limb : MonoBehaviour
public class Limb : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private const string LAST_HOLD_SUFFIX = "Last";
    
    [Header("Drag Control")]
    public float maxDistance = 100.0f; 
    // [SerializeField] private Plane _raycastPlane;
    
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
    private bool _isDragging;

    // /// <summary>
    // /// If the limb is holding onto something
    // /// </summary>
    public bool IsHolding => _attachedHold != null;
    
    private Action<Joint> _holdReleasedHandle;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        EventBus.Register(EventHooks.HoldReleased, _holdReleasedHandle = OnHoldReleased);
    }

    private void OnDestroy()
    {
        EventBus.Unregister(EventHooks.HoldReleased, _holdReleasedHandle);
    }
    
    public void OnPointerDown(PointerEventData eventData) {
        _isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData) {
        _isDragging = false;
    }
    
    void Update() 
    { 
        // if (!_isDragging) 
        // if (!Input.GetMouseButtonDown(0)) 

        // tODO go for event bus
        if (_attachedHold && _attachedHold.connectedBody == null)
        {
            _attachedHold = null;
        }
        
        if (!Input.GetMouseButtonDown(0) || !CanDrag) 
            return; 
        
        Camera mainCamera = FindCamera(); 
        
        // TODO support touch on mobile
        RaycastHit hit; 
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out hit, maxDistance)) 
        // if(!Physics.Raycast(ray, out hit, maxDistance, layerMask: LayerMask.NameToLayer("Wall"))) 
        //     return; 
        if(!hit.rigidbody || hit.rigidbody.isKinematic) 
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
        
        while (Input.GetMouseButton(0) && CanDrag) 
        { 
            Ray ray = cam.ScreenPointToRay(Input.mousePosition); 
            springJoint.transform.position = ray.GetPoint(distance); 
            yield return null; 
        } 
        
        if (springJoint.connectedBody) 
        { 
            springJoint.connectedBody.drag             = oldDrag; 
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

    // private void OnTriggerExit(Collider other)
    // {
    //     Debug.Log($"[{this.GetType().ToString()}] [{name}] trigger exit: {other.name}");
    //     // DetachFromHold();
    // }

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
            Debug.Log($"{name} detach from {_attachedHold.name}");

            _attachedHold.connectedBody = null;
            _attachedHold = null;
        }
    }
}
