using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;



public class Canon : MonoBehaviour
{
    public SpriteRenderer bottom; 
    public SpriteRenderer canonSprite;
    public CanonData canonData;
    private Slot _slot;
    private int _currAmmo;
    public TextMeshProUGUI ammoText;
    public float fireHeight = 5f;
    public LayerMask targetMask;
    public GameObject bulletPrefab;
    public Transform firePoint;
    private bool isFiring = false;
    private Coroutine firingRoutine;
    public Sprite bulletSprite;
    public float fireCooldown = 0.25f;

    [Header("Muzzle Settings")]
    public SpriteRenderer muzzleRenderer; // Hoặc bạn có thể dùng GameObject nếu dùng Particle/VFX
    public float flashScale = 1.5f;
    public float flashDuration = 0.15f;

    private BusColor _busColor;

    private void OnEnable()
    {
        GameEvents.RemoveCanon += SetLayer;
    }

    private void OnDisable()
    {
        GameEvents.RemoveCanon -= SetLayer;
    }








    private void Awake()
    {
        _slot = GetComponentInParent<Slot>();
        muzzleRenderer.enabled = false;
    }
    public void CanonSpriteConfig(CanonVisualData data) 
    {
        bottom.sprite = data.bottomSprite;
        bottom.color = data.Color;
        canonSprite.sprite = data.canonSprite;
        _busColor = data.canonColor;
        SetUpAnimation();
    }


    private void SetUpAnimation() 
    {
        transform.localScale = Vector3.one;
        Sequence SetupRecoil = DOTween.Sequence();
        SetupRecoil.Append(canonSprite.transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.OutQuad));
        SetupRecoil.Append(canonSprite.transform.DOScale(Vector3.one*0.75f, 0.25f).SetEase(Ease.InOutBounce));
        
    }
    public void CanonFire() 
    {
        Vector3 screenLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
        float fireWidth = Mathf.Abs(screenRight.x - screenLeft.x); 

        Vector2 boxCenterLocal = new Vector2(0f, fireHeight / 2f);
        Vector2 boxSize = new Vector2(fireWidth, fireHeight);
        Vector2 boxCenterWorld = firePoint.TransformPoint(boxCenterLocal);
        float angle = firePoint.eulerAngles.z;

        Collider2D target = Physics2D.OverlapBox(boxCenterWorld, boxSize, angle, targetMask);

        if (target != null)
        {
            Vector2 dir = (target.transform.position - firePoint.position).normalized;
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            canonSprite.transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);

            CanonFireAnimation(() =>
            {
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
                bullet.transform.DOMove(target.transform.position, 0.4f).SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        Destroy(bullet);
                    });
            });
        }
        else
        {
            canonSprite.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }
    public void StartFiringLoop(int AmmoAmount)
    {
        _currAmmo = AmmoAmount;
        if (firingRoutine != null) StopCoroutine(firingRoutine);
        firingRoutine = StartCoroutine(FiringLoop());
    }

    public void SetLayer(bool Higher)
    {
        bottom.sortingOrder = Higher ? 60 : 20; // Đặt layer cho bottom sprite
        canonSprite.sortingOrder = Higher ? 65 : 25 ; // Đặt layer cho canon sprite
    }

    IEnumerator FiringLoop()
    {
        yield return new WaitForSeconds(0.6f);
        while (true)
        {
            if (_currAmmo <= 0)
            {
                DoneAnimation();
                yield break;
            }

            SnakeSegment target = GetTargetInFront();

            if (target != null && !isFiring)
            {
                
                isFiring = true;
                target.targetLocked = true;
                Vector2 dir = (target.gameObject.transform.position - firePoint.position).normalized;
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                canonSprite.transform.rotation = Quaternion.Euler(0f, 0f, targetAngle - 90f);
                CanonFireAnimation(() =>
                {
                    // CannonFire Process
                    PlayFlash(); 
                    
                    Bullet bullet = Pool.Instance.bulletEffect;
                   
                    bullet.transform.position = firePoint.position;
                    Vector2 dirToTarget = (target.transform.position - bullet.transform.position).normalized;
                    float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
                    bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);

                    bullet.GetComponent<Bullet>().SetTarget(target,_busColor);
                    bullet.GetComponent<SpriteRenderer>().sprite = bulletSprite;

                    SoundManager.Instance.Play(Constants.LaunchSound);
                    _currAmmo--;
                    _slot.bulletNumberText.text = _currAmmo.ToString();
                    isFiring = false;
                });
            }
            else if (target == null)
            {
                canonSprite.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

            yield return new WaitForSeconds(fireCooldown); // delay giữa các lần check/bắn
        }
    }

    public void PlayFlash()
    {
        muzzleRenderer.enabled = true;
        muzzleRenderer.material.color = new Color(1, 1, 1, 1); // Reset alpha

        // Scale to larger size and then shrink back
        muzzleRenderer.transform.localScale = Vector3.one * flashScale;

        Sequence muzzleSeq = DOTween.Sequence();
        Material muzzleMaterial = muzzleRenderer.material; // Access the material of the SpriteRenderer
        muzzleSeq.Append(muzzleMaterial.DOFade(0f, flashDuration).SetEase(Ease.OutQuad)); // Fade Out
        muzzleSeq.Join(muzzleRenderer.transform.DOScale(Vector3.one, flashDuration).SetEase(Ease.OutQuad)); // Scale back to original
        muzzleSeq.OnComplete(() => muzzleRenderer.enabled = false); // Disable after completion
    }

    private SnakeSegment GetTargetInFront()
    {
        Vector3 screenLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
        float fireWidth = Mathf.Abs(screenRight.x - screenLeft.x);

        float centerX = (screenLeft.x + screenRight.x) / 2f;
        float centerY = transform.position.y + fireHeight / 2f;

        Vector2 boxCenterWorld = new Vector2(centerX, centerY);
        Vector2 boxSize = new Vector2(fireWidth, fireHeight);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenterWorld, boxSize, 0f, targetMask);

        SnakeSegment bestTarget = null;
        int lowestIndex = int.MaxValue;

        foreach (var hit in hits)
        {
            SnakeSegment body = hit.GetComponent<SnakeSegment>();
            if (body == null) continue;

            // So màu
            if (body.busColor != _busColor) continue;

            // Nếu đã bị khóa bởi súng khác
            if (body.targetLocked) continue;

            // Lấy target có index nhỏ nhất
            if (body.index < lowestIndex)
            {
                bestTarget = body;
                lowestIndex = body.index;
            }
        }

        return bestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        if (Camera.main == null) return;

        Vector3 screenLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
        float fireWidth = Mathf.Abs(screenRight.x - screenLeft.x);

        float centerX = (screenLeft.x + screenRight.x) / 2f;
        float centerY = transform.position.y + fireHeight / 2f; // ← đổi y

        Vector2 boxCenterWorld = new Vector2(centerX, centerY);
        Vector2 boxSize = new Vector2(fireWidth, fireHeight);

        Gizmos.color = new Color(0f, 1f, 0.2f, 0.3f);
        Gizmos.DrawCube(boxCenterWorld, boxSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(boxCenterWorld, boxSize);
    }




    public void CanonFireAnimation(System.Action onComplete = null)
    {
        canonSprite.transform.localScale = Vector3.one * 0.75f; 
        Sequence fireRecoil = DOTween.Sequence();
        fireRecoil.Append(canonSprite.transform.DOScale(new Vector3(1f, 0.6f, 0.75f), 0.1f).SetEase(Ease.OutQuad));
        fireRecoil.Append(canonSprite.transform.DOScale(new Vector3(0.75f, 1f, 0.75f), 0.2f).SetEase(Ease.InOutElastic));
        fireRecoil.Append(canonSprite.transform.DOScale(Vector3.one * 0.75f, 0.1f).SetEase(Ease.OutExpo));
        fireRecoil.OnComplete(() => onComplete?.Invoke());
    }

    public void DoneAnimation() 
    {
        Sequence DoneRecoil = DOTween.Sequence();
        DoneRecoil.Append(transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutQuad));
        DoneRecoil.Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutCubic));
        DoneRecoil.OnComplete(() =>
        {
            _slot.SetOccupied(false);
            _slot.bulletNumberText.gameObject.SetActive(false); 
            gameObject.SetActive(false);
        });
    }
    
}
