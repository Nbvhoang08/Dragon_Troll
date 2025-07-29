using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class Slot : MonoBehaviour
{
    public float priority = 0f;
    public bool isOccupied = false;
    public Vector3 lowerPoint;
    [SerializeField] private float raycastDistance = 5f;
    public Image bulletImage;
    [HideInInspector]public TextMeshProUGUI bulletNumberText;
    public int bulletNumber = 0;
    public Canon canon;
    [SerializeField] private SpriteRenderer BackGround;
    public ParticleSystem[] particleSystems;

    private void OnEnable()
    {
        GameEvents.RemoveCanon += SetLayer;
        GameEvents.BoostFire += BoostFireHeight;
    }

    private void OnDisable()
    {
        GameEvents.RemoveCanon -= SetLayer;
        GameEvents.BoostFire -= BoostFireHeight;
    }
    private void Start()
    {
        canon = GetComponentInChildren<Canon>();
        canon.gameObject.SetActive(false);
        bulletNumberText = GetComponentInChildren<TextMeshProUGUI>();
        if (bulletNumberText != null)
        {
            bulletNumberText.text = "";
            bulletNumberText.gameObject.SetActive(false);
        }
       
        Vector2 origin = transform.position;
        Vector2 direction = -transform.up.normalized;
        Vector2 end = origin + direction * raycastDistance;

        RaycastHit2D hit = Physics2D.Linecast(origin, end, LayerMask.GetMask("Road"));

        if (hit.collider != null)
        {
            Bounds bounds = hit.collider.bounds;
            lowerPoint= GetIntersectionWithCenterLine(origin, direction, bounds);
        }
        else
        {
            Debug.DrawLine(origin, end, Color.red, 1f);
        }
    }

    public void OnMouseDown()
    {
        if (GameManager.Instance.gameState != GameState.RemoveCanon) return;
        particleSystems[0].Play();
        canon.DoneAnimation();
        GameManager.Instance.RemoveCanonDone();
    }


    public void BoostFireHeight()
    {
        if (canon != null)
        {
            particleSystems[2].Play();
            canon.fireHeight =10;
        }
    }

    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
        
    }
    private Vector2 GetIntersectionWithCenterLine(Vector2 rayOrigin, Vector2 rayDir, Bounds bounds)
    {
        Vector2 center = bounds.center;
        Vector2 size = bounds.size;
        Vector2 centerLineStart, centerLineEnd;
        if (size.x >= size.y)
        {
            centerLineStart = new Vector2(center.x - 100f, center.y);
            centerLineEnd = new Vector2(center.x + 100f, center.y);
        }
        else
        {
            centerLineStart = new Vector2(center.x, center.y - 100f);
            centerLineEnd = new Vector2(center.x, center.y + 100f);
        }

        Vector2 rayEnd = rayOrigin + rayDir * raycastDistance;
        Vector2? intersection = LineIntersection(rayOrigin, rayEnd, centerLineStart, centerLineEnd);
        if (intersection.HasValue)
            return intersection.Value;

        Debug.LogWarning("Không tìm được giao điểm với center line!");
        return rayOrigin;
    }
    private Vector2? LineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float A1 = p2.y - p1.y;
        float B1 = p1.x - p2.x;
        float C1 = A1 * p1.x + B1 * p1.y;

        float A2 = p4.y - p3.y;
        float B2 = p3.x - p4.x;
        float C2 = A2 * p3.x + B2 * p3.y;

        float denominator = A1 * B2 - A2 * B1;

        if (Mathf.Approximately(denominator, 0f))
            return null; // Song song

        float x = (B2 * C1 - B1 * C2) / denominator;
        float y = (A1 * C2 - A2 * C1) / denominator;

        return new Vector2(x, y);
    }
    public void OnOccupied(BusColor busColor , BusType busType) 
    {
        SoundManager.Instance.Play(Constants.Occupied);
        canon.gameObject.SetActive(true);
        CanonVisualData data = canon.canonData.GetVisualData(busColor);
        canon.CanonSpriteConfig(data);
        bulletImage.sprite = data.AmmoImage;
        canon.bulletSprite = data.bulletSprite;
        bulletNumberText.gameObject.SetActive(true);
        bulletNumber = Constants.BulletAmount[(int)busType];
        canon.StartFiringLoop(bulletNumber);
        if (bulletNumberText != null)
        {
            bulletNumberText.text = Constants.BulletAmount[(int)busType].ToString();
        }
        
    }

    public void SetLayer(bool higher) 
    {
        BackGround.sortingOrder = higher? 59 :0;
    }

}
