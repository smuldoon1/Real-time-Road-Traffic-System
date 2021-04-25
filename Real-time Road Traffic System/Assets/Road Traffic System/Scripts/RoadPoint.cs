using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RoadPoint
{
    [SerializeField]
    Vector3 position;

    [SerializeField]
    Vector3 forward;

    public RoadPoint(Vector3 position, Vector3 forward)
    {
        this.position = position;
        this.forward = forward;
    }

    public Vector3 Position
    {
        get { return position; }
    }

    public Vector3 Forward
    {
        get { return forward.normalized; }
    }

    public Vector3 Right
    {
        get { return Vector3.Cross(Forward, Vector3.up).normalized; }
    }

    public Vector3 Up
    {
        get { return Vector3.Cross(Right, Forward).normalized; }
    }

    public Quaternion Rotation
    {   
        get { return Quaternion.LookRotation(Forward, Up); }
    }
}
