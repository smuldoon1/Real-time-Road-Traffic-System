using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadNetwork))]
public class RoadNetworkEditor : Editor
{
    RoadNetwork network;
    Road selectedRoad;

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

    static Material defaultMaterial;

    Tool previousTool = Tool.None;

    private void OnEnable()
    {
        network = (RoadNetwork)target;

        // If this is the first time selecting a road, the global settings will not have been set yet so call ResetGlobals
        if (!areGlobalsSet)
            ResetGlobals();

        if (network.roads == null || network.roads.Count == 0)
            network.CreateRoadNetwork(defaultMaterial);

        selectedRoad = network.ActiveRoad;

        // Disables the default transform/rotation/other handle when first selecting the road network
        previousTool = Tools.current;
        Tools.current = Tool.None;

        // Ensures the mesh is regenerated when the developer undos or redos
        Undo.undoRedoPerformed += GenerateMesh;

        network.OnRoadSelected += SelectRoad;
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

        defaultMaterial = (Material)EditorGUIUtility.Load("Materials/Road-hd.mat");
    }

    void SelectRoad(Road activeRoad)
    {
        if (activeRoad != null)
            selectedRoad = activeRoad;
    }

    private void OnSceneGUI()
    {
        try
        {
            if (selectedRoad != null)
            {
                DrawNetwork(); // Draw the road and the path
                SceneInput(); // Get input from the developer
                Repaint(); // Used to update the inspectors selected node
            }
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
        if (showEquidistantPointsFoldout && selectedRoad.equidistantPoints != null)
        {
            Handles.color = equidistantPointColour;
            for (int i = 0; i < selectedRoad.equidistantPoints.Length; i++)
            {
                Handles.SphereHandleCap(0, selectedRoad.equidistantPoints[i].Position, Quaternion.identity, equidistantPointSize, EventType.Repaint);
            }
        }

        // Draw road as a bezier curve
        for (int i = 0; i < selectedRoad.SectionCount; i++)
        {
            Vector3[] nodes = selectedRoad.GetRoadSection(i);
            Handles.color = controlConnectionColour;
            Handles.DrawLine(nodes[1], nodes[0]);
            Handles.DrawLine(nodes[2], nodes[3]);
            Handles.DrawBezier(nodes[0], nodes[3], nodes[1], nodes[2], roadPathColour, null, 2f);
        }

        // Draw all nodes
        for (int i = 0; i < selectedRoad.NodeCount; i++)
        {
            if (i == selectedNode)
                Handles.color = selectedNodeColour;
            else
                Handles.color = i % 3 == 0 ? anchorNodeColour : controlNodeColour;
            Handles.SphereHandleCap(0, selectedRoad[i], Quaternion.identity, i % 3 == 0 ? anchorNodeSize : controlNodeSize, EventType.Repaint);
        }

        // Draw selected node as a movement handler
        if (selectedNode != -1)
        {
            Vector3 newPosition = Handles.DoPositionHandle(selectedRoad[selectedNode], Quaternion.identity);
            if (selectedRoad[selectedNode] != newPosition)
            {
                Undo.RecordObject(selectedRoad, "Move Node");
                selectedRoad.MoveNode(selectedNode, newPosition);
                GenerateMesh();
            }
        }

        // Draw vehicle paths
        if (selectedRoad.equidistantPoints != null)
        {
            Handles.color = vehiclePathColour;
            for (int i = 0; i < selectedRoad.equidistantPoints.Length && (i < selectedRoad.equidistantPoints.Length - 1 || selectedRoad.IsRingRoad); i++)
            {
                RoadPoint point0 = selectedRoad.equidistantPoints[i];
                RoadPoint point1 = selectedRoad.equidistantPoints[i < selectedRoad.equidistantPoints.Length - 1 ? i + 1 : 0];

                Handles.DrawLine(point0.Position + point0.Up * 0.1f + point0.Right * selectedRoad.RoadWidth * 0.25f, point1.Position + point0.Up * 0.1f + point1.Right * selectedRoad.RoadWidth * 0.25f);
                Handles.DrawLine(point0.Position + point0.Up * 0.1f - point0.Right * selectedRoad.RoadWidth * 0.25f, point1.Position + point0.Up * 0.1f - point1.Right * selectedRoad.RoadWidth * 0.25f);
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
            Undo.RecordObject(selectedRoad, "Create Road Section");
            selectedRoad.CreateRoadSection(HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition), selectedNode, selectedRoad.RoadWidth * .5f);
            GenerateMesh();
        }

        // Create a new road section
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && currentEvent.control)
        {
            selectedRoad = network.CreateNewRoad(defaultMaterial);
            Undo.RecordObject(network, "Create new road");
            GenerateMesh();
        }

        // Delete an anchor, removing a road section
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
        {
            Undo.RecordObject(network, "Remove Road Section");
            selectedRoad.RemoveRoadSection(HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition), anchorNodeSize * .5f, controlNodeSize * .5f);
            GenerateMesh();
        }

        // Select/deselect an anchor node
        if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
        {
            // Attempt to select a node and assign it to newSelectedNode
            int newSelectedNode = selectedRoad.SelectNode(HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition), anchorNodeSize * .5f, controlNodeSize * .5f);

            // If there was a node selected and now there isn't, deselect the node
            if (selectedNode != -1 && newSelectedNode == -1)
            {
                Undo.RecordObject(selectedRoad, "Deselect Anchor Node");
                selectedNode = -1;
            }
            // If a node has been selected other than itself, select the node
            if (selectedNode != newSelectedNode && newSelectedNode != -1)
            {
                GUIUtility.hotControl = GUIUtility.GetControlID(FocusType.Passive); // Stops the road network object losing focus when the mouse is left-clicked

                Undo.RecordObject(selectedRoad, "Select Anchor Node");
                selectedNode = newSelectedNode;

                currentEvent.Use();
            }
        }

        // Delete the selected road when pressing DEL
        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Delete)
        {
            DeleteRoad();
            currentEvent.Use();
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        // Delete the road
        if (GUILayout.Button("Delete Road"))
            DeleteRoad();

        // Reset the entire road network
        if (GUILayout.Button("Reset Road Network"))
        {
            Undo.RecordObject(network, "Reset road network");
            network.CreateRoadNetwork(defaultMaterial);
            selectedRoad = network.roads[0];
            GenerateMesh();
            SceneView.RepaintAll();
        }
        EditorGUILayout.EndHorizontal();

        if (selectedRoad != null)
        {
            // Toggle whether or not the road loops
            bool isRingRoad = GUILayout.Toggle(selectedRoad.IsRingRoad, "Toggle Ring Road");
            if (selectedRoad.IsRingRoad != isRingRoad)
            {
                Undo.RecordObject(selectedRoad, "Toggle Ring Road");
                selectedRoad.IsRingRoad = isRingRoad;
                GenerateMesh();
            }

            // Road speed limit
            selectedRoad.SpeedLimit = EditorGUILayout.FloatField("Maximum speed limit", selectedRoad.SpeedLimit);

            // Overall road width
            float previousWidth = selectedRoad.RoadWidth;
            selectedRoad.RoadWidth = EditorGUILayout.FloatField("Road width", selectedRoad.RoadWidth);
            if (previousWidth != selectedRoad.RoadWidth)
                GenerateMesh();

            // Texture tiling value, sets the road texture to tile with a consistent length regardless of the road size
            float previousTiling = selectedRoad.TextureTiling;
            selectedRoad.TextureTiling = EditorGUILayout.Slider("Texture tiling", selectedRoad.TextureTiling, 0.01f, 1f);
            if (previousTiling != selectedRoad.TextureTiling)
                GenerateMesh();

            // Texture tiling value, sets the road texture to tile with a consistent length regardless of the road size
            Material previousMaterial = selectedRoad.Material;
            selectedRoad.Material = (Material)EditorGUILayout.ObjectField("Material", selectedRoad.Material, typeof(Material), false);
            if (previousMaterial != selectedRoad.Material)
            {
                selectedRoad.UpdateMaterial(selectedRoad.Material);
                GenerateMesh();
            }

            nodeEditingFoldout = EditorGUILayout.Foldout(nodeEditingFoldout, "Edit Node");

            // Allow the selected node to be edited
            if (nodeEditingFoldout)
            {
                bool doesNodeExist = false;
                try
                {
                    Vector3 node = selectedRoad[selectedNode];
                    doesNodeExist = true;
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    GUILayout.Label("No node selected.");
                }
                if (doesNodeExist)
                {
                    // Move node field
                    Vector3 newPosition = EditorGUILayout.Vector3Field("Node Position", selectedRoad[selectedNode]);
                    if (selectedRoad[selectedNode] != newPosition)
                    {
                        Undo.RecordObject(selectedRoad, "Move Node");
                        selectedRoad.MoveNode(selectedNode, newPosition);
                        GenerateMesh();
                    }

                    // Insert button creates a new node between the selected node and the next one
                    if (selectedNode % 3 != 0)
                        GUI.enabled = false;
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Insert node"))
                    {
                        Undo.RecordObject(selectedRoad, "Insert road section");
                        selectedRoad.SeperateRoadSection(selectedNode);
                        selectedNode += 3;
                        GenerateMesh();
                    }
                    // Delete node button, only enabled if the selected node is an anchor node and there is more than 2 anchor nodes
                    if (selectedRoad.NodeCount < 7)
                        GUI.enabled = false;
                    if (GUILayout.Button("Delete Node"))
                    {
                        Undo.RecordObject(selectedRoad, "Remove Road Section");
                        selectedRoad.RemoveRoadSection(selectedNode);
                        GenerateMesh();
                    }
                    GUI.enabled = true;
                    GUILayout.EndHorizontal();

                    // Additional node information
                    EditorGUILayout.LabelField("Node Index: ", selectedNode.ToString());
                    EditorGUILayout.LabelField("Node Type: ", selectedNode % 3 == 0 ? "Anchor" : "Control");
                }
            }

            showEquidistantPointsFoldout = EditorGUILayout.Foldout(showEquidistantPointsFoldout, "Equidistant Point Settings");

            float previousEPD = selectedRoad.equidistantPointDistance;
            if (showEquidistantPointsFoldout)
            {
                selectedRoad.equidistantPointDistance = EditorGUILayout.Slider("Distance between points", selectedRoad.equidistantPointDistance, 0.05f, 4f);

                // Equidistant point information
                EditorGUILayout.LabelField("Number of points: ", selectedRoad.equidistantPoints.Length.ToString());
            }

            // Manually generate mesh
            if (GUILayout.Button("Generate Road Mesh") || previousEPD != selectedRoad.equidistantPointDistance)
                GenerateMesh();

            // Mesh data
            if (selectedRoad.equidistantPoints != null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Tris: " + 6 * selectedRoad.equidistantPoints.Length);
                EditorGUILayout.LabelField("Verts: " + 2 * selectedRoad.equidistantPoints.Length);
                GUILayout.EndHorizontal();
            }
        }

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

            defaultMaterial = (Material)EditorGUILayout.ObjectField("Default road material", defaultMaterial, typeof(Material), true);

            if (GUILayout.Button("Reset Global Settings"))
                ResetGlobals();
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(network);
            if (selectedRoad != null)
                Undo.RecordObject(selectedRoad, "Road changes");
            else
                Undo.RecordObject(network, "Road network changes");
            GenerateMesh();
        }
    }

    // Deletes the selected road
    void DeleteRoad()
    {
        Undo.DestroyObjectImmediate(selectedRoad.gameObject);
        selectedRoad = null;
    }

    // Generate the mesh of the road
    public void GenerateMesh()
    {
        if (selectedRoad != null)
        {
            GenerateEquidistantPoints();
            selectedRoad.GenerateRoad(RoadMesh.CreateMesh
            (
                selectedRoad.equidistantPoints,
                selectedRoad.RoadWidth,
                selectedRoad.IsRingRoad
            ),
            new Vector2(1, Mathf.RoundToInt(selectedRoad.TextureTiling * selectedRoad.equidistantPoints.Length * selectedRoad.equidistantPointDistance)));
        }
    }

    // Generates equidistant points along the road, returns false if there are too many points
    public bool GenerateEquidistantPoints()
    {
        List<Vector3> positions = new List<Vector3>();

        positions.Add(selectedRoad[0]); // Start with the position of the first node
        Vector3 previousPoint = selectedRoad[0];
        float previousDistance = 0f;
        for (int i = 0; i < selectedRoad.SectionCount; i++)
        {
            Vector3[] roadSection = selectedRoad.GetRoadSection(i);

            float nodePerimeterLength = Vector3.Distance(selectedRoad[0], selectedRoad[1]) + Vector3.Distance(selectedRoad[1], selectedRoad[2]) + Vector3.Distance(selectedRoad[2], selectedRoad[3]);
            float estimatedLength = Vector3.Distance(selectedRoad[0], selectedRoad[3]) + nodePerimeterLength * 0.5f;
            int divisions = Mathf.CeilToInt(estimatedLength * 50);

            // Keep placing points along the curve until the end of the road section
            float time = 0;
            while (time <= 1)
            {
                time += 1f / divisions;
                Vector3 point = Curve.CubicCurve(roadSection[0], roadSection[1], roadSection[2], roadSection[3], time);
                previousDistance += Vector3.Distance(previousPoint, point);

                while (previousDistance >= selectedRoad.equidistantPointDistance)
                {
                    float overEstimate = previousDistance - selectedRoad.equidistantPointDistance;
                    Vector3 adjustedPoint = point + (previousPoint - point).normalized * overEstimate;
                    positions.Add(adjustedPoint);
                    previousDistance = overEstimate;
                    previousPoint = adjustedPoint;

                    // If there are too many points in the road cancel the operation and return
                    if (positions.Count > 65536)
                    {
                        Debug.LogError("Too many points in the road (" + positions.Count + "). The maximum amount is 65536. Decrease the equidistant point distance or reduce the complexity of the road.");
                        return false;
                    }
                }
                previousPoint = point;
            }
        }
        // After getting all of the points on the road, get their forward vector and store them together in a list of RoadPoints
        List<RoadPoint> roadPoints = new List<RoadPoint>();
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 forward = Vector3.zero;
            if (i < positions.Count - 1 || selectedRoad.IsRingRoad)
                forward += positions[(i + 1) % positions.Count] - positions[i];
            if (i > 0 || selectedRoad.IsRingRoad)
                forward += positions[i] - positions[(i - 1 + positions.Count) % positions.Count];
            forward.Normalize();
            roadPoints.Add(new RoadPoint(positions[i], forward));
        }
        selectedRoad.equidistantPoints = roadPoints.ToArray(); // Convert the point list and store it in the road class
        return true;
    }

    void OnDisable()
    {
        // Enables the tool again once the road is deselected
        Tools.current = previousTool;

        Undo.undoRedoPerformed -= GenerateMesh;

        network.OnRoadSelected -= SelectRoad;
    }
}