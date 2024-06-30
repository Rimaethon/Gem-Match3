using Rimaethon.Scripts.Managers;
using UnityEngine;

namespace Scripts
{
    public class DarkGrassUnderlayBoardItem:UnderlayBoardItem
    {
        [SerializeField] private Sprite _lightGrassSprite;
        private bool isDarkGrassExploded;
        public override void OnExplode()
        {
            if(IsExploding)
                return;
            _isExploding= true;
            if (!isDarkGrassExploded)
            {
                EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,_position,_itemID,0);
                _spriteRenderer.sprite = _lightGrassSprite;
                isDarkGrassExploded = true;
                _isExploding = false;
                AudioManager.Instance.PlaySFX(SFXClips.BushSound);
            }
            else
            {
                int random = Random.Range(0, 1);

                AudioManager.Instance.PlaySFX(random == 1 ? SFXClips.BushSound : SFXClips.GrassSound);
                EventManager.Instance.Broadcast(GameEvents.AddActionToHandle,_position,_itemID-1,0);
                OnRemove();
            }
        }

    }
}
