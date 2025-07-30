using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public List<Transform> checkpoints;
    public bool MusicOn = true;
    public bool HapticOn = true;
    public Slot[] slots;
    public GameState gameState = GameState.Playing;
    public GameObject DarkBG;

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
        if (Input.GetKeyDown(KeyCode.E))
        {
            RemoveCanon();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
           SlipBus();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Boost();
        }

    }
    public void Boost() 
    {
        GameEvents.BoostFire?.Invoke();
    }
    void SlipBus()
    {
        gameState = GameState.Slip;
        DarkBG.SetActive(true);
        SpriteRenderer sr = DarkBG.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        // Fade alpha từ 0 -> 1
        sr.material.DOFade(1f, 1f)
          .SetEase(Ease.Linear);
    }
    public void SlipDone()
    {
          gameState = GameState.Playing;
        // Fade alpha từ 1 -> 0
        SpriteRenderer sr = DarkBG.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        // Fade alpha từ 1 -> 0
        sr.material.DOFade(0f, 1f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {
              DarkBG.SetActive(false); // Tắt object sau khi fade
          });

    }
    void RemoveCanon() 
    {
        gameState = GameState.RemoveCanon;
        DarkBG.SetActive(true);
        DarkBG.GetComponent<SpriteRenderer>().sortingOrder = 55;
        SpriteRenderer sr = DarkBG.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        // Fade alpha từ 0 -> 1
        sr.material.DOFade(1f, 0.5f)
          .SetEase(Ease.Linear);
        GameEvents.RemoveCanon?.Invoke(true);

    }
    public void RemoveCanonDone() 
    {
        gameState = GameState.Playing;
        SpriteRenderer sr = DarkBG.GetComponent<SpriteRenderer>();
        if (sr == null) return;
        sr.material.DOFade(0f, 1f)
          .SetEase(Ease.Linear)
          .OnComplete(() =>
          {
              DarkBG.SetActive(false); // Tắt object sau khi fade
          });
    }
}
