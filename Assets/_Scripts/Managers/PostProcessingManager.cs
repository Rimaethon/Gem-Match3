using System;
using Rimaethon.Scripts.Managers;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

//Bloom takes a lot of performance, so we need to close it when it's not needed.
public class PostProcessingManager : MonoBehaviour
{
    private int _bloomOpenCount = 0;
   private Volume volume;

    private Bloom bloom;

    private void Awake()
    {
        volume = GetComponent<Volume>();
        if (volume.profile.TryGet(out bloom) == false)
        {
            Debug.LogError("Bloom is not set in the Volume component.");
            return;
        }
        bloom.active = true;
    }
    private void OnEnable()
    {
        EventManager.Instance.AddHandler(GameEvents.OnCloseBloom, BloomClose);
        EventManager.Instance.AddHandler(GameEvents.OnOpenBloom, BloomOpen);
    }

    private void OnDisable()
    {
        if(EventManager.Instance==null)
            return;
        EventManager.Instance.RemoveHandler(GameEvents.OnCloseBloom, BloomClose);
        EventManager.Instance.RemoveHandler(GameEvents.OnOpenBloom, BloomOpen);
    }

    private void BloomOpen()
    {
        _bloomOpenCount++;
        bloom.active = true;
    }

    private void BloomClose()
    {
        _bloomOpenCount--;
        if (_bloomOpenCount <= 0)
        {
            bloom.active = false;
            _bloomOpenCount = 0;
        }
    }
}
