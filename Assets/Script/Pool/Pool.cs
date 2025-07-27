using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool : Singleton<Pool>
{
    [SerializeField] BulletPool bulletPool;
    [SerializeField] EffectPool CollisionEffect, ClickedEffect;
    [SerializeField] EffectPool[] BodyEffect;



    public Bullet bulletEffect { get { return bulletPool.GetPrefabInstance(); } }
    public Effect collisionEffect { get { return CollisionEffect.GetPrefabInstance(); } }
    public Effect clickedEffect { get { return ClickedEffect.GetPrefabInstance(); } }
    public Effect bodyEffect(BusColor typeBody)
    {
        return BodyEffect[(int)typeBody].GetPrefabInstance();
    }
}
