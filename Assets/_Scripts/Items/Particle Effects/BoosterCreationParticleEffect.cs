using UnityEngine;

namespace Scripts
{
    public class BoosterCreationParticleEffect:MonoBehaviour
    {

        private void OnParticleSystemStopped()
        {
            gameObject.transform.SetParent(null);
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
