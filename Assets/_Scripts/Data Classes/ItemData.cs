using Scripts;
using UnityEngine;

namespace _Scripts.Data_Classes
{
    public class ItemData
    {
        public Sprite ItemSprite;
        public int ItemHealth=1;
        public GameObject ItemPrefab;
        public GameObject ItemParticleEffect;
        public IItemAction ItemAction;
        
    }
}