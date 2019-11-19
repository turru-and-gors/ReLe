using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MovingCamera : MonoBehaviour
{
    Vector3 initialPosition;
    
    void Start()
    {
        initialPosition = transform.position;
    }

    public void MoveCameraUp(float stepSize)
    {
        Vector3 camPos = transform.position;
        camPos.y += stepSize;
        transform.position = camPos;
    }

    public void ResetCamera()
    {
        transform.position = initialPosition;
    }
}
