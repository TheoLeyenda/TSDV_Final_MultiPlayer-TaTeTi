using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateThis : MonoBehaviour
{
    public enum AxisRotate
    {
        X,
        Y,
        Z,
    }
    [SerializeField] private float speedRotation = 10;
    [SerializeField] private AxisRotate axisRotate;
    // Update is called once per frame
    void Update()
    {
        switch (axisRotate)
        {
            case AxisRotate.X:
                transform.Rotate(transform.right, speedRotation * Time.deltaTime);
                break;
            case AxisRotate.Y:
                transform.Rotate(transform.up, speedRotation * Time.deltaTime);
                break;
            case AxisRotate.Z:
                transform.Rotate(transform.forward, speedRotation * Time.deltaTime);
                break;
        }
    }
}
