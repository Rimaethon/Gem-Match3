using System;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "CrackerParticleEffect", menuName = "Create CrackerParticleEffect")]

    public class CrackerParticleEffect:AbstractItemDataSO
    {
        public float FireworkRotation = 0;
        public float Speed = 1;

        

        private void Start()
        {
            ItemPrefab.transform.rotation = Quaternion.Euler(0, 0, FireworkRotation);

        }

    
    }
}