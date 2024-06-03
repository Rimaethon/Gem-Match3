using _Scripts.Data_Classes;
using Rimaethon.Scripts.Managers;
using Scripts;
using UnityEngine;

namespace Rimaethon.Runtime.UI
{
    [RequireComponent(typeof(Canvas))]
    public class TopLayerCanvasManager:MonoBehaviour
    {
        [SerializeField] private GameObject noMovesLeftPanel;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private GameObject mainEventGoalExplodedEffect;
        private Canvas _boosterCanvas;
        private Vector2 _boosterCanvasRectTransformSizeDelta;
        private EventData _mainEvent;
        private bool _hasMainEvent;
        private static Camera MainCamera => Camera.main;
        private int _mainEventID;
        private void Awake()
        {
            _hasMainEvent = SaveManager.Instance.HasMainEvent();
            if (_hasMainEvent)
                _mainEvent = SaveManager.Instance.GetMainEventData();
            _boosterCanvas = GetComponent<Canvas>();
            _mainEventID = _mainEvent.eventObjectiveID;
            _boosterCanvasRectTransformSizeDelta= GetComponent<RectTransform>().sizeDelta;
        }
        private void OnEnable()
        {
            EventManager.Instance.AddHandler(GameEvents.OnNoMovesLeft, HandleNoMovesLeft);
            EventManager.Instance.AddHandler<Vector3,int,int>(GameEvents.OnMainEventGoalMatch, HandleItemMatched);
            EventManager.Instance.AddHandler<Vector2Int,int>(GameEvents.OnItemExplosion, HandleItemExplosion);
            EventManager.Instance.AddHandler(GameEvents.OnLevelCompleted, () =>
            {
                _boosterCanvas.enabled = true;
                levelCompletePanel.SetActive(true);
            });
        }
        private void OnDisable()
        {
            if (EventManager.Instance == null) return;
            EventManager.Instance.RemoveHandler(GameEvents.OnNoMovesLeft, HandleNoMovesLeft);
            EventManager.Instance.RemoveHandler<Vector3,int,int>(GameEvents.OnMainEventGoalMatch, HandleItemMatched);
            EventManager.Instance.RemoveHandler<Vector2Int,int>(GameEvents.OnItemExplosion, HandleItemExplosion);
        }
        private void HandleMainEventGoalRemoved(Vector3 pos,int count)
        {
            MainEventUIEffect effect = ObjectPool.Instance.GetMainEventUIEffect(_mainEventID);
           effect.gameObject.transform.SetParent( _boosterCanvas.transform);
           effect.rectTransform.localScale = Vector3.one;
            SetUIPosition(effect.rectTransform,pos);
           effect.textMeshProUGUI.text = "+"+ count;
           effect.Move();
           EventManager.Instance.Broadcast<int,int>(GameEvents.OnMainEventGoalRemoval, _mainEventID, count);
        }
   
        private void HandleItemMatched(Vector3 item, int itemID,int amount)
        {
            if (_hasMainEvent&& _mainEvent.eventObjectiveID == itemID)
            {
                HandleMainEventGoalRemoved(item, amount);
            }
        }

        private void HandleItemExplosion(Vector2Int itemPos, int itemID)
        {
            if (_hasMainEvent&& _mainEvent.eventObjectiveID == itemID)
            { 
                Vector3 item = LevelGrid.Instance.GetCellCenterWorld(itemPos);
                HandleMainEventGoalRemoved(item, 1);
            }
        }
        private void HandleNoMovesLeft()
        {
            _boosterCanvas.enabled = true;
            noMovesLeftPanel.SetActive(true);
        }
         
        private void SetUIPosition(RectTransform uiElement,Vector3 position)
        {

            Vector2 viewportPosition=MainCamera.WorldToViewportPoint(position);
            Vector2 worldObjectScreenPosition=new Vector2(
                ((viewportPosition.x*_boosterCanvasRectTransformSizeDelta.x)-(_boosterCanvasRectTransformSizeDelta.x*0.5f)),
                ((viewportPosition.y*_boosterCanvasRectTransformSizeDelta.y)-(_boosterCanvasRectTransformSizeDelta.y*0.5f)));
            uiElement.localPosition=worldObjectScreenPosition;
        }
     
    }
}