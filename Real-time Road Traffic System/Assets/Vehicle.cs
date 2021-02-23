using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public float accelerationForce;
    public float brakeForce;
    public float steerForce;
    public float maxSteeringAngle;

    public Transform[] driveWheels;

    Rigidbody rb;

    float steeringAngle;
    float drivingAngle;

    Vector3 turningPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        turningPoint = new Vector3();
        foreach (Transform t in driveWheels)
            turningPoint += t.position;
        turningPoint /= driveWheels.Length;
    }

    public void Accelerate(float accelerationAmount)
    {
        rb.AddRelativeForce(Vector3.forward * accelerationAmount);
    }

    public void Brake(float brakeAmount)
    {
        rb.AddRelativeForce(Vector3.back * brakeAmount);
    }

    public void Steer(float direction)
    {
        steeringAngle = Mathf.Clamp(steeringAngle + direction, -maxSteeringAngle, maxSteeringAngle);

        RotateWheel(driveWheels[0]);
        RotateWheel(driveWheels[1]);
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

    private void FixedUpdate()
    {
        transform.RotateAroundLocal(Vector3.up, steeringAngle * rb.velocity.z * Time.deltaTime);
    }

    void RotateWheel(Transform wheel)
    {
        driveWheels[0].rotation = Quaternion.AngleAxis(steeringAngle, Vector3.up);
    }
}
