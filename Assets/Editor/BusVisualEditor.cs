using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BusVisual))]
public class BusVisualEditor : Editor
{
    private BusVisual _busVisual;
    private float _lastRotationZ;

    private bool _holdingR;
    private bool _holdingT;
    private bool _holdingC;

    private void OnEnable()
    {
        _busVisual = (BusVisual)target;
        _lastRotationZ = _busVisual.transform.eulerAngles.z;
        SceneView.duringSceneGui += DuringSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= DuringSceneGUI;
    }

    private void DuringSceneGUI(SceneView sceneView)
    {
        if (_busVisual == null) return;

        Event e = Event.current;

        // Track key state
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.R) _holdingR = true;
            if (e.keyCode == KeyCode.T) _holdingT = true;
            if (e.keyCode == KeyCode.C) _holdingC = true;
        }
        else if (e.type == EventType.KeyUp)
        {
            if (e.keyCode == KeyCode.R) _holdingR = false;
            if (e.keyCode == KeyCode.T) _holdingT = false;
            if (e.keyCode == KeyCode.C) _holdingC = false;
        }

        // Auto update VisualConfig when rotation changed
        float currentZ = _busVisual.transform.eulerAngles.z;
        if (!Mathf.Approximately(currentZ, _lastRotationZ))
        {
            _lastRotationZ = currentZ;
            _busVisual.VisualConfig();
            EditorUtility.SetDirty(_busVisual);
        }

        // Process ScrollWheel
        if (e.type == EventType.ScrollWheel)
        {
            float scrollDir = -Mathf.Sign(e.delta.y);

            if (_holdingR || _holdingT || _holdingC)
            {
                // Chặn zoom SceneView
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                if (_holdingR)
                {
                    Undo.RecordObject(_busVisual.transform, "Rotate Bus");
                    _busVisual.transform.Rotate(0f, 0f, scrollDir * 5f);
                    _busVisual.VisualConfig();
                    _busVisual.RecenterCollider();
                    EditorUtility.SetDirty(_busVisual);
                }
                else if (_holdingT)
                {
                    Undo.RecordObject(_busVisual, "Change Bus Type");
                    int maxType = System.Enum.GetValues(typeof(BusType)).Length;
                    int newType = ((int)_busVisual.busType + (int)scrollDir + maxType) % maxType;
                    _busVisual.busType = (BusType)newType;
                    _busVisual.VisualConfig();
                    _busVisual.RecenterCollider();
                    EditorUtility.SetDirty(_busVisual);
                }
                else if (_holdingC)
                {
                    Undo.RecordObject(_busVisual, "Change Bus Color");
                    int maxColor = System.Enum.GetValues(typeof(BusColor)).Length;
                    int newColor = ((int)_busVisual.busColor + (int)scrollDir + maxColor) % maxColor;
                    _busVisual.busColor = (BusColor)newColor;
                    _busVisual.VisualConfig();
                    EditorUtility.SetDirty(_busVisual);
                }

                e.Use(); // Ăn luôn event
            }
        }
    }
}
