using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Vehicle : MonoBehaviour
{
    public RoadNetwork roadNetwork; // The user-set network the vehicle should use

    Road road; // The current road the vehicle is on
    public Lane lane; // The current lane the vehicle is on

    public float topSpeed = 10f; // The maximum speed of the vehicle. It will try to stay at this speed if there are no obstacles or speed limit preventing this
    public float acceleration = 2f; // How quickly the vehicle will reach its target speed. Does not affect slowing down
    public float speedCheckDistance = 10f; // How far in front of the vehicle will be checked to test for corners or obstructions, based on the vehicles current velocity
    [Min(0)]
    public float obstructionBreakingAmount = 0.2f; // How far away an obstruction can be before applying breaking
    [Min(0)]
    public float turningSharpness = 3f; // The sharpness of the speed increase/decrease when turning
    [Range(0, 1)]
    public float minimumTurningFactor = 0.4f; // The minimum decrease when turning. In the range of 0 - 1

    float velocity; // The current velocity of the vehicle
    int currentPoint; // The most recent point on the path data that the vehicle has passed
    float time; // Time value used to interpolate between current point and the next point on the road

    RoadPoint[] path; // The current set of data points the vehicle is using
    float pointDistance; // The distance between these equally spaced points

    BoxCollider vehicleCollider; // All vehicles need a collider so that other vehicles can detect them
    LayerMask collisionMask; // Collision mask used to ignore colliders in vehicle collision detection

    public bool randomiseStartingPosition = true; // Randomises the positions of each vehicle when the application is ran

    public bool showCollisionBounds = true; // Shows the collision boxes used in collision detection when Gizmos are turned on in editor mode

    // Delegates used for vehicle collision events
    public delegate void AnyVehicleCollisionEvent(Vehicle vehicle, Collision collision);
    public delegate void VehicleCollisionEvent(Collision collision);

    public static event AnyVehicleCollisionEvent OnAnyVehicleCollision; // Used to detect any vehicles being in a collision
    public event VehicleCollisionEvent OnVehicleCollision; // Used to detect a vehicle collision

    private void Awake()
    {
        if (roadNetwork == null)
        {
            roadNetwork = FindObjectOfType<RoadNetwork>();
            Debug.LogWarning("No road network was set for vehicle: " + name + ". Trying to find a road network for the vehicle to use.");
        }

        road = roadNetwork.roads[0];
        UpdateRouteData();

        vehicleCollider = GetComponent<BoxCollider>();
        collisionMask = ~LayerMask.GetMask("Vehicle Ignore");

        if (randomiseStartingPosition)
        {
            currentPoint = Random.Range(0, road.equidistantPoints.Length);
            lane = (Lane)Random.Range(0, 2);
        }
    }

    // Can be called to update the vehicles current road data
    public void UpdateRouteData()
    {
        path = road.lane0;
        pointDistance = road.equidistantPointDistance;
    }

    // Called every frame to allow the vehicle to drive
    private void Update()
    {
        if (lane == 0)
            path = road.lane0;
        else
            path = road.lane1;

        transform.position = LerpedPosition(currentPoint, lane == 0 ? 1 : -1);
        transform.forward = LerpedForward(currentPoint, lane == 0 ? 1 : -1);

        float targetVelocity = CalculateSpeed();
        velocity = Mathf.Min(velocity + (targetVelocity * Time.deltaTime * acceleration), targetVelocity);

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
    }

    // Get an interpolated position of the vehicle between its current point and the next
    Vector3 LerpedPosition(int i, int direction)
    {
        int index0 = (i + path.Length) % path.Length;
        int index1 = (i + direction + path.Length) % path.Length;
        return Vector3.Lerp(path[index0].Position, path[index1].Position, time);
    }

    // Get an interpolated forward vector of the vehicle between its current point and the next
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
        int numberOfPoints = Mathf.FloorToInt(Mathf.Max(5f, speedCheckDistance) / pointDistance);
        for (int i = Mathf.FloorToInt(vehicleCollider.size.z / pointDistance); i < numberOfPoints; i++)
        {
            RoadPoint roadPoint = path[(path.Length + currentPoint + (lane == Lane.LEFT ? i : -i)) % path.Length];

            // Get all colliders not in the ignored layer, roads could probably use this layer if needed
            Collider[] colliders = Physics.OverlapBox(roadPoint.Position + vehicleCollider.center,
                .5f * vehicleCollider.size,
                roadPoint.Rotation,
                collisionMask);     
            
            // For each collider in the immediate path of the vehicle (except the vehicle itself), check the exact distance away from it
            // nearestObstructionDistance is always the distance from the closest point of this vehicle to the closest point of the closest collider
            foreach (Collider c in colliders)
            {
                if (c.transform != transform)
                    nearestObstructionDistance = Mathf.Min(nearestObstructionDistance, 
                        Vector3.Distance(vehicleCollider.ClosestPoint(c.transform.position), c.ClosestPoint(transform.position)));
            }
        }
        // Use the obstruction distance / the checked distance to get 'x'
        // Then use the function:  y = (b+1)x - b  (where b is obstructionBreakingAmount) to get the actual breaking amount from 0 - 1
        float obstructionFactor = Mathf.Clamp((obstructionBreakingAmount + 1) * Mathf.Pow(nearestObstructionDistance / speedCheckDistance, 1) - obstructionBreakingAmount, 0f, 1f);

        // Turning speed calculation
        float cornerMagnitude = 0f;
        numberOfPoints = Mathf.FloorToInt(Mathf.Max(5f, velocity / topSpeed * speedCheckDistance / pointDistance));
        for (int i = 0; i < numberOfPoints; i++)
            cornerMagnitude += (path[(path.Length + currentPoint + (lane == Lane.LEFT ? i : -i)) % path.Length].Forward - path[currentPoint].Forward).magnitude;
        float turningFactor = Mathf.Clamp(-(1 - minimumTurningFactor) * Mathf.Pow(cornerMagnitude /= numberOfPoints, turningSharpness) + 1f, minimumTurningFactor, 1f);

        // Return the final calculated speed
        return Mathf.Min(road.SpeedLimit, topSpeed * turningFactor * obstructionFactor);
    }

    // When the vehicle collides with something else, the vehicle collision events should be invoked
    // To properly register a collision, either the vehicle or the collided object must have a RigidBody component
    private void OnCollisionEnter(Collision collision)
    {
        if (collisionMask == (collisionMask | (1 << collision.gameObject.layer)))
        {
            OnAnyVehicleCollision?.Invoke(this, collision);
            OnVehicleCollision?.Invoke(collision);
        }
    }

    // Shows the developer how the vehicles slow down at corners and obstructions
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && road != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(path[currentPoint].Position, path[currentPoint].Position + path[currentPoint].Forward * 3f);
            Gizmos.color = Color.red;
            int numberOfPoints = Mathf.FloorToInt(Mathf.Max(5f, velocity / topSpeed * speedCheckDistance / pointDistance));
            for (int i = 0; i < numberOfPoints; i++)
            {
                RoadPoint roadPoint = path[(path.Length + currentPoint + (lane == Lane.LEFT ? i : -i)) % path.Length];
                // Shows the collision boxes for each point the vehicle is checking
                if (showCollisionBounds)
                {
                    Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
                    Gizmos.matrix = Matrix4x4.TRS(roadPoint.Position + vehicleCollider.center, roadPoint.Rotation, vehicleCollider.size);
                    Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                    Gizmos.matrix = Matrix4x4.identity;
                }
                float magnitude = (roadPoint.Forward - path[currentPoint].Forward).magnitude;
                Gizmos.color = new Gradient()
                {
                    colorKeys = new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.yellow, 0.6f), new GradientColorKey(Color.red, 1f) },
                }.Evaluate(Mathf.Clamp(magnitude, 0f, 1f));
                Gizmos.DrawLine(roadPoint.Position,
                    roadPoint.Position + (roadPoint.Forward - path[currentPoint].Forward));
            }
        }
    }
}
