using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Road
{
    [SerializeField, HideInInspector]
    List<Vector3> nodes; // Nodes follow the pattern :- anchor, control, control, anchor, control control, anchor, control etc.

    [SerializeField, HideInInspector]
    bool isRingRoad; // Closes the path so that the road continues in a loop

    [SerializeField, HideInInspector]
    float roadWidth; // Total road width

    [SerializeField, HideInInspector]
    float textureTiling; // Tiling amount for the road texture

    // Stores equidistant points along the length of the road used to generate the mesh and calculate route distance
    [SerializeField, HideInInspector]
    public RoadPoint[] equidistantPoints;

    public float equidistantPointDistance = 0.1f;
    public float equidistantPointAccuracy = 1; // The accuracy of the equidistant points on each road section, the higher the more accurate

    public const int MAX_EQUIDISTANT_POINTS = 4096;

    const float NODE_CLICK_DETECTION_RADIUS = 0.15f; // The sphere radius that checks if a node has been clicked on for the purposes of selection or deletion

    Mesh mesh; // The mesh of the road

    // Places the four initial control nodes at arbitrary positions
    public Road(Vector3 centre)
    {
        nodes = new List<Vector3>
        {
            centre + Vector3.left,
            centre + Vector3.left * .5f,
            centre + Vector3.right * .5f,
            centre + Vector3.right
        };
        roadWidth = 1;
    }

    // Return a node given its index
    public Vector3 this[int i]
    {
        get { return nodes[i]; }
    }

    // Return the number of nodes in the road 
    public int NodeCount
    {
        get { return nodes.Count; }
    }

    // Return the number of sections in a road
    public int SectionCount
    {
        get { return nodes.Count / 3; }
    }

    // Getter and setter for the isRingRoad boolean
    public bool IsRingRoad
    {
        get
        {
            return isRingRoad;
        }
        set
        {
            if (isRingRoad != value)
            {
                isRingRoad = value;
                // Create two new control points between the start and end anchor points
                if (isRingRoad)
                {
                    nodes.Add(nodes[nodes.Count - 1] * 2 - nodes[nodes.Count - 2]);
                    nodes.Add(nodes[0] * 2 - nodes[1]);
                }
                // Remove the two control points
                else
                {
                    nodes.RemoveRange(nodes.Count - 2, 2);
                }
            }
        }
    }

    public float RoadWidth
    {
        get { return roadWidth; }
        set { roadWidth = Mathf.Max(0f, value); }
    }

    public float TextureTiling
    {
        get { return textureTiling; }
        set { textureTiling = value; }
    }

    // Creates a new section of road
    public void CreateRoadSection(Vector3 anchorPosition)
    {
        // Create a new control node which is parallel with the previous two nodes
        nodes.Add(nodes[nodes.Count - 1] * 2 - nodes[nodes.Count - 2]);

        // Create a second control node between the previous node and the anchor position
        nodes.Add((nodes[nodes.Count - 1] + anchorPosition) * .5f);

        // Finally, create the new anchor node
        nodes.Add(anchorPosition);
    }

    // Tries to create a new section of road using a ray based on the mouse position,
    // the y value is equal to the previous anchor nodes height
    public void CreateRoadSection(Ray mouseRay)
    {
        Plane plane = new Plane(Vector3.down, nodes[nodes.Count - 1].y);
        if (!plane.Raycast(mouseRay, out float distanceFromOrigin))
            return;
        CreateRoadSection(mouseRay.GetPoint(distanceFromOrigin));
    }

    // Removes a section of road by deleting the anchor node and its corresponding control nodes
    public void RemoveRoadSection(int anchorIndex)
    {
        if (SectionCount > 2 || (!isRingRoad && SectionCount > 1))
        if (anchorIndex == 0)
        {
            if (isRingRoad)
                nodes[nodes.Count - 1] = nodes[2];
            nodes.RemoveRange(0, 3);
        }
        else if (anchorIndex == nodes.Count - 1 && !isRingRoad)
        {
            nodes.RemoveRange(anchorIndex - 2, 3);
        }
        else
        {
            nodes.RemoveRange(anchorIndex - 1, 3);
        }
    }

    // Attempt to remove a road section by checking if the mouse clicks an anchor node
    public void RemoveRoadSection(Ray mouseRay)
    {
        int anchorIndex = SelectNode(mouseRay);
        if (anchorIndex != -1 && anchorIndex % 3 == 0)
            RemoveRoadSection(anchorIndex);
    }

    // Seperate a road section by adding a new anchor node between them
    public void SeperateRoadSection(Vector3 anchorPosition, int sectionIndex)
    {
        nodes.InsertRange(sectionIndex * 3 + 2, new Vector3[]
        {
            Vector3.zero, anchorPosition, Vector3.zero
        });
    }

    // Returns a section of road by its index
    public Vector3[] GetRoadSection(int sectionIndex)
    {
        return new Vector3[]
        {
            nodes[sectionIndex * 3],
            nodes[sectionIndex * 3 + 1],
            nodes[sectionIndex * 3 + 2],
            nodes[GetNodeIndex(sectionIndex * 3 + 3)]
        };
    }

    // Can be used to change the position of a node
    public void MoveNode(int nodeIndex, Vector3 newPosition)
    {
        Vector3 movementChange = newPosition - nodes[nodeIndex];
        nodes[nodeIndex] = newPosition;

        // Handle anchor point movement - anchor points are always multiples of 3
        if (nodeIndex % 3 == 0)
        {
            // Move adjacent control points alongside
            if (nodeIndex + 1 < nodes.Count || isRingRoad)
                nodes[GetNodeIndex(nodeIndex + 1)] += movementChange;
            if (nodeIndex - 1 >= 0 || isRingRoad)
                nodes[GetNodeIndex(nodeIndex - 1)] += movementChange;
        }
        // Handle control point movement
        else
        {
            bool isNextNodeAnAnchor = (nodeIndex + 1) % 3 == 0;
            int pairedControlIndex = isNextNodeAnAnchor ? nodeIndex + 2 : nodeIndex - 2;
            int anchorIndex = isNextNodeAnAnchor ? nodeIndex + 1 : nodeIndex - 1;

            if (pairedControlIndex >= 0 && pairedControlIndex < nodes.Count || isRingRoad)
            {
                float distance = (nodes[GetNodeIndex(anchorIndex)] - nodes[GetNodeIndex(pairedControlIndex)]).magnitude;
                Vector3 direction = (nodes[GetNodeIndex(anchorIndex)] - newPosition).normalized;

                nodes[GetNodeIndex(pairedControlIndex)] = nodes[GetNodeIndex(anchorIndex)] + direction * distance;
            }
        }
    }

    // Returns the index of an anchor node from a mouse click
    public int SelectNode(Ray mouseRay)
    {
        int nodeIndex = -1;
        for (int i = 0; i < nodes.Count && nodeIndex == -1; i++)
        {
            // If ray passes through a sphere centered on an anchor node, set anchorIndex to i
            if (IntersectionFunctions.CheckLineIntersectsSphere(mouseRay, nodes[i], NODE_CLICK_DETECTION_RADIUS))
                nodeIndex = i;
        }
        return nodeIndex;
    }

    // Returns the index of a node and loops around the list if necessary
    int GetNodeIndex(int i)
    {
        return (i + nodes.Count) % nodes.Count;
    }
}
