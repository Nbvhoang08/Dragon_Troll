using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    void Start()
    {

        GameEvents.GameStart?.Invoke();
    }
    private void OnDisable()
    {
        GameEvents.GameStart -= OnStart;
    }

    private void OnEnable()
    {
        GameEvents.GameStart += OnStart;
    }

    public void OnStart()
    {
        TweenHelper.DoLocalScale(this.transform,Vector3.one*1.5f, 0.3f ,DG.Tweening.Ease.InOutQuad);
    }
}
