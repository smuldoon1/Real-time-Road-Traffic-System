using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public float accelerationForce;
    public float brakeForce;
    public float steerForce;

    public Rigidbody[] frontWheels = new Rigidbody[2];
    public Rigidbody frontAxle;

    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Accelerate(float accelerationAmount)
    {
        //rb.AddRelativeForce(Vector3.forward * accelerationAmount);
        AddWheelTorque(frontWheels[0], accelerationAmount, 1);
        AddWheelTorque(frontWheels[1], accelerationAmount, 1);
    }

    public void Brake(float brakeAmount)
    {
        //rb.AddRelativeForce(Vector3.back * brakeAmount);
        AddWheelTorque(frontWheels[0], brakeAmount, -1);
        AddWheelTorque(frontWheels[1], brakeAmount, -1);
    }

    public void Steer(float direction)
    {
        SteerWheel(frontAxle, direction);
    }

    public void Update()
    {
        // Testing code only
        if (Input.GetKey(KeyCode.W))
        {
            Accelerate(accelerationForce);
        }
        if (Input.GetKey(KeyCode.S))
        {
            Brake(brakeForce);
        }
        if (Input.GetKey(KeyCode.A))
        {
            Steer(-steerForce);
        }
        if (Input.GetKey(KeyCode.D))
        {
            Steer(steerForce);
        }
    }

    void AddWheelTorque(Rigidbody wheel, float accelerationAmount, int dir)
    {
        wheel.AddRelativeTorque(Vector3.right * accelerationAmount * dir);
    }

    void SteerWheel(Rigidbody axle, float direction)
    {
        axle.transform.Rotate(Vector3.up, direction, Space.World);
    }
}
