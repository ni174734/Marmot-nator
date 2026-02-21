using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    void FixedUpdate() 
    {
        if (target != null) 
        {
            //transform.position = new Vector3(target.position.x, target.position.y, -10);
            Vector3 desiredPosition = target.position + offset;
			float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime);
            Vector3 smoothPosition = Vector3.Lerp(transform.position, desiredPosition, t);
            transform.position = smoothPosition;
        }
    }
}
