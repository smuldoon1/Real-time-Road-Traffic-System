using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IntersectionFunctions
{
    // Determine if a ray interects a sphere, either at one or two points
    public static bool CheckLineIntersectsSphere(Ray line, Vector3 sphereCentre, float sphereRadius)
    {
        Vector3 p1 = line.origin;
        Vector3 p2 = line.origin + line.direction;
        Vector3 p3 = sphereCentre;

        float a = Mathf.Pow(p2.x - p1.x, 2) + Mathf.Pow(p2.y - p1.y, 2) + Mathf.Pow(p2.z - p1.z, 2);
        float b = 2 * ((p2.x - p1.x) * (p1.x - p3.x) + (p2.y - p1.y) * (p1.y - p3.y) + (p2.z - p1.z) * (p1.z - p3.z));
        float c = Mathf.Pow(p3.x, 2) + Mathf.Pow(p3.y, 2) + Mathf.Pow(p3.z, 2) +
            Mathf.Pow(p1.x, 2) + Mathf.Pow(p1.y, 2) + Mathf.Pow(p1.z, 2) -
            2 * (p3.x * p1.x + p3.y * p1.y + p3.z * p1.z) - Mathf.Pow(sphereRadius, 2);

        return b * b - 4 * a * c >= 0;
    }
}
