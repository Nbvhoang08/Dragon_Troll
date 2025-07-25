using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BusVisual))]
public class BusVisualEditor : Editor
{
    private BusVisual _busVisual;
    private float _lastRotationZ;

    private void OnEnable()
    {
        _busVisual = (BusVisual)target;
        _lastRotationZ = _busVisual.transform.eulerAngles.z;
    }

    private void OnSceneGUI()
    {
        if (_busVisual == null) return;

        float currentZ = _busVisual.transform.eulerAngles.z;
        if (!Mathf.Approximately(currentZ, _lastRotationZ))
        {
            _lastRotationZ = currentZ;
            _busVisual.VisualConfig();

            // Đảm bảo Unity hiểu là object đã thay đổi
            EditorUtility.SetDirty(_busVisual);
        }
    }
}
