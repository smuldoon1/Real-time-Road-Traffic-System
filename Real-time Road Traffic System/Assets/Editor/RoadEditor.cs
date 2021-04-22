using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
        Event e = Event.current;

        // When this road is selected in the editor, holding shift allows the actual road gameObject to be selected
        if (Selection.activeGameObject == road.gameObject && !e.shift)
        {
            Selection.activeGameObject = road.transform.parent.gameObject;
            road.transform.parent.GetComponent<RoadNetwork>().ActiveRoad = road;
        }
    }
}
