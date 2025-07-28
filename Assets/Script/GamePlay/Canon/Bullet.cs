using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : GenericPoolableObject, IPoolable
{
    [Header("Settings")]
    public float speed =1f;
    private SnakeSegment _target;
    public BusColor color;
    public void SetTarget(SnakeSegment target,BusColor canonColor)
    {
        _target = target;
        color = canonColor;
    }


    void Update()
    {
        if (_target == null)
        {
            ReturnToPool();
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, _target.gameObject.transform.position, speed * Time.deltaTime);
        Vector3 dir = _target.gameObject.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg-90;
        transform.rotation = Quaternion.Euler(0f, 0f, angle); // Cho 2D, mặt đạn hướng theo trục X
        if (Vector3.Distance(transform.position, _target.gameObject.transform.position) < 0.05f)
        {
            OnHitTarget();
        }
    }

    void OnHitTarget()
    {
        Effect bodyEffect = Pool.Instance.bodyEffect(color);
        bodyEffect.transform.position = _target.gameObject.transform.position;
        bodyEffect.transform.localScale = Vector3.one * 2f;
        SoundManager.Instance.Play(Constants.BodySound);
        _target.GetComponent<SnakeSegment>().OnHit();
        _target = null; // Clear target after hit
        ReturnToPool();

    }

}
