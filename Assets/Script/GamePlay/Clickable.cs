using UnityEngine;

public class Clickable : MonoBehaviour
{
    void OnMouseDown()
    {
        Destroy(gameObject);
    }
}
