using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed =1f;
    private Transform _target;

    public void SetTarget(Transform target)
    {
        _target = target;
    }


    void Update()
    {
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }
        transform.position = Vector3.MoveTowards(transform.position, _target.position, speed * Time.deltaTime);
        Vector3 dir = _target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg-90;
        transform.rotation = Quaternion.Euler(0f, 0f, angle); // Cho 2D, mặt đạn hướng theo trục X
        if (Vector3.Distance(transform.position, _target.position) < 0.05f)
        {
            OnHitTarget();
        }
    }

    void OnHitTarget()
    {
        // TODO: hiệu ứng nổ, trừ máu, pool lại, ...
        Destroy(gameObject);
    }

}
