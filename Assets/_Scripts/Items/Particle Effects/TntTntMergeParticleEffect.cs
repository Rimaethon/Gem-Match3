using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class TntTntMergeParticleEffect : MonoBehaviour
{
     [SerializeField] private List<ParticleSystem> explosionParticles;
     [SerializeField] private GameObject fuseEffect;
     [SerializeField] private Transform fuseEndDestination;
     private ParticleSystem _fuseParticle;
     private SpriteRenderer _tntSpriteRenderer;
     private Vector3 _initialFusePosition;
     private Vector3 _explosionScale = new Vector3(4f, 4f, 4f);
     private void Awake()
     {
          _fuseParticle = fuseEffect.GetComponent<ParticleSystem>();
          _initialFusePosition = fuseEffect.transform.localPosition;
          _tntSpriteRenderer = GetComponent<SpriteRenderer>();
     }
     
     public void PlayExplosionEffect()
     {
          fuseEffect.SetActive(false);
          _tntSpriteRenderer.enabled = true;
          gameObject.transform.DOScale(_explosionScale, 0.3f).SetUpdate(UpdateType.Fixed).OnComplete(() =>
          {
               fuseEffect.SetActive(true);
               _fuseParticle.Play();
               AudioManager.Instance.PlaySFX(SFXClips.FuseSoundEffect);
               fuseEffect.transform.DOLocalMove(fuseEndDestination.localPosition, 0.3f).SetUpdate(UpdateType.Fixed).OnComplete(() =>
               {
                    AudioManager.Instance.PlaySFX(SFXClips.TntTntMergeExplosionSound);
                    _fuseParticle.Stop();
                    fuseEffect.SetActive(false);
                    _tntSpriteRenderer.enabled = false;
                    foreach (var particleEffect in explosionParticles )
                    {
                         particleEffect.Play();
                    }
                    fuseEffect.transform.localPosition = _initialFusePosition;
               });
          });
     }
     
     
}
