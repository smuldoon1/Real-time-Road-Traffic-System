using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleCollisionListener : MonoBehaviour
{
    // Listens for vehicle collisions and logs when there is a collision or a red light is ran
    void OnVehicleCollision(Vehicle vehicle, Collision collision)
    {
        TrafficLight tl = collision.gameObject.GetComponent<TrafficLight>();
        if (tl != null)
        {
            if (tl.GetMode == 0 || tl.GetMode == 1)
                Debug.Log("Red light ran! " + vehicle.name + " ran a red light.");
        }
        else
            Debug.Log("Vehicle collision! " + vehicle.name + " collided with " + collision.gameObject.name);
        //Debug.Break(); Use this to pause the editor upon a collision occuring, useful for determining where and how a collision has happened
    }

    private void OnEnable()
    {
        Vehicle.OnAnyVehicleCollision += OnVehicleCollision;
    }

    private void OnDisable()
    {
        Vehicle.OnAnyVehicleCollision -= OnVehicleCollision;
    }
}
