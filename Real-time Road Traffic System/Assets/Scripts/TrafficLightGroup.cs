using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightGroup : MonoBehaviour
{
    public TrafficLight[] trafficLights; // Array of traffic lights that are linked together
    public int modes; // How many modes the traffic light group will switch between
    public float modeDuration; // How long between each mode toggle
    public float transitionTime; // How long an amber light lasts between each traffic light switch

    public int currentMode; // The current mode of the traffic light group, it will also start on this mode

    float time; // The current time since the last mode toggle

    private void Start()
    {
        ToggleTrafficLights(currentMode); // Initialise the traffic light modes
    }

    private void Update()
    {
        time += Time.deltaTime;
        if (time >= modeDuration)
        {
            time = 0;
            currentMode++;
            if (currentMode == modes)
                currentMode = 0;
            ToggleTrafficLights(currentMode);
        }
    }

    // Toggle the traffic lights
    void ToggleTrafficLights(int mode)
    {
        foreach (TrafficLight tl in trafficLights)
        {
            tl.SetMode(mode);
        }
    }
}
