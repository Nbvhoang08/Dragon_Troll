using UnityEngine;

public class Clickable : MonoBehaviour
{
    public int index; // Index trong danh s�ch bodyParts
    public SnakeController controller;

    void OnMouseDown()
    {
        controller.RemoveSegment(index);
    }
}
