using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateComponent : MonoBehaviour
{
    private enum RotationAxis
    {
        X,
        Y,
        Z,
    }
    [SerializeField] private float rotationSpeed = 5;
    [SerializeField] private RotationAxis axisRotation = RotationAxis.Z;
    private Vector3 axis;
    void Start()
    {
        switch (axisRotation)
        {
            case RotationAxis.X:
                axis = transform.right;
                break;
            case RotationAxis.Y:
                axis = transform.up;
                break;
            case RotationAxis.Z:
                axis = transform.forward;
                break;
        }
    }
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(axis, rotationSpeed);
    }
}
