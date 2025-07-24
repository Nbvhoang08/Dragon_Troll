using UnityEngine;

public class Clickable : MonoBehaviour
{
    public int index; // Index trong danh sách bodyParts
    public SnakeController controller;

    void OnMouseDown()
    {
        controller.RemoveSegment(index);
    }
}
