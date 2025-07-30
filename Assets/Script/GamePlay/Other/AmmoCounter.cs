using System.Collections.Generic;
using UnityEngine;

public static class AmmoCounter
{
    // Quy đổi BusType -> số đạn
    /// <summary>
    /// Tính tổng đạn từ tất cả Bus trong Scene + Bus trong WareHouse
    /// </summary>
    public static List<AmmoEntry> CalculateAmmoInSceneAndWareHouse()
    {
        Dictionary<BusColor, int> ammoDict = new Dictionary<BusColor, int>();

        // 1️⃣ Quét BusVisual đã spawn trong Scene
        BusVisual[] buses = Object.FindObjectsOfType<BusVisual>();
        foreach (var bus in buses)
        {
            if (bus.busColor == BusColor.None) continue;
            int ammo = Constants.BulletAmount[(int)bus.busType];

            if (!ammoDict.ContainsKey(bus.busColor))
                ammoDict[bus.busColor] = 0;

            ammoDict[bus.busColor] += ammo;
        }

        // 2️⃣ Quét tất cả WareHouse
        WareHouse[] warehouses = Object.FindObjectsOfType<WareHouse>();
        foreach (var wh in warehouses)
        {
            foreach (var bus in wh.busList)
            {
                if (bus.busColor == BusColor.None) continue;
                int ammo = Constants    .BulletAmount[(int)bus.busType];

                if (!ammoDict.ContainsKey(bus.busColor))
                    ammoDict[bus.busColor] = 0;

                ammoDict[bus.busColor] += ammo;
            }
        }

        // 3️⃣ Convert sang List<AmmoEntry>
        List<AmmoEntry> result = new List<AmmoEntry>();
        foreach (var kvp in ammoDict)
        {
            result.Add(new AmmoEntry { color = kvp.Key, count = kvp.Value });
        }

        return result;
    }
}

