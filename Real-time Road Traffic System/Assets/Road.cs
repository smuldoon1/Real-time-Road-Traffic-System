using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : MonoBehaviour
{
    public Node nodeA;
    public Node nodeB;

    public Vector3[] curvePoints;

    public void UpdatePoints()
    {
        if (nodeA == null || nodeB == null)
            return;
        nodeA.SetPosition(curvePoints[0]);
        nodeB.SetPosition(curvePoints[3]);
    }

    public Vector3 GetPoint(float t)
    {
        return transform.TransformPoint(BezierCurve.GetPoint(curvePoints[0], curvePoints[1], curvePoints[2], curvePoints[3], t));
    }

    public void Reset()
    {
        if (nodeA == null || nodeB == null)
        {
            curvePoints = new Vector3[]
            {
                nodeA.Position,
                nodeA.Position + nodeB.Position * .33f,
                nodeA.Position + nodeB.Position * .67f,
                nodeB.Position
            };
        }
    }
}
