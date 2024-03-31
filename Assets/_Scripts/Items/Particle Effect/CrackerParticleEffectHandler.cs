using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

public class CrackerParticleEffectHandler : ParticleEffectHandler
{
    [SerializeField] private float speed=5;
    [SerializeField] private GameObject fireworkUp;
    [SerializeField] private GameObject fireworkDown;
    [SerializeField] private float lifeTime = 1.5f;
    private void Awake()
    {
        if (fireworkDown == null || fireworkUp == null)
        {
            Debug.LogError("Firework Up or Down is not assigned");
        }
              
    }

    private void OnEnable()
    {
        fireworkUp.SetActive(true);
        fireworkDown.SetActive(true);
        fireworkUp.transform.localRotation = Quaternion.Euler(0, 0, -90);
        fireworkDown.transform.localRotation = Quaternion.Euler(0, 0, 90);
        fireworkUp.transform.localPosition = new Vector3(0, 0.11f, 0);
        fireworkDown.transform.localPosition = new Vector3(0, -0.11f, 0); 
        lifeTime = 1.5f;
    }

    private void FixedUpdate()
    {
        fireworkUp.transform.localPosition += speed * Time.fixedDeltaTime * Vector3.up;
        fireworkDown.transform.localPosition += speed * Time.fixedDeltaTime * Vector3.down;
        lifeTime -= Time.fixedDeltaTime;
        if (lifeTime <= 0)
        {
            ObjectPool.Instance.ReturnParticleEffectToPool(gameObject, particleEffectID);
        }
    }
}
