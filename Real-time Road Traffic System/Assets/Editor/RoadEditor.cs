using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Road)), CanEditMultipleObjects]
public class RoadEditor : Editor
{
    Road road;
    Transform handleTransform;
    Quaternion handleRotation;

    private void OnSceneGUI()
    {
        road = target as Road;

        handleTransform = road.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;
        Vector3 point0 = handleTransform.TransformPoint(road.nodeA.Position);
        Vector3 point1 = handleTransform.TransformPoint(road.nodeB.Position);

        Handles.color = Color.yellow;
        Handles.DrawLine(point0, point1);

        EditorGUI.BeginChangeCheck();
        point0 = Handles.DoPositionHandle(point0, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(road, "Move Node");
            EditorUtility.SetDirty(road);
            road.nodeA.SetPosition(handleTransform.InverseTransformPoint(point0));
        }
        EditorGUI.BeginChangeCheck();
        point1 = Handles.DoPositionHandle(point1, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(road, "Move Node");
            EditorUtility.SetDirty(road);
            road.nodeB.SetPosition(handleTransform.InverseTransformPoint(point1));
        }
    }
}
