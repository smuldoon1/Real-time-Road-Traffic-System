using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadNetwork))]
public class RoadNetworkEditor : Editor
{
    RoadNetwork network;
    Road road;

    int selectedNode = -1;
    bool nodeEditingFoldout = true;
    bool showEquidistantPointsFoldout = false;
    bool globalSettingsFoldout = false;

    static bool areGlobalsSet = false;

    static Color anchorNodeColour;
    static Color controlNodeColour;
    static Color selectedNodeColour;
    static Color equidistantPointColour;
    static Color roadPathColour;
    static Color controlConnectionColour;
    static Color vehiclePathColour;

    static float anchorNodeSize;
    static float controlNodeSize;
    static float equidistantPointSize;

    Tool previousTool = Tool.None;

    RoadNetworkEditor()
    {
        // If this is the first time selecting a road, the global settings will not have been set yet so call ResetGlobals
        if (!areGlobalsSet)
            ResetGlobals();
    }

    private void OnEnable()
    {
        network = (RoadNetwork)target;
        if (network.road == null)
            network.CreateRoad();
        road = network.road;

        // Disables the default transform/rotation/other handle when first selecting the road network
        previousTool = Tools.current;
        Tools.current = Tool.None;

        // Ensures the mesh is regenerated when the developer undos or redos
        Undo.undoRedoPerformed += GenerateMesh;
    }

    // Reset the global settings back to their default values
    void ResetGlobals()
    {
        areGlobalsSet = true;

        anchorNodeColour = new Color(0.98f, 0.17f, 0.01f, 0.75f);
        controlNodeColour = new Color(0.19f, 0.55f, 1f, 0.4f);
        selectedNodeColour = new Color(1f, 1f, 0.38f, 0.8f);
        equidistantPointColour = new Color(1f, 1f, 1f, 0.25f);
        roadPathColour = Color.yellow;
        controlConnectionColour = Color.grey;
        vehiclePathColour = Color.green;

        anchorNodeSize = 1f;
        controlNodeSize = 0.5f;
        equidistantPointSize = 0.35f;
    }

    private void OnSceneGUI()
    {
        try
        {
            DrawNetwork(); // Draw the road and the path
            SceneInput(); // Get input from the developer
            Repaint(); // Used to update the inspectors selected node
        }
        catch (System.ArgumentOutOfRangeException)
        {
            selectedNode = -1;
        }
    }

    // Draws the road network and its editing handles
    void DrawNetwork()
    {
        // Draw equidistant points
        if (showEquidistantPointsFoldout && road.equidistantPoints != null)
        {
            Handles.color = equidistantPointColour;
            for (int i = 0; i < road.equidistantPoints.Length; i++)
            {
                Handles.SphereHandleCap(0, road.equidistantPoints[i].Position, Quaternion.identity, equidistantPointSize, EventType.Repaint);
            }
        }

        // Draw road as a bezier curve
        for (int i = 0; i < road.SectionCount; i++)
        {
            Vector3[] nodes = road.GetRoadSection(i);
            Handles.color = controlConnectionColour;
            Handles.DrawLine(nodes[1], nodes[0]);
            Handles.DrawLine(nodes[2], nodes[3]);
            Handles.DrawBezier(nodes[0], nodes[3], nodes[1], nodes[2], roadPathColour, null, 2f);
        }

        // Draw all nodes
        for (int i = 0; i < road.NodeCount; i++)
        {
            if (i == selectedNode)
                Handles.color = selectedNodeColour;
            else
                Handles.color = i % 3 == 0 ? anchorNodeColour : controlNodeColour;
            Handles.SphereHandleCap(0, road[i], Quaternion.identity, i % 3 == 0 ? anchorNodeSize : controlNodeSize, EventType.Repaint);
        }

        // Draw selected node as a movement handler
        if (selectedNode != -1)
        {
            Vector3 newPosition = Handles.DoPositionHandle(road[selectedNode], Quaternion.identity);
            if (road[selectedNode] != newPosition)
            {
                Undo.RecordObject(network, "Move Node");
                road.MoveNode(selectedNode, newPosition);
                GenerateMesh();
            }
        }

        // Draw vehicle paths
        if (road.equidistantPoints != null)
        {
            Handles.color = vehiclePathColour;
            for (int i = 0; i < road.equidistantPoints.Length && (i < road.equidistantPoints.Length - 1 || road.IsRingRoad); i++)
            {
                RoadPoint point0 = road.equidistantPoints[i];
                RoadPoint point1 = road.equidistantPoints[i < road.equidistantPoints.Length - 1 ? i + 1 : 0];

                Handles.DrawLine(point0.Position + point0.Up * 0.1f + point0.Right * road.RoadWidth * 0.25f, point1.Position + point0.Up * 0.1f + point1.Right * road.RoadWidth * 0.25f);
                Handles.DrawLine(point0.Position + point0.Up * 0.1f - point0.Right * road.RoadWidth * 0.25f, point1.Position + point0.Up * 0.1f - point1.Right * road.RoadWidth * 0.25f);
            }
        }
    }

    // Detects input from the developer within the scene view
    void SceneInput()
    {
        Event currentEvent = Event.current;

        // Create a new road section
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.shift)
        {
            Undo.RecordObject(network, "Create Road Section");
            road.CreateRoadSection(HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition));
            GenerateMesh();
        }

        // Delete an anchor, removing a road section
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
        {
            Undo.RecordObject(network, "Remove Road Section");
            road.RemoveRoadSection(HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition), anchorNodeSize * .5f, controlNodeSize * .5f);
            GenerateMesh();
        }

        // Select/deselect an anchor node
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
        {
            // Attempt to select a node and assign it to newSelectedNode
            int newSelectedNode = road.SelectNode(HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition), anchorNodeSize * .5f, controlNodeSize * .5f);

            // If there was a node selected and now there isn't, deselect the node
            if (selectedNode != -1 && newSelectedNode == -1)
            {
                Undo.RecordObject(network, "Deselect Anchor Node");
                selectedNode = -1;
            }
            // If a node has been selected other than itself, select the node
            if (selectedNode != newSelectedNode && newSelectedNode != -1)
            {
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive); // Stops the road network object losing focus when the mouse is left-clicked

                Undo.RecordObject(network, "Select Anchor Node");
                selectedNode = newSelectedNode;

                currentEvent.Use();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Reset the road
        if (GUILayout.Button("Reset Road"))
        {
            network.CreateRoad();
            road = network.road;
            GenerateMesh();
            SceneView.RepaintAll();
        }

        // Toggle whether or not the road loops
        bool isRingRoad = GUILayout.Toggle(road.IsRingRoad, "Toggle Ring Road");
        if (road.IsRingRoad != isRingRoad)
        {
            Undo.RecordObject(network, "Toggle Ring Road");
            road.IsRingRoad = isRingRoad;
            GenerateMesh();
        }

        // Overall road width
        float previousWidth = road.RoadWidth;
        road.RoadWidth = EditorGUILayout.FloatField("Road width", road.RoadWidth);
        if (previousWidth != road.RoadWidth)
            GenerateMesh();

        // Texture tiling value, sets the road texture to tile with a consistent length regardless of the road size
        float previousTiling = road.TextureTiling;
        road.TextureTiling = EditorGUILayout.FloatField("Texture tiling", road.TextureTiling);
        if (previousTiling != road.TextureTiling)
            GenerateMesh();

        nodeEditingFoldout = EditorGUILayout.Foldout(nodeEditingFoldout, "Edit Node");

        // Allow the selected node to be edited
        if (nodeEditingFoldout)
        {
            bool doesNodeExist = false;
            try
            {
                Vector3 node = road[selectedNode];
                doesNodeExist = true;
            }
            catch (System.ArgumentOutOfRangeException)
            {
                GUILayout.Label("No node selected.");
            }
            if (doesNodeExist)
            {
                // Move node field
                Vector3 newPosition = EditorGUILayout.Vector3Field("Node Position", road[selectedNode]);
                if (road[selectedNode] != newPosition)
                {
                    Undo.RecordObject(network, "Move Node");
                    road.MoveNode(selectedNode, newPosition);
                    GenerateMesh();
                }

                // Delete node button, only enabled if the selected node is an anchor node and there is more than 2 anchor nodes
                if (selectedNode % 3 != 0 || road.NodeCount < 7)
                    GUI.enabled = false;
                if (GUILayout.Button("Delete Node"))
                {
                    Undo.RecordObject(network, "Remove Road Section");
                    road.RemoveRoadSection(selectedNode);
                    GenerateMesh();
                }
                GUI.enabled = true;

                // Additional node information
                EditorGUILayout.LabelField("Node Index: ", selectedNode.ToString());
                EditorGUILayout.LabelField("Node Type: ", selectedNode % 3 == 0 ? "Anchor" : "Control");
            }
        }

        showEquidistantPointsFoldout = EditorGUILayout.Foldout(showEquidistantPointsFoldout, "Equidistant Point Settings");

        float previousEPD = road.equidistantPointDistance;
        float previousEPA = road.equidistantPointAccuracy;
        if (showEquidistantPointsFoldout)
        {
            road.equidistantPointDistance = EditorGUILayout.Slider("Distance between points", road.equidistantPointDistance, 0.05f, 1f);
            road.equidistantPointAccuracy = EditorGUILayout.Slider("Point calculation accuracy", road.equidistantPointAccuracy, 0f, 1f);

            // Equidistant point information
            EditorGUILayout.LabelField("Number of points: ", road.equidistantPoints.Length.ToString());
        }
        if (GUILayout.Button("Generate Road Mesh") || previousEPD != road.equidistantPointDistance || previousEPA != road.equidistantPointAccuracy)
            GenerateMesh();

        globalSettingsFoldout = EditorGUILayout.Foldout(globalSettingsFoldout, "Global Settings");

        if (globalSettingsFoldout)
        {
            anchorNodeSize = Mathf.Max(0, EditorGUILayout.FloatField("Anchor node size", anchorNodeSize));
            controlNodeSize = Mathf.Max(0, EditorGUILayout.FloatField("Control node size", controlNodeSize));
            equidistantPointSize = Mathf.Max(0, EditorGUILayout.FloatField("Equidistant point size", equidistantPointSize));

            anchorNodeColour = EditorGUILayout.ColorField("Anchor node colour", anchorNodeColour);
            controlNodeColour = EditorGUILayout.ColorField("Control node colour", controlNodeColour);
            selectedNodeColour = EditorGUILayout.ColorField("Selected node colour", selectedNodeColour);
            equidistantPointColour = EditorGUILayout.ColorField("Equidistant point colour", equidistantPointColour);
            controlConnectionColour = EditorGUILayout.ColorField("Anchor-to-control line colour", controlConnectionColour);
            roadPathColour = EditorGUILayout.ColorField("Road path colour", roadPathColour);
            vehiclePathColour = EditorGUILayout.ColorField("Vehicle path colour", vehiclePathColour);

            if (GUILayout.Button("Reset Global Settings"))
                ResetGlobals();
        }
    }

    // Generate the mesh of the road
    public void GenerateMesh()
    {
        if (!GenerateEquidistantPoints())
        {
            Debug.LogError("Error creating road: Too many points (>" + Road.MAX_EQUIDISTANT_POINTS + "). Reduce the complexity of the road or increase the distance between the points.");
            return;
        }
        network.SetMesh(RoadMesh.CreateMesh
        (
            road.equidistantPoints,
            road.RoadWidth,
            road.IsRingRoad),
            new Vector2(1, Mathf.RoundToInt(road.TextureTiling * road.equidistantPoints.Length * road.equidistantPointDistance))
        );
    }

    // Generates equidistant points along the road, returns false if there are too many points
    public bool GenerateEquidistantPoints()
    {
        List<Vector3> positions = new List<Vector3>();

        positions.Add(road[0]); // Start with the position of the first node
        Vector3 previousPoint = road[0];
        float previousDistance = 0f;
        for (int i = 0; i < road.SectionCount; i++)
        {
            Vector3[] roadSection = road.GetRoadSection(i);

            float nodePerimeterLength = Vector3.Distance(road[0], road[1]) + Vector3.Distance(road[1], road[2]) + Vector3.Distance(road[2], road[3]);
            float estimatedLength = Vector3.Distance(road[0], road[3]) + nodePerimeterLength * 0.5f;
            int divisions = Mathf.CeilToInt(estimatedLength * road.equidistantPointAccuracy * 10);

            // Keep placing points along the curve until the end of the road section
            float time = 0;
            while (time <= 1)
            {
                time += 1f / divisions;
                Vector3 point = Curve.CubicCurve(roadSection[0], roadSection[1], roadSection[2], roadSection[3], time);
                previousDistance += Vector3.Distance(previousPoint, point);

                while (previousDistance >= road.equidistantPointDistance)
                {
                    float overEstimate = previousDistance - road.equidistantPointDistance;
                    Vector3 adjustedPoint = point + (previousPoint - point).normalized * overEstimate;
                    positions.Add(adjustedPoint);
                    previousDistance = overEstimate;
                    previousPoint = adjustedPoint;

                    // If there are too many points in the road cancel the operation and return
                    if (positions.Count > Road.MAX_EQUIDISTANT_POINTS)
                        return false;
                }
                previousPoint = point;
            }
        }
        // After getting all of the points on the road, get their forward vector and store them together in a list of RoadPoints
        List<RoadPoint> roadPoints = new List<RoadPoint>();
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 forward = Vector3.zero;
            if (i < positions.Count - 1 || road.IsRingRoad)
                forward += positions[(i + 1) % positions.Count] - positions[i];
            if (i > 0 || road.IsRingRoad)
                forward += positions[i] - positions[(i - 1 + positions.Count) % positions.Count];
            forward.Normalize();
            roadPoints.Add(new RoadPoint(positions[i], forward));
        }
        road.equidistantPoints = roadPoints.ToArray(); // Convert the point list and store it in the road class
        return true;
    }

    void OnDisable()
    {
        // Enables the tool again once the road is deselected
        Tools.current = previousTool;
    }
}

public enum PathMode
{
    NONE, CONTINUOUS, INTERVALS
}