using System.Security;
using UnityEngine;

namespace Scripts
{
    public class LightBallParticleEffect:BoosterParticleEffect
    {
        public GameObject lightBallExplosionParticle;
        public GameObject lightBallSprite;
        private void OnDisable()
        {
            if(ObjectPool.Instance==null)
                return;
            ObjectPool.Instance.ReturnBoosterParticleEffect(gameObject, ItemID);
        }
    }
}