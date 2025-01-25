using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // todo support touch
        
        // Release hold if user clicks on it
        if(Input.GetMouseButtonUp(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.gameObject.CompareTag("Hold") &&
                    hit.transform.GetComponent<Joint>() is Joint holdJoint)
                {
                    holdJoint.connectedBody = null;
                }
            }
        }
    }
    
}
