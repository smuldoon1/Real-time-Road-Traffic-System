using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Road))]
public class RoadEditor : Editor
{
    Road road;

    private void OnEnable()
    {
        road = (Road)target;
    }

    private void OnSceneGUI()
    {
        if (Selection.activeGameObject == road.gameObject)
        {
            Selection.activeGameObject = road.transform.parent.gameObject;
            road.transform.parent.GetComponent<RoadNetwork>().ActiveRoad = road;
        }
    }
}
