using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrafficLight : MonoBehaviour
{
    public bool[] modeSetting; // Array that stores the value of the traffic light in each mode, false = red light; true = green light

    Collider blockingCollider; // Collider used to block vehicles when there is a red light
    TrafficLightBulbs bulbs; // Component used for changing the emission of each light bulb on or off

    int currentMode; // The traffic lights current mode (0 = red, 1 = red & amber, 2 = green, 3 = amber

    public int GetMode
    {
        get
        {
            return currentMode;
        }
    }

    private void Awake()
    {
        blockingCollider = GetComponent<Collider>(); // Get the traffic light collider
        if (GetComponentInChildren<TrafficLightBulbs>() != null)
            bulbs = GetComponentInChildren<TrafficLightBulbs>(); // Get the traffic light bulbs component
    }

    // Toggles the traffic light on or off with a given mode
    public void UpdateMode(int mode, float transitionTime)
    {
        StartCoroutine(SetMode(mode, transitionTime));
    }

    // Sets the collider and light bulb values for the traffic lights, only if there is a change in mode
    // Sets the light bulb emission according to the UK traffic light sequence
    IEnumerator SetMode(int mode, float transitionTime)
    {
        if (modeSetting[mode] && currentMode == 0)
        {
            currentMode = 1;
            if (bulbs != null) bulbs.SetLights(true, true, false);
            yield return new WaitForSeconds(transitionTime);
            currentMode = 2;
            if (bulbs != null) bulbs.SetLights(false, false, true);
            blockingCollider.enabled = false;
        }
        else if (!modeSetting[mode] && currentMode == 2)
        {
            currentMode = 3;
            if (bulbs != null) bulbs.SetLights(false, true, false);
            blockingCollider.enabled = true;
            yield return new WaitForSeconds(transitionTime);
            currentMode = 0;
            if (bulbs != null) bulbs.SetLights(true, false, false);
        }
    }
}
