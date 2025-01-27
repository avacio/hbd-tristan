using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class HoldManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset _inputActionAsset;

    private InputAction _inputAction;
    
    private void Start()
    {
        _inputAction = _inputActionAsset.actionMaps[1]["Click"];
        _inputAction.performed += OnClickUp;
    }

    private void OnDestroy()
    {
        _inputAction.performed -= OnClickUp;
    }

    void OnClickUp(InputAction.CallbackContext context)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.CompareTag("Hold") &&
                hit.transform.GetComponent<Joint>() is Joint holdJoint)
            {
                Debug.Log($"Release hold on {holdJoint.name}");
                EventBus.Trigger(EventHooks.HoldReleased, holdJoint);
            }
        }
    }
}
