using UnityEngine;

namespace Scripts
{
    public class MissileExplosionParticleEffect:MonoBehaviour
    {
        private void OnDisable()
        {
            if(ObjectPool.Instance==null)
                return;
            ObjectPool.Instance.ReturnMissileExplosionEffect(gameObject);
        }
    }
}