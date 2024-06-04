using UnityEngine;

namespace Scripts
{
    public class MatchItemParticleEffect:ItemParticleEffect
    {
        private void OnParticleSystemStopped()
        {
            if(ObjectPool.Instance==null)
                return;
            ObjectPool.Instance.ReturnBoosterCreationEffect(gameObject);
        }
        private void Awake()
        {
            var particleSystem = GetComponent<ParticleSystem>().main;
            particleSystem.stopAction = ParticleSystemStopAction.Callback;
        }
    }
}