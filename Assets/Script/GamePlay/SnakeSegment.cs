using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSegment : MonoBehaviour
{
    public int index; // Index trong danh sách bodyParts
    public SnakeController controller;
    private Tween segmentTween;
    void OnMouseDown()
    {
        controller.RemoveSegment(index);
    }
    public void Init(Vector3[] path, float moveDuration, PathType pathType) 
    {
        segmentTween = transform.DOPath(path, moveDuration, pathType, PathMode.TopDown2D)
                            .SetEase(Ease.Linear);
    }
    public void PauseMove()
    {
        if (segmentTween != null && segmentTween.IsActive() && segmentTween.IsPlaying())
        {
            segmentTween.Pause();
        }
    }
    public void continueTween()
    {
        if (segmentTween != null && segmentTween.IsActive() && !segmentTween.IsPlaying())
        {
            segmentTween.Play();
        }
    }   
    public void RemoveSegment()
    {
        if (segmentTween != null)
        {
            segmentTween.Kill();
        }
        Destroy(gameObject);
    }

}
