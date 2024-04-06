using System.Threading;
using Cysharp.Threading.Tasks;
using Rimaethon.Scripts.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts
{
    public class InputManager: MonoBehaviour
    {
        private PlayerInputs _playerInput;
        private Camera _mainCamera;
        private Vector2 _clickPos;
        private Vector2 _dragPos;
        private bool _isDragging=true;
        private bool _isBoardLocked;
        private int _booardLockCount; 
        private void Awake()
        {
            _playerInput = new PlayerInputs();
            _mainCamera = Camera.main;
        }
        private void OnEnable()
        {
            _playerInput.Enable();
            _playerInput.Player.Click.started +=OnTouch;
            _playerInput.Player.Click.canceled += OnRelease;
            _playerInput.Player.Drag.performed+= OnDrag;
            EventManager.Instance.AddHandler(GameEvents.OnBoardLock, OnBoardLock);
            EventManager.Instance.AddHandler(GameEvents.OnBoardUnlock, OnBoardUnlock);
        }

      
        private void OnDisable()
        {
            _playerInput.Disable();
            _playerInput.Player.Click.started -=OnTouch;
            _playerInput.Player.Drag.performed-= OnDrag;
            _playerInput.Player.Click.canceled -= OnRelease;
            if (EventManager.Instance == null)
                return;
            EventManager.Instance.RemoveHandler(GameEvents.OnBoardLock, OnBoardLock);
            EventManager.Instance.RemoveHandler(GameEvents.OnBoardUnlock, OnBoardUnlock);
        }

        private void OnBoardLock()
        {
            _booardLockCount++;
            _isBoardLocked = true;
        }

        private void OnBoardUnlock()
        {
            _booardLockCount--;
            if (_booardLockCount == 0)
            {
                _isBoardLocked = false;
            }
        }
 
        private void OnTouch(InputAction.CallbackContext context)
        {
            if (_isBoardLocked) return;
            _clickPos = _mainCamera.ScreenToWorldPoint(_playerInput.Player.Drag.ReadValue<Vector2>());
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
            _dragPos = _mainCamera.ScreenToWorldPoint(_playerInput.Player.Drag.ReadValue<Vector2>());
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