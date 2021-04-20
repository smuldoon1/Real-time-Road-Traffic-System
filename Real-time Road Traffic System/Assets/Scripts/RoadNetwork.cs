using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadNetwork : MonoBehaviour
{
    [HideInInspector]
    public List<Road> roads;

    public Road selectedRoad;

    public delegate void RoadUpdateEvent();
    public event RoadUpdateEvent OnRoadChanged;

    private void OnEnable()
    {
        roads = new List<Road>();
    }

    public void CreateRoadNetwork()
    {
        foreach (Road road in roads)
        {
            try
            {
                DestroyImmediate(road.gameObject);
            }
            catch
            {
                roads = new List<Road>();
            }
        }
        roads = new List<Road>();
        CreateNewRoad();
    }

    public Road CreateNewRoad()
    {
        Road newRoad = new GameObject("Road (" + roads.Count + ")").AddComponent<Road>();
        newRoad.InitialiseRoad(transform.position);
        newRoad.transform.parent = transform;
        roads.Add(newRoad);
        return newRoad;
    }

    public Road ActiveRoad
    {
        get
        {
            return selectedRoad;
        }
        set
        {
            foreach (Road road in roads)
                if (road == value)
                    selectedRoad = road;
        }
    }
}
