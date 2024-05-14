using System;
using UnityEngine;

namespace Scripts
{
    public class BoosterParticleEffect:ItemParticleEffect
    {
 
        public override void OnDisable()
        {
            if(ObjectPool.Instance==null)
                return;
            ObjectPool.Instance.ReturnBoosterParticleEffect( gameObject, ItemID);
        }
    }
}