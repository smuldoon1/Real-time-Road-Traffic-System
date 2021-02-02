using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{
    public Node nodeA;
    public Node nodeB;

    public Vector3[] curvePoints;

    public void Reset()
    {
        curvePoints = new Vector3[]
        {
            nodeA.Position,
            nodeA.Position + nodeB.Position / 2f,
            nodeB.Position
        };
    }
}
