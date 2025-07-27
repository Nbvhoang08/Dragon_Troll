using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<Transform> checkpoints;
    public bool MusicOn = true;
    public bool HapticOn = true;
    public Slot[] slots;
    public Slot ValidSlot()
    {
        Slot bestSlot = null;
        float highestPriority = float.MinValue;

        foreach (var slot in slots)
        {
            if (!slot.isOccupied && slot.priority > highestPriority)
            {
                highestPriority = slot.priority;
                bestSlot = slot;
            }
        }
        return bestSlot;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 0 = Left click
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; // Đảm bảo z = 0 nếu là game 2D

            Effect clickEffect = Pool.Instance.clickedEffect;
            clickEffect.transform.position = mousePos;
        }
    }
}
