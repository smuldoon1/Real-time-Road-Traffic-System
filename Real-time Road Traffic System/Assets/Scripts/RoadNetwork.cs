using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadNetwork : MonoBehaviour
{
    [HideInInspector]
    public List<Road> roads;

    Road activeRoad;

    public delegate void RoadUpdateEvent();
    public event RoadUpdateEvent OnRoadChanged;

    public delegate void RoadSelectEvent(Road road);
    public event RoadSelectEvent OnRoadSelected;

    public void CreateRoadNetwork(Material defaultMaterial)
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
        CreateNewRoad(defaultMaterial);
    }

    public Road CreateNewRoad(Material defaultMaterial)
    {
        Road newRoad = new GameObject("Road (" + roads.Count + ")").AddComponent<Road>();
        newRoad.InitialiseRoad(transform.position);
        newRoad.transform.parent = transform;
        newRoad.UpdateMaterial(defaultMaterial);
        roads.Add(newRoad);
        return newRoad;
    }

    public Road ActiveRoad
    {
        get
        {
            return activeRoad;
        }
        set
        {
            foreach (Road road in roads)
                if (road == value)
                {
                    activeRoad = value;
                    OnRoadSelected?.Invoke(road);
                }
        }
    }
}
