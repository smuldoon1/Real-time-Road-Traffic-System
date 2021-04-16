using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoadMesh
{
    // Create a mesh based on a path of equidistant points
    public static Mesh CreateMesh(RoadPoint[] points, float roadWidth, bool isClosed)
    {
        Vector3[] vertices = new Vector3[points.Length * 2];
        int trianglesSize = 2 * (points.Length - 1) + ((isClosed) ? 2 : 0);
        int[] triangles = new int[trianglesSize * 3];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0, vertIndex = 0, triIndex = 0; i < points.Length; i++, vertIndex += 2, triIndex += 6)
        {
            vertices[vertIndex] = points[i].Position + points[i].Right * roadWidth * 0.5f;
            vertices[vertIndex + 1] = points[i].Position - points[i].Right * roadWidth * 0.5f;

            float completion = i / (float)(points.Length - 1f);
            float v = 1 - Mathf.Abs(2 * completion - 1);
            uvs[vertIndex] = new Vector2(0, v);
            uvs[vertIndex + 1] = new Vector2(1, v);

            if (i < points.Length - 1 || isClosed)
            {
                triangles[triIndex] = vertIndex;
                triangles[triIndex + 1] = (vertIndex + 2) % vertices.Length;
                triangles[triIndex + 2] = vertIndex + 1;

                triangles[triIndex + 3] = vertIndex + 1;
                triangles[triIndex + 4] = (vertIndex + 2) % vertices.Length;
                triangles[triIndex + 5] = (vertIndex + 3) % vertices.Length;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;
    }
}
