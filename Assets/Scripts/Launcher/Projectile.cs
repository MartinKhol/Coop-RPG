using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Projectile : MonoBehaviour
{
    public LayerMask layerMask;
    public float timeToLive = 10f;
    public int damage = 1;
    public Effect[] effects;

    [SerializeField]
    private float speed = 1f;

    private Transform myTransform;

    private int threatMod = 0; //threat modificator
    private GameObject source = null; //who fired this projectile;

    void Start()
    {
        myTransform = GetComponent<Transform>();
        Destroy(gameObject, timeToLive);
    }

    void Update()
    {
        myTransform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    public void SetSourceAndThreat(GameObject _source, int _threadMod)
    {
        source = _source;
        threatMod = _threadMod;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (layerMask == (layerMask | (1 << collision.gameObject.layer)))
        {
            var hp = collision.GetComponent<HealthPoints>();
            if (hp != null)
            {
                if (source != null)
                    hp.Damage(damage, source.transform, threatMod);
                else
                    hp.Damage(damage);
            }

            var target = collision.GetComponent<StatusEffects>();
            if (target != null)
            {
                foreach (var effect in effects)
                {
                    var tempEfect = new Effect(effect);
                    tempEfect.Apply(target);
                }
            }
        }
        Destroy(gameObject);
    }
}
