using DG.Tweening;
using UnityEngine;

namespace Scripts
{
    //Actualy it should change depending on the type of the rocket but for now it is just a simple merge effect
    public class TntRocketMergeParticleEffect: MonoBehaviour
    {
    [SerializeField] private GameObject tnt;
    [SerializeField] private GameObject rocket;
    [SerializeField] private Vector3 tntInitialPosition = new Vector3(0.25f,0f,0);
    [SerializeField] private Vector3 rocketInitialPosition = new Vector3(-0.25f,0f,0);
    [SerializeField] private int loopCount = 1;
    private bool _isExploding;
    private Sequence _scaleTnt;
    private Sequence _scaleRocket;
    private float _duration; 
    public void InitializeEffect(float duration)
    {
        _duration = duration;
        ResetEffect();
        MoveBooster(tnt, rocketInitialPosition);
        MoveBooster(rocket, tntInitialPosition);
        _scaleTnt = ScaleBooster(tnt, Vector3.one * 0.75f, Vector3.one, Vector3.one * 1.25f, Vector3.one);
        _scaleRocket = ScaleBooster(rocket, Vector3.one * 1.25f, Vector3.one, Vector3.one * 0.75f, Vector3.one);
    }

    private void ResetEffect()
    {
        _isExploding = false;
        tnt.SetActive(true);
        rocket.SetActive(true);
    }

    private Sequence ScaleBooster(GameObject booster, Vector3 scale1, Vector3 scale2, Vector3 scale3, Vector3 scale4)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(ScaleAndSort(booster, scale1));
        sequence.Append(ScaleAndSort(booster, scale2));
        sequence.Append(ScaleAndSort(booster, scale3));
        sequence.Append(ScaleAndSort(booster, scale4));
        sequence.SetLoops(-1, LoopType.Restart);
        return sequence;
    }

    private void MoveBooster(GameObject booster, Vector3 destination)
    {
        booster.transform.DOLocalMove(destination, _duration)
            .SetUpdate(UpdateType.Fixed)
            .SetEase(Ease.InOutSine)
            .SetLoops(loopCount, LoopType.Yoyo).onComplete += () =>
        {
            _scaleTnt.Kill();
            _scaleRocket.Kill();
            booster.transform.DOLocalMove(booster.transform.localPosition*1.2f , _duration/2)
                .SetUpdate(UpdateType.Fixed).SetEase(Ease.InOutSine).onComplete += () =>
            {
                booster.transform.DOLocalMove(booster.transform.localPosition/5, _duration / 2)
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
        tnt.SetActive(false);
        rocket.SetActive(false);
    }

    private Tween ScaleAndSort(GameObject lightBall, Vector3 scale)
    {
        return lightBall.transform.DOScale(scale, _duration / 2) // Use duration/2 for scale duration
            .SetUpdate(UpdateType.Fixed)
            .SetEase(Ease.InOutSine)
            .OnUpdate(() => lightBall.GetComponent<SpriteRenderer>().sortingOrder = (int)(lightBall.transform.localScale.x * 15));
    }
    }
}