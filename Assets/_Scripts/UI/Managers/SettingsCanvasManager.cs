using _Scripts.Data_Classes;
using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace Rimaethon.Runtime.UI
{
    public class SettingsCanvasManager : MonoBehaviour
    {
        [SerializeField] private Button musicToggle;
        [SerializeField] private RectTransform musicToggleImage;
        [SerializeField] private Button soundToggle;
        [SerializeField] private RectTransform soundToggleImage;
        [SerializeField] private Button hintToggle;
        [SerializeField] private RectTransform hintToggleImage;
        [SerializeField] private Button notificationsToggle;
        [SerializeField] private RectTransform notificationsToggleImage;
        [SerializeField] private TextMeshProUGUI versionText;
        private float _toggleOffPivotXMax = 0.6f;
        private float _toggleOffPivotXMin = 0.035f;
        private float _toggleOnPivotXMin = 0.36f;
        private float _toggleOnPivotXMax = 1f;
        bool _isMusicOn;
        bool _isSoundOn;
        bool _isHintOn;
        private bool _isNotificationOn;
        private void Start()
        {
            _isMusicOn = SaveManager.Instance.IsMusicOn();
            _isSoundOn = SaveManager.Instance.IsSfxOn();
            _isHintOn = SaveManager.Instance.IsHintOn();
            _isNotificationOn = SaveManager.Instance.IsNotificationOn();
            versionText.text = "Version "+SaveManager.Instance.GetVersion();
            MoveToggle(_isMusicOn, musicToggleImage);
            MoveToggle(_isSoundOn, soundToggleImage);
            MoveToggle(_isHintOn, hintToggleImage);
            MoveToggle(_isNotificationOn, notificationsToggleImage);
        }
        private void OnEnable()
        {
            musicToggle.onClick.AddListener(() =>
            {
               _isMusicOn= MoveToggle(!_isMusicOn, musicToggleImage);
               SaveManager.Instance.SetMusic(_isMusicOn);
               AudioManager.Instance.PlaySFX(SFXClips.UIButtonSound);
            });
            soundToggle.onClick.AddListener(() =>
            {
                _isSoundOn=MoveToggle(!_isSoundOn, soundToggleImage);
                SaveManager.Instance.SetSFX(_isSoundOn);
                AudioManager.Instance.PlaySFX(SFXClips.UIButtonSound);
            });
            hintToggle.onClick.AddListener(() =>
            {
                _isHintOn=MoveToggle(!_isHintOn, hintToggleImage);
                SaveManager.Instance.SetHint(_isHintOn);
                AudioManager.Instance.PlaySFX(SFXClips.UIButtonSound);
            });
            notificationsToggle.onClick.AddListener(() =>
            {
                _isNotificationOn=MoveToggle(!_isNotificationOn, notificationsToggleImage);
                SaveManager.Instance.SetNotification(_isNotificationOn);
                AudioManager.Instance.PlaySFX(SFXClips.UIButtonSound);
            });
        }
        private void OnDisable()
        {
            musicToggle.onClick.RemoveAllListeners();
            soundToggle.onClick.RemoveAllListeners();
            hintToggle.onClick.RemoveAllListeners();
            notificationsToggle.onClick.RemoveAllListeners();
        }
        private bool MoveToggle( bool toogleState, RectTransform toggleImage)
        {
            if (toogleState)
            {
                toggleImage.DOAnchorMin(new Vector2(_toggleOnPivotXMin, 0f), 0.1f).SetUpdate(UpdateType.Fixed);
                toggleImage.DOAnchorMax(new Vector2(_toggleOnPivotXMax, 1f), 0.1f).SetUpdate(UpdateType.Fixed);
            }
            else
            {
                toggleImage.DOAnchorMin(new Vector2(_toggleOffPivotXMin, 0f), 0.1f).SetUpdate(UpdateType.Fixed);
                toggleImage.DOAnchorMax(new Vector2(_toggleOffPivotXMax, 1f), 0.1f).SetUpdate(UpdateType.Fixed);
            }

            return toogleState;

        }
    }
}
