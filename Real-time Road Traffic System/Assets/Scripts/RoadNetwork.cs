using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadNetwork : MonoBehaviour
{
    [HideInInspector]
    public Road road;

    public delegate void RoadUpdateEvent();
    public event RoadUpdateEvent OnRoadChanged;

    public void CreateRoad()
    {
        road = new Road(transform.position);
    }

    private void Awake()
    {
        SetLanePoints();
    }

    public void GenerateRoad(Mesh mesh, Vector2 textureScale)
    {
        GetComponent<MeshFilter>().mesh = mesh;
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer.sharedMaterial != null)
            renderer.sharedMaterial.mainTextureScale = textureScale;

        SetLanePoints();
    }

    public void UpdateMaterial(Material material)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (material != null)
            renderer.sharedMaterial = new Material(material);
        else
            renderer.sharedMaterial = null;
    }

    public void SetLanePoints()
    {
        List<RoadPoint> lane0List = new List<RoadPoint>();
        List<RoadPoint> lane1List = new List<RoadPoint>();
        for (int i = 0; i < road.equidistantPoints.Length; i++)
        {
            RoadPoint currentPoint = road.equidistantPoints[i];
            lane0List.Add(new RoadPoint(currentPoint.Position + currentPoint.Right * road.RoadWidth * 0.25f, currentPoint.Forward));
            lane1List.Add(new RoadPoint(currentPoint.Position - currentPoint.Right * road.RoadWidth * 0.25f, -currentPoint.Forward));
        }
        road.lane0 = lane0List.ToArray();
        road.lane1 = lane1List.ToArray();

        if (OnRoadChanged != null)
            OnRoadChanged();
    }
}
