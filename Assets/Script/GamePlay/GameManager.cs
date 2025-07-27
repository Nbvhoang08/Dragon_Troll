using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<Transform> checkpoints;
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

}
