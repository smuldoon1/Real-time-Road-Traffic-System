using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Vehicle : MonoBehaviour
{
    public RoadNetwork roadNetwork;
    Road road;

    Collider vehicleCollider;

    public float topSpeed = 6f; // The maximum speed of the vehicle. It will try to stay at this speed if there are no obstacles or speed limit preventing this
    public float speedCheckDistance = 1f; // How far in front of the vehicle will be checked to test for corners or obstructions, based on the vehicles current velocity
    [Min(0)]
    public float obstructionBreakingAmount; // How far away an obstruction can be before applying breaking
    [Min(0)]
    public float turningSharpness = 3f; // The sharpness of the speed increase/decrease when turning
    [Range(0, 1)]
    public float minimumTurningFactor = 0.4f; // The minimum decrease when turning. In the range of 0 - 1

    public float velocity;
    public int currentPoint;
    public float time;
    public Lane lane;

    RoadPoint[] path;
    float pointDistance;

    private void Awake()
    {
        road = roadNetwork.roads[0];
        UpdateRouteData();

        vehicleCollider = GetComponent<Collider>();

        StartCoroutine(Drive());
    }

    public void UpdateRouteData()
    {
        Debug.Log("Road name" + road.name);
        path = road.lane0;
        pointDistance = road.equidistantPointDistance;
    }

    private IEnumerator Drive()
    {
        while (true)
        {
            if (lane == 0)
                path = road.lane0;
            else
                path = road.lane1;

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

    // Calculate the speed the vehicle should be travelling at; factoring in the speed limit, road topology and possible obstructions
    float CalculateSpeed()
    {
        // Slow down for objects in the vehicles immediate path
        float nearestObstructionDistance = Mathf.Infinity; 
        int numberOfPoints = Mathf.FloorToInt(Mathf.Max(5f, speedCheckDistance) / road.equidistantPointDistance);
        for (int i = 0; i < numberOfPoints; i++)
        {
            Vector3 position = path[(path.Length + currentPoint + (lane == Lane.LEFT ? i : -i)) % path.Length].Position;
            Collider[] colliders = Physics.OverlapSphere(position, road.RoadWidth * 0.225f, ~LayerMask.GetMask("Vehicle Ignore"));
            
            // For each collider in the immediate path of the vehicle (except the vehicle itself), check the exact distance away from it
            // nearestObstructionDistance is always the distance from the closest point of this vehicle to the closest point of the closest collider
            foreach (Collider c in colliders)
            {
                if (c.transform != transform)
                    nearestObstructionDistance = Mathf.Min(nearestObstructionDistance, Vector3.Distance(vehicleCollider.ClosestPoint(c.transform.position), c.ClosestPoint(transform.position)));
            }
        }
        // Use the obstruction distance / the checked distance to get 'x'
        // Then use the function:  y = (b+1)x - b  (where b is obstructionBreakingAmount) to get the actual breaking amount from 0 - 1
        float obstructionFactor = Mathf.Clamp((obstructionBreakingAmount + 1) * Mathf.Pow(nearestObstructionDistance / speedCheckDistance, 1) - obstructionBreakingAmount, 0f, 1f);


        // Turning speed calculation
        float cornerMagnitude = 0f;
        numberOfPoints = Mathf.FloorToInt(Mathf.Max(5f, velocity / topSpeed * speedCheckDistance / road.equidistantPointDistance));
        for (int i = 0; i < numberOfPoints; i++)
            cornerMagnitude += (path[(path.Length + currentPoint + (lane == Lane.LEFT ? i : -i)) % path.Length].Forward - path[currentPoint].Forward).magnitude;
        float turningFactor = Mathf.Clamp(-(1 - minimumTurningFactor) * Mathf.Pow(cornerMagnitude /= numberOfPoints, turningSharpness) + 1f, minimumTurningFactor, 1f);

        // Finally, return the final calculated speed
        return Mathf.Min(road.SpeedLimit, topSpeed * turningFactor * obstructionFactor);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && road != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(path[currentPoint].Position, path[currentPoint].Position + path[currentPoint].Forward * 3f);
            Gizmos.color = Color.red;
            int numberOfPoints = Mathf.FloorToInt(Mathf.Max(5f, velocity / topSpeed * speedCheckDistance / road.equidistantPointDistance));
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
