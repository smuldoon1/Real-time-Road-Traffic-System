using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoadNetwork : MonoBehaviour
{
    [HideInInspector]
    public Road road;

    public void CreateRoad()
    {
        road = new Road(transform.position);
    }
    
    public void SetMesh(Mesh mesh, Vector2 textureScale)
    {
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = textureScale;
    }
}
