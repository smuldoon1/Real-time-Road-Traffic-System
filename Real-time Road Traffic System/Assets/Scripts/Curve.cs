using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Curve
{
    // Generates a quadratic curve
    public static Vector3 QuadraticCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        Vector3 line1 = Vector3.Lerp(p0, p1, t);
        Vector3 line2 = Vector3.Lerp(p1, p2, t);

        return Vector3.Lerp(line1, line2, t);
    }

    // Generates a bezier curve
    public static Vector3 CubicCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        Vector3 curve1 = QuadraticCurve(p0, p1, p2, t);
        Vector3 curve2 = QuadraticCurve(p1, p2, p3, t);

        return Vector3.Lerp(curve1, curve2, t);
    }
}
