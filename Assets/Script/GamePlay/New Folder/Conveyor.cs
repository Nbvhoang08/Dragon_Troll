using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    [SerializeField] private Vector2 boxSize = new Vector2(20f, 1f);
    [SerializeField] private LayerMask busLayer;
    public float speed = 2f;                // Tốc độ di chuyển
    public float resetX = -10f;             // Vị trí X khi bus được đưa lại đầu
    public float endX = 10f;                // Vị trí X khi bus rời khỏi màn hình
    [SerializeField] private List<Transform> Point;
    private bool _isPaused = false;
    [SerializeField] private List<Bus> busList = new();

    private void OnEnable()
    {
        GameEvents.ConveyorRun += SetConveyorState;
        GameEvents.ConveyorBusListUpdate += UpdateListBus;
    }

    private void OnDisable()
    {
        GameEvents.ConveyorRun -= SetConveyorState;
        GameEvents.ConveyorBusListUpdate -= UpdateListBus;
    }


    private void Start()
    {
        Vector2 center = (Vector2)transform.position;
        Collider2D[] results = Physics2D.OverlapBoxAll(center, boxSize, 0f, busLayer);

        foreach (var col in results)
        {
            Bus bus = col.GetComponent<Bus>();
            if (bus != null)
            {
                busList.Add(bus);
                bus.IsOnConveyor = true; 
            }
        }
        
    }


    void Update()
    {
        if (_isPaused || AllBusesReady()) return;

        foreach (var bus in busList)
        {
            bus.transform.Translate(-Vector3.right * speed * Time.deltaTime);

            if (bus.transform.position.x <= endX)
            {
                bus.transform.position = new Vector3(resetX, bus.transform.position.y, bus.transform.position.z);
            }
        }
    }

    
    public void SetConveyorState(bool canRun)
    {
        _isPaused =canRun;
    }
    void UpdateListBus(Bus bus) 
    {
        if (busList.Contains(bus))
        {
            busList.Remove(bus);
        }
        
    }
    bool AllBusesReady()
    {
        foreach (var bus in busList)
        {
            bus.isMoving = false; // Đặt trạng thái isMoving của tất cả bus về false
            return false;
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}
