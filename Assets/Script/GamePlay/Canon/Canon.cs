﻿using DG.Tweening;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TreeEditor;
using UnityEngine;
using UnityEngine.WSA;

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
        SetUpAnimation();
    }


    private void SetUpAnimation() 
    {
        transform.localScale = Vector3.one;
        Sequence SetupRecoil = DOTween.Sequence();
        SetupRecoil.Append(canonSprite.transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.OutQuad));
        SetupRecoil.Append(canonSprite.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.InOutBounce));
        
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

    IEnumerator FiringLoop()
    {
        while (true)
        {
            if (_currAmmo <= 0)
            {
                DoneAnimation();
                yield break;
            }

            Collider2D target = GetTargetInFront();

            if (target != null && !isFiring)
            {
                isFiring = true;
                Vector2 dir = (target.transform.position - firePoint.position).normalized;
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                canonSprite.transform.rotation = Quaternion.Euler(0f, 0f, targetAngle - 90f);
                CanonFireAnimation(() =>
                {
                    PlayFlash(); // Hiệu ứng flash khi bắn
                    GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

                    Vector2 dirToTarget = (target.transform.position - bullet.transform.position).normalized;
                    float angle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
                    bullet.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);

                    bullet.GetComponent<Bullet>().SetTarget(target.transform);
                    bullet.GetComponent<SpriteRenderer>().sprite = bulletSprite;
                   
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
        muzzleRenderer.color = new Color(1, 1, 1, 1); // Reset alpha

        // Scale to lớn hơn rồi thu nhỏ lại
        muzzleRenderer.transform.localScale = Vector3.one * flashScale;

        Sequence muzzleSeq = DOTween.Sequence();
        muzzleSeq.Append(muzzleRenderer.DOFade(0f, flashDuration).SetEase(Ease.OutQuad)); // Fade Out
        muzzleSeq.Join(muzzleRenderer.transform.DOScale(Vector3.one, flashDuration).SetEase(Ease.OutQuad)); // Scale về gốc
        muzzleSeq.OnComplete(() => muzzleRenderer.enabled = false); // Ẩn đi sau khi xong
    }
    private Collider2D GetTargetInFront()
    {
        Vector3 screenLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 screenRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0));
        float fireWidth = Mathf.Abs(screenRight.x - screenLeft.x);

        float centerX = (screenLeft.x + screenRight.x) / 2f;
        float centerY = transform.position.y + fireHeight / 2f; // ← dùng transform thay vì firePoint

        Vector2 boxCenterWorld = new Vector2(centerX, centerY);
        Vector2 boxSize = new Vector2(fireWidth, fireHeight);

        return Physics2D.OverlapBox(boxCenterWorld, boxSize, 0f, targetMask);
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
