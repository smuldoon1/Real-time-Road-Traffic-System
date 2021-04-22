using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrafficLight : MonoBehaviour
{
    public bool[] modeSetting; // Array that stores the value of the traffic light in each mode, false = red light; true = green light

    Collider blockingCollider; // Collider used to block vehicles when there is a red light
    TrafficLightBulbs bulbs; // Component used for changing the emission of each light bulb on or off

    bool currentMode; // The traffic lights current mode

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
        if (modeSetting[mode] && !currentMode)
        {
            currentMode = true;
            bulbs.SetLights(true, true, false);
            yield return new WaitForSeconds(transitionTime);
            bulbs.SetLights(false, false, true);
            blockingCollider.enabled = false;
        }
        else if (!modeSetting[mode] && currentMode)
        {
            currentMode = false;
            bulbs.SetLights(false, true, false);
            blockingCollider.enabled = true;
            yield return new WaitForSeconds(transitionTime);
            bulbs.SetLights(true, false, false);
        }
    }
}
