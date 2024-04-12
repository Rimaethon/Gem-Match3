using System;
using UnityEngine;

namespace Scripts
{
    public class ItemParticleEffect:MonoBehaviour
    {
        public int ItemID;

        private void OnDisable()
        {
            if(ObjectPool.Instance==null)
                return;
            Debug.Log("Returning Particle Effect"+gameObject.name);
            ObjectPool.Instance.ReturnParticleEffect(gameObject, ItemID);
        }
    }
}