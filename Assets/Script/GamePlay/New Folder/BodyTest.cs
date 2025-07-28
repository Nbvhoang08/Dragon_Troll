using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyTest : MonoBehaviour , IColorBody
{
    [field: SerializeField] public BusColor busColor { get; set; }
    [field: SerializeField] public bool targetLocked { get; set; }
    [field: SerializeField] public int index { get; set; }

    public void OnHit()
    {
        Destroy(gameObject);
    }
}
