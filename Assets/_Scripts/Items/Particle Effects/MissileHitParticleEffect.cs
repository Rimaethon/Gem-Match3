using UnityEngine;

namespace Scripts
{
    public class MissileHitParticleEffect:MonoBehaviour
    {
        private void OnDisable()
        {
            if(ObjectPool.Instance==null)
                return;
            ObjectPool.Instance.ReturnMissileHitEffect(gameObject);
        }
    }
}