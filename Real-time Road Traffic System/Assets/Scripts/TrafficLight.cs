using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrafficLight : MonoBehaviour
{
    public bool[] modeSetting; // Array that stores the value of the traffic light in each mode, false = red light; true = green light

    Collider blockingCollider; // Collider used to block vehicles when there is a red light
    TrafficLightBulbs bulbs; // Component used for changing the emission of each light bulb on or off

    bool currentMode;

    private void Start()
    {
        blockingCollider = GetComponent<Collider>(); // Get the traffic light collider
        if (GetComponentInChildren<TrafficLightBulbs>() != null)
            bulbs = GetComponentInChildren<TrafficLightBulbs>(); // Get the traffic light bulbs component
    }

    // Toggles the traffic light on or off with a given mode
    public void SetMode(int mode)
    {
        if (modeSetting[mode] == true) // Green light
        {
            blockingCollider.enabled = false;
            if (bulbs != null)
                bulbs.SetLights(false, false, true);
        }
        else // Red light
        {
            blockingCollider.enabled = true;
            if (bulbs != null)
                bulbs.SetLights(true, false, false);
        }
    }
}
