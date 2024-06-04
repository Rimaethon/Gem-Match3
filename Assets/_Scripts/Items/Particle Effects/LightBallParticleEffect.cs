using System;
using System.Collections.Generic;
using System.Security;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace Scripts
{
    public class LightBallParticleEffect:BoosterParticleEffect
    {
        public ParticleSystem lightBallExplosionParticles;
        public GameObject lightBallSprite;
        public List<LightBallRay> lightBallRays;
        public GameObject chargingParticleEffects;
        private Tween _rotationTween;

        public void OnEnable()
        {
            _rotationTween = lightBallSprite.transform.DOLocalRotate(new Vector3(0, 0, 1000), 1, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear);
            lightBallSprite.SetActive(true);
            chargingParticleEffects.SetActive(true);
        }

        public  void OnDisable()
        {
            if(_rotationTween != null)
            {
                _rotationTween.Kill();
                _rotationTween = null;
            }
        }
    }
}