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

    const int curveSteps = 10;

    private void OnSceneGUI()
    {
        road = target as Road;
        handleTransform = road.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        Vector3 point0 = ShowPoint(0);
        Vector3 point1 = ShowPoint(1);
        Vector3 point2 = ShowPoint(2);
        Vector3 point3 = ShowPoint(3);

        Handles.color = Color.gray;
        Handles.DrawLine(point0, point1);
        Handles.DrawLine(point2, point3);

        Handles.DrawBezier(point0, point3, point1, point2, Color.white, null, 2f);
    }

    Vector3 ShowPoint(int index)
    {
        Vector3 point = handleTransform.TransformPoint(road.curvePoints[index]);
        EditorGUI.BeginChangeCheck();
        point = Handles.DoPositionHandle(point, handleRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(road, "Move Node");
            EditorUtility.SetDirty(road);
            road.curvePoints[index] = handleTransform.InverseTransformPoint(point);
        }
        road.UpdatePoints();
        return point;
    }
}
