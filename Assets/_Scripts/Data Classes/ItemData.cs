using Scripts;
using UnityEngine;

namespace _Scripts.Data_Classes
{
    public class ItemData
    {
        public Sprite ItemSprite;
        public GameObject ItemPrefab;
        public GameObject ItemParticleEffect;
        public GameObject UITrailEffect;
        public IItemAction ItemAction;
        
    }
}