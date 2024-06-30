using System.Collections.Generic;
using System.IO;
using _Scripts.Data_Classes;
using _Scripts.UI.Panels;
using Cysharp.Threading.Tasks;
using Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rimaethon.Runtime.UI
{
    public class MainEventPanel:ProgressBarPanel
    {
        [SerializeField] private MainEventGoalObtainedPanel mainEventGoalObtainedPanel;
        private Dictionary<int, int> collectedItems = new Dictionary<int, int>();

        protected override void Awake()
        {
            _eventFolderPath = "Assets/Data/Events/";
            _eventRewardJsonName = "MainEventRewardData";
            _eventJsonName = "MainEvent";
            if (SaveManager.Instance.HasMainEvent())
            {
                _eventData = SaveManager.Instance.GetMainEventData();
                eventGoalIcon.sprite = itemDatabase.GetItemSprite(_eventData.eventGoalID);
                base.Awake();
            }

        }

        public async UniTask HandleMainEventGoalObtained()
        {
            collectedItems=SceneController.Instance.CollectedItems;
            foreach (KeyValuePair<int, int> item in collectedItems)
            {
                await  mainEventGoalObtainedPanel.StartGoalObtainedAnimation(item.Value, itemDatabase.GetItemSprite(item.Key),eventGoalIcon.transform.position);
                await HandleProgressBar(item.Value);
            }
        }

        [Button]
        public async UniTask MockMainEventGoalObtained(int value,int itemID)
        {
            await  mainEventGoalObtainedPanel.StartGoalObtainedAnimation(value, itemDatabase.GetItemSprite(itemID),eventGoalIcon.transform.position);
            await HandleProgressBar(value);
        }

    }
}
