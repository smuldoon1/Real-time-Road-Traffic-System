using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadNetwork : MonoBehaviour
{
    [HideInInspector]
    public List<Road> roads; // The list of all roads used in this network

    Road activeRoad;

    // Event used by the editor to detect when a road has been selected
    public delegate void RoadSelectEvent(Road road);
    public event RoadSelectEvent OnRoadSelected;

    // Resets the road netwoek and creates a single basic road
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

    // Creates a new road using a default material
    public Road CreateNewRoad(Material defaultMaterial)
    {
        Road newRoad = new GameObject("Road (" + roads.Count + ")").AddComponent<Road>();
        newRoad.InitialiseRoad(transform.position);
        newRoad.transform.parent = transform;
        newRoad.UpdateMaterial(defaultMaterial);
        roads.Add(newRoad);
        return newRoad;
    }

    // The networks currently selected road
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
