using System.Threading;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Scripts
{
    public class InputManager: MonoBehaviour
    {
        [SerializeField] private GameObject _eventSystem;
        private PlayerInputs _playerInputs;
        private PlayerInput _playerInput;
        private Camera _mainCamera;
        private Vector2 _clickPos;
        private Vector2 _dragPos;
        private bool _isDragging=true;
        private bool _isBoardLocked;
        private int _booardLockCount; 
        private int _playerInputLockCount;
        private void Awake()
        {
            _playerInputs = new PlayerInputs();
            _playerInput = GetComponent<PlayerInput>();
            _mainCamera = Camera.main;
        }
        private void OnEnable()
        {
            _playerInputs.Enable();
            _playerInputs.Player.Click.started +=OnTouch;
            _playerInputs.Player.Click.canceled += OnRelease;
            _playerInputs.Player.Drag.performed+= OnDrag;

            EventManager.Instance.AddHandler(GameEvents.OnPlayerInputLock, OnPlayerInputLock);
            EventManager.Instance.AddHandler(GameEvents.OnPlayerInputUnlock, OnPlayerInputUnlock);
        }

        private void OnPlayerInputLock()
        {
         _playerInputLockCount++;
         _playerInputs.Disable();
         _playerInput.enabled = false;
         _eventSystem.SetActive(false);
         _isBoardLocked = true;
        }
        private void OnPlayerInputUnlock()
        {
            _playerInputLockCount--;
            if (_playerInputLockCount <= 0)
            {
                _playerInputs.Enable();
                _playerInput.enabled = true;
                _eventSystem.SetActive(true);
                _isBoardLocked = false;
            }
        }


        private void OnDisable()
        {
            _playerInputs.Disable();
            _playerInputs.Player.Click.started -=OnTouch;
            _playerInputs.Player.Drag.performed-= OnDrag;
            _playerInputs.Player.Click.canceled -= OnRelease;
            if (EventManager.Instance == null)
                return;

            EventManager.Instance.RemoveHandler(GameEvents.OnPlayerInputLock, OnPlayerInputLock);
            EventManager.Instance.RemoveHandler(GameEvents.OnPlayerInputUnlock, OnPlayerInputUnlock);
        }


 
        private void OnTouch(InputAction.CallbackContext context)
        {
            _clickPos = _mainCamera.ScreenToWorldPoint(_playerInputs.Player.Drag.ReadValue<Vector2>());
            EventManager.Instance.Broadcast(GameEvents.OnScreenTouch, _clickPos);
            if (_isBoardLocked)
                return;
            EventManager.Instance.Broadcast(GameEvents.OnTouch, _clickPos);
            _isDragging = true;
        }
       private void OnRelease(InputAction.CallbackContext context)
        {
            if (_isBoardLocked) return;

            if (_isDragging)
            {
                EventManager.Instance.Broadcast(GameEvents.OnClick, _clickPos);
                _isDragging = false;
            }
            else
            {
                EventManager.Instance.Broadcast(GameEvents.OnTouch, _clickPos);
            }
        }
        
        private void OnDrag(InputAction.CallbackContext context)
        {
            if (_isBoardLocked) return;
            if (!_isDragging) return;
            _dragPos = _mainCamera.ScreenToWorldPoint(_playerInputs.Player.Drag.ReadValue<Vector2>());
            float xDiff = _dragPos.x- _clickPos.x  ;
            float yDiff =_dragPos.y-_clickPos.y ;
            if(Mathf.Abs(yDiff) > Mathf.Abs(xDiff) && Mathf.Abs(yDiff) > 0.1f)
            {
                EventManager.Instance.Broadcast(GameEvents.OnSwipe,
                    yDiff > 0 ? new Vector2Int(0, 1) : new Vector2Int(0, -1), _clickPos);
                _isDragging = false;
            }
            else if(Mathf.Abs(xDiff) > 0.1f)
            {
                EventManager.Instance.Broadcast(GameEvents.OnSwipe,
                    xDiff > 0 ? new Vector2Int(1, 0) : new Vector2Int(-1, 0), _clickPos); 
                _isDragging = false;
            }
        }
            
        
    }
    
       
    }