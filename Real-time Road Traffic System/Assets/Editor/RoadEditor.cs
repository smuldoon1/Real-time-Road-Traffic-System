using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Road))]
public class RoadEditor : Editor
{
    private void OnSceneGUI()
    {
        Road road = target as Road;

        Transform handleTransform = road.transform;
        Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;
        Vector3 point0 = handleTransform.TransformPoint(road.nodeA.Position);
        Vector3 point1 = handleTransform.TransformPoint(road.nodeB.Position);

        Handles.color = Color.yellow;
        Handles.DrawLine(point0, point1);
        Handles.DoPositionHandle(point0, handleRotation);
        Handles.DoPositionHandle(point1, handleRotation);
    }
}
