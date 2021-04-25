using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TrafficLightBulbs : MonoBehaviour
{
    private void Start()
    {
        Material[] m = GetComponent<MeshRenderer>().materials;
        Material[] lights = new Material[] { new Material( m[0]), new Material(m[1]), new Material(m[2]) }; // Create copies of each original material
        m[0] = lights[0];
        m[1] = lights[1];
        m[2] = lights[2];
    }

    // Set the light bulbs of the traffic light on or off
    public void SetLights(bool red, bool amber, bool green)
    {
        if (red)
            SetEmission(1, true, Color.red);
        else
            SetEmission(1, false, Color.black);

        if (amber)
            SetEmission(0, true, Color.yellow);
        else
            SetEmission(0, false, Color.black);

        if (green)
            SetEmission(2, true, Color.green);
        else
            SetEmission(2, false, Color.black);
    }

    // Set the emmision values of a light bulb
    void SetEmission(int lightIndex, bool isEmissive, Color colour)
    {
        Material m = GetComponent<MeshRenderer>().materials[lightIndex];
        if (isEmissive)
        {
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        else
        {
            m.DisableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
        }
        m.SetColor("_EmissiveColor", colour);
    }
}
