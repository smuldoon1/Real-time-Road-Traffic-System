using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public RoadNetwork roadNetwork;
    public float speed;

    public int currentPoint;

    RoadPoint[] points;
    public float time;

    public int lane;

    private void Start()
    {
        points = roadNetwork.road.lane0;
        StartCoroutine(Drive());
    }

    private IEnumerator Drive()
    {
        while (true)
        {
            if (lane == 0)
                points = roadNetwork.road.lane0;
            else
                points = roadNetwork.road.lane1;
            transform.position = LerpedPosition(currentPoint, lane == 0 ? 1 : -1);
            transform.forward = LerpedForward(currentPoint, lane == 0 ? 1 : -1);
            time += Time.deltaTime * Mathf.Abs(speed);
            if (currentPoint >= points.Length)
                currentPoint -= points.Length;
            if (currentPoint < 0)
                currentPoint += points.Length;
            if (time >= 1)
            {
                currentPoint += Mathf.FloorToInt(time) * lane == 0 ? 1 : -1;
                time %= 1;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    Vector3 LerpedPosition(int i, int direction)
    {
        int index0 = (i + points.Length) % points.Length;
        int index1 = (i + direction + points.Length) % points.Length;
        return Vector3.Lerp(points[index0].Position, points[index1].Position, time);
    }

    Vector3 LerpedForward(int i, int direction)
    {
        int index0 = (i + points.Length) % points.Length;
        int index1 = (i + direction + points.Length) % points.Length;
        return Vector3.Lerp(points[index0].Forward, points[index1].Forward, time);
    }
}
