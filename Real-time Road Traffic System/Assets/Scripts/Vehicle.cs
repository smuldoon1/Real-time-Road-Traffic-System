using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public RoadNetwork roadNetwork;
    public float topSpeed = 6f; // The maximum speed of the vehicle. It will try to stay at this speed if there are no obstacles or speed limit preventing this
    public float speedCheckDistance = 1f; // How far in front of the vehicle will be checked to test for corners or obstructions, based on the vehicles current velocity
    [Min(0)]
    public float turningSharpness = 3f; // The sharpness of the speed increase/decrease when turning
    [Range(0, 1)]
    public float minimumTurningSpeed = 0.6f; // The minimum decrease when turning. In the range of 0 - 1

    public float velocity;
    public int currentPoint;
    public float time;
    public Lane lane;

    RoadPoint[] path;
    float pointDistance;

    private void Start()
    {
        UpdateRouteData();
        StartCoroutine(Drive());
    }

    public void UpdateRouteData()
    {
        path = roadNetwork.road.lane0;
        pointDistance = roadNetwork.road.equidistantPointDistance;
    }

    private IEnumerator Drive()
    {
        while (true)
        {
            if (lane == 0)
                path = roadNetwork.road.lane0;
            else
                path = roadNetwork.road.lane1;

            transform.position = LerpedPosition(currentPoint, lane == 0 ? 1 : -1);
            transform.forward = LerpedForward(currentPoint, lane == 0 ? 1 : -1);

            velocity = CalculateSpeed();

            time += Time.deltaTime * velocity / pointDistance;
            if (time >= 1)
            {
                currentPoint += Mathf.FloorToInt(time) * (lane == Lane.LEFT ? 1 : -1);
                time %= 1;
            }
            if (currentPoint >= path.Length)
                currentPoint -= path.Length;
            else if (currentPoint < 0)
                currentPoint += path.Length;
            yield return new WaitForEndOfFrame();
        }
    }

    Vector3 LerpedPosition(int i, int direction)
    {
        int index0 = (i + path.Length) % path.Length;
        int index1 = (i + direction + path.Length) % path.Length;
        return Vector3.Lerp(path[index0].Position, path[index1].Position, time);
    }

    Vector3 LerpedForward(int i, int direction)
    {
        int index0 = (i + path.Length) % path.Length;
        int index1 = (i + direction + path.Length) % path.Length;
        return Vector3.Lerp(path[index0].Forward, path[index1].Forward, time);
    }

    float CalculateSpeed()
    {
        float totalMagnitude = 0f;
        int numberOfPoints = Mathf.FloorToInt(Mathf.Max(5f, velocity * speedCheckDistance) / roadNetwork.road.equidistantPointDistance);
        for (int i = 1; i < numberOfPoints; i++)
            totalMagnitude += (path[(path.Length + currentPoint + (lane == Lane.LEFT ? i : -i)) % path.Length].Forward - path[currentPoint].Forward).magnitude;
        float magnitude = totalMagnitude /= numberOfPoints - 1;
        float turningFactor = Mathf.Clamp(-minimumTurningSpeed * Mathf.Pow(magnitude, turningSharpness) + 1f, 1 - minimumTurningSpeed, 1f);
        return Mathf.Min(roadNetwork.road.SpeedLimit * turningFactor, topSpeed * turningFactor);
    }

    private void OnEnable()
    {
        roadNetwork.OnRoadChanged += UpdateRouteData;
    }

    private void OnDisable()
    {
        roadNetwork.OnRoadChanged -= UpdateRouteData;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(path[currentPoint].Position, path[currentPoint].Position + path[currentPoint].Forward * 3f);
            Gizmos.color = Color.red;
            int numberOfPoints = Mathf.FloorToInt(Mathf.Max(5f, velocity * speedCheckDistance) / roadNetwork.road.equidistantPointDistance);
            for (int i = 1; i < numberOfPoints; i++)
            {
                float magnitude = (path[(path.Length + currentPoint + (lane == Lane.LEFT ? i : -i)) % path.Length].Forward - path[currentPoint].Forward).magnitude;
                Gizmos.color = new Gradient()
                {
                    colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.yellow, 0.6f), new GradientColorKey(Color.red, 1f) },
                }.Evaluate(Mathf.Clamp(magnitude, 0f, 1f));
                Gizmos.DrawLine(path[(path.Length + currentPoint + (lane == Lane.LEFT ? i : -i)) % path.Length].Position,
                    path[(path.Length + currentPoint + (lane == Lane.LEFT ? i : -i)) % path.Length].Position + (path[(path.Length + currentPoint + (lane == Lane.LEFT ? i : -i)) % path.Length].Forward - path[currentPoint].Forward));
            }
        }
    }
}
