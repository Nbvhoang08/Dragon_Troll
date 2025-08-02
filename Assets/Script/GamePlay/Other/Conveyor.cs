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

    public Renderer targetRenderer;  // Renderer có material muốn scroll
    public Vector2 scrollSpeed = new Vector2(0.5f, 0f); // cuộn theo X

    private Material _mat;
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
        _mat = targetRenderer.material;
    }


    void Update()
    {
        if (GameManager.Instance.gameState != GameState.Playing) return;
        if (_isPaused || AllBusesReady()) return;

        foreach (var bus in busList)
        {
            bus.transform.Translate(-Vector3.right * speed * Time.deltaTime);

            if (bus.transform.position.x <= endX)
            {
                bus.transform.position = new Vector3(resetX, bus.transform.position.y, bus.transform.position.z);
            }
        }
        // Offset theo thời gian
        Vector2 offset = _mat.mainTextureOffset;
        offset += scrollSpeed * Time.deltaTime;
        _mat.mainTextureOffset = offset;
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
