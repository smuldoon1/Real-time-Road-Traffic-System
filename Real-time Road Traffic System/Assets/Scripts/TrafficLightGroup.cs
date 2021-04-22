using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightGroup : MonoBehaviour
{
    public TrafficLight[] trafficLights; // Array of traffic lights that are linked together
    public float[] modeDurations; // How long between each mode change, size of the array determines the number of modes
    public float transitionTime; // Time between traffic light changes

    public int currentMode; // The current mode of the traffic light group, it will also start on this mode

    float time; // The current time since the last mode toggle

    private void Start()
    {
        ToggleTrafficLights(currentMode); // Initialise the traffic light modes
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time >= modeDurations[currentMode])
        {
            time = 0;
            currentMode++;
            if (currentMode == modeDurations.Length)
                currentMode = 0;
            ToggleTrafficLights(currentMode);
        }
    }

    // Toggle the traffic lights
    void ToggleTrafficLights(int mode)
    {
        foreach (TrafficLight tl in trafficLights)
        {
            tl.UpdateMode(mode, transitionTime);
        }
    }
}
