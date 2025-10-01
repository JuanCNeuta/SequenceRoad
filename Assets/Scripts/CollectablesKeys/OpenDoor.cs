using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


public class OpenDoor : MonoBehaviour
{
    public float speed;
    public float angle;
    public Vector3 direction;

    public bool openTrigger = false;

    void Start()
    {
        angle = transform.eulerAngles.y;
    }

    void Update()
    {
        if (Mathf.Round(transform.eulerAngles.y) != angle)
        {
            transform.Rotate(direction * speed);
        }

    }

    public void Open(float targetAngle)
    {
        angle = targetAngle;
        direction = Vector3.up;
        openTrigger = true;
    }
}
