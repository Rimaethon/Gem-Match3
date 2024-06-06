using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class LightBallLightBallMergeParticleEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private ParticleSystem powerBallParticleEffect;
    [SerializeField] private GameObject lightBall1;
    [SerializeField] private GameObject lightBall2;
    [SerializeField] private Vector3 lightBall1InitialPosition = new Vector3(0.25f,0f,0);
    [SerializeField] private Vector3 lightBall2InitialPosition = new Vector3(-0.25f,0f,0);
    [SerializeField] private int loopCount = 5;
    private bool _isExploding;
    private Sequence _scaleLightBall1;
    private Sequence _scaleLightBall2;
    private float _duration = 0.4f; 
    public void InitializeEffect(float duration)
    {
        this._duration = duration;
        ResetEffect();
        powerBallParticleEffect.gameObject.SetActive(true);
        powerBallParticleEffect.Play();
        MoveLightBall(lightBall1, lightBall2InitialPosition);
        MoveLightBall(lightBall2, lightBall1InitialPosition);
        _scaleLightBall1 = ScaleLightBall(lightBall1, Vector3.one * 0.75f, Vector3.one, Vector3.one * 1.25f, Vector3.one);
        _scaleLightBall2 = ScaleLightBall(lightBall2, Vector3.one * 1.25f, Vector3.one, Vector3.one * 0.75f, Vector3.one);
    }

    private void ResetEffect()
    {
        _isExploding = false;
        lightBall1.SetActive(true);
        lightBall2.SetActive(true);
    }

    private Sequence ScaleLightBall(GameObject lightBall, Vector3 scale1, Vector3 scale2, Vector3 scale3, Vector3 scale4)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(ScaleAndSort(lightBall, scale1));
        sequence.Append(ScaleAndSort(lightBall, scale2));
        sequence.Append(ScaleAndSort(lightBall, scale3));
        sequence.Append(ScaleAndSort(lightBall, scale4));
        sequence.SetLoops(-1, LoopType.Restart);
        return sequence;
    }

    private void MoveLightBall(GameObject lightBall, Vector3 destination)
    {
        lightBall.transform.DOLocalMove(destination, _duration)
            .SetUpdate(UpdateType.Fixed)
            .SetEase(Ease.InOutSine)
            .SetLoops(loopCount, LoopType.Yoyo).onComplete += () =>
        {
            _scaleLightBall1.Kill();
            _scaleLightBall2.Kill();
            lightBall.transform.DOLocalMove(lightBall.transform.localPosition * 2, _duration)
                .SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete += () =>
            {
                lightBall.transform.DOLocalMove(Vector3.zero, _duration / 2)
                    .SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete += Explode;
            };
        };
    }

    private void Explode()
    {
        if (_isExploding)
        {
            return;
        }
        _isExploding = true;
        lightBall1.SetActive(false);
        lightBall2.SetActive(false);
        powerBallParticleEffect.gameObject.SetActive(false);
        explosionParticles.Play(true);
    }

    private Tween ScaleAndSort(GameObject lightBall, Vector3 scale)
    {
        return lightBall.transform.DOScale(scale, _duration / 2) // Use duration/2 for scale duration
            .SetUpdate(UpdateType.Fixed)
            .SetEase(Ease.InOutSine)
            .OnUpdate(() => lightBall.GetComponent<SpriteRenderer>().sortingOrder = (int)(lightBall.transform.localScale.x * 15));
    }
}