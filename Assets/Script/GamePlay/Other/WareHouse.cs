using System.Collections.Generic;
using UnityEngine;
using System.Data;
using DG.Tweening;
using TMPro;




#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class WareHouse : MonoBehaviour
{
    public List<GameObject> DirWareHouse;
    public BusDirection busDirection;
    public List<BusInWareHouse> busList;
    public List<Bus> buses;
    public LayerMask busLayerMask;
    public int currentBusIndex = 0;
    public TextMeshPro[] busCount;
    void Start()
    {
        currentBusIndex = 0;
        busCount[(int)busDirection].text = busList.Count.ToString();
        CheckFrontAndSpawnBus();
    }
    
    public void SetUpBus() 
    {
        if (Pool.Instance == null) return;
        if (currentBusIndex >= busList.Count) return;
        Bus bus = Pool.Instance.bus;
        bus._collider.enabled = false;
        bus.transform.position = transform.position;
        bus.transform.rotation = Quaternion.Euler(0f, 0f, ConvertBusDirToAngle());
        bus._busVisual.busColor = busList[currentBusIndex].busColor;
        bus._busVisual.busType = busList[currentBusIndex].busType;
        bus._busVisual.VisualConfig();
        bus.wareHouse = this;
        buses.Add(bus);
        Vector2 dir = GetDirectionFromBusDirection(busDirection);
        MoveBusOut(bus, dir);


    }

    public void NextBus()
    {
        if (currentBusIndex >= busList.Count)
        {
            return;
        }
        SetUpBus();
        currentBusIndex++;
        busCount[(int)busDirection].text = (busList.Count - currentBusIndex).ToString();

    }

    public void SetWareHouse()
    {
        for (int i = 0; i < DirWareHouse.Count; i++)
        {
            if (DirWareHouse[i] != null)
            {
                DirWareHouse[i].SetActive(i == (int)busDirection);
            }
        }
    }

    public void CheckFrontAndSpawnBus()
    {
        Vector2 dir = GetDirectionFromBusDirection(busDirection);
        Vector2 origin = (Vector2)transform.position;
        float distance = 2f;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, distance, busLayerMask);

        if (hit.collider == null)
        {
            NextBus(); // tạo bus mới
        }
        else 
        {
            Bus bus = hit.collider.GetComponent<Bus>();
            if (bus != null)
            {
                bus.wareHouse = this;
            }
        }
    }
    private void OnDrawGizmos()
    {
        // Màu raycast
        Gizmos.color = Color.red;

        // Lấy hướng ray giống lúc check
        Vector2 dir = GetDirectionFromBusDirection(busDirection);
        Vector2 origin = transform.position;
        float distance = 2f;

        // Vẽ ray trong Scene
        Gizmos.DrawLine(origin, origin + dir * distance);

        // Vẽ quả cầu nhỏ ở đầu ray để dễ nhìn
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(origin + dir * distance, 0.1f);

        // Vẽ vị trí Warehouse
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(origin, 0.1f);
    }


    private void MoveBusOut(Bus bus, Vector2 direction)
    {
        float moveDistance = 1.75f;
        float moveDuration = moveDistance / 10f;

        Vector3 targetPos = bus.transform.position + (Vector3)(direction * moveDistance);
        bus.transform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear).OnComplete(()=> bus._collider.enabled = true);
    }

    private Vector2 GetDirectionFromBusDirection(BusDirection dir)
    {
        return dir switch
        {
            BusDirection.Up => Vector2.up,
            BusDirection.Down => Vector2.down,
            BusDirection.Left => Vector2.left,
            BusDirection.Right => Vector2.right,
            BusDirection.UpLeft => (Vector2.up + Vector2.left).normalized,
            BusDirection.UpRight => (Vector2.up + Vector2.right).normalized,
            BusDirection.DownLeft => (Vector2.down + Vector2.left).normalized,
            BusDirection.DownRight => (Vector2.down + Vector2.right).normalized,
            _ => Vector2.zero
        };
    }


    float ConvertBusDirToAngle()
    {
        float angle = 0f;

        switch (busDirection)
        {
            case BusDirection.Up:
                angle = 0f;
                break;
            case BusDirection.Down:
                angle = 180f;
                break;
            case BusDirection.Left:
                angle = 90f;
                break;
            case BusDirection.Right:
                angle = -90f;
                break;

            case BusDirection.UpLeft:
                angle = 45f;
                break;
            case BusDirection.UpRight:
                angle = -45f;
                break;
            case BusDirection.DownLeft:
                angle = 135f;
                break;
            case BusDirection.DownRight:
                angle = -135f;
                break;
        }

        return angle;
    }




#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            if (this == null || DirWareHouse == null)
                return;
            if (this != null && DirWareHouse != null)
            {
                SetWareHouse();
                if (!Application.isPlaying)
                    EditorUtility.SetDirty(this);
            }
           
        };
    }
#endif
}

[System.Serializable]
public class BusInWareHouse
{
    public BusType busType;
    public BusColor busColor;   

}
