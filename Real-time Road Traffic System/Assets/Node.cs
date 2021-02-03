using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Vector3 Position { get { return transform.position; } }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(Position, 1f);
    }
}
