using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.TouchPhase;

[RequireComponent(typeof(Collider))]
public class Limb : MonoBehaviour
{
    private const string LAST_HOLD_SUFFIX = "Last";
    private const string CHECKPOINT_HOLD_SUBSTRING = "Checkpoint";
    
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
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        EventBus.Register(EventHooks.HoldReleased, _holdReleasedHandle = OnHoldReleased);
    }

    private void OnDestroy()
    {
        EventBus.Unregister(EventHooks.HoldReleased, _holdReleasedHandle);
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
        // Debug.Log($"[{this.GetType().ToString()}] [{name}] trigger enter: {other.name}");
        AttachToHold(other);

        if (other.name.EndsWith(LAST_HOLD_SUFFIX))
        {
            // Debug.Log($"[{this.GetType().ToString()}] Last hold reached");
            EventBus.Trigger(EventHooks.LastHoldReached, true);
        }
    }

    public void AttachToHold(Collider other)
    {
        if (other.GetComponent<Joint>() is Joint holdJoint)
        {
            holdJoint.connectedBody = _rb;
            _attachedHold = holdJoint;
            
            if (other.name.Contains(CHECKPOINT_HOLD_SUBSTRING))
            {
                EventBus.Trigger(EventHooks.CheckpointHoldReached, other);
            }
        }
    }

    private void OnHoldReleased(Joint joint)
    {
        if (joint == _attachedHold)
        {
            DetachFromHold();
        }
    }

    public void DetachFromHold()
    {
        if (_attachedHold != null)
        {
            _attachedHold.connectedBody = null;
            _attachedHold = null;
        }
    }
}
