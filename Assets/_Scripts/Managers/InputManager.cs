using System;
using Rimaethon.Scripts.Managers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Scripts
{
    public class InputManager: MonoBehaviour
    {
        private PlayerInputs _playerInput;
        private Camera _mainCamera;
        private Vector2 _clickPos;
        private Vector2 _dragPos;
        private bool _isDragging=true;

        
        private void Awake()
        {
            _playerInput = new PlayerInputs();
            _mainCamera = Camera.main;
        }
        private void OnEnable()
        {
            _playerInput.Enable();
            _playerInput.Player.Click.started +=OnClick;
            _playerInput.Player.Click.canceled += OnRelease;
            _playerInput.Player.Drag.performed+= OnDrag;
        }
        private void OnDisable()
        {
            _playerInput.Disable();
            _playerInput.Player.Click.started -=OnClick;
            _playerInput.Player.Drag.performed-= OnDrag;
        }
        
        private void OnClick(InputAction.CallbackContext context)
        {
            
            _clickPos = _mainCamera.ScreenToWorldPoint(_playerInput.Player.Drag.ReadValue<Vector2>());
            EventManager.Instance.Broadcast(GameEvents.OnTouch, _clickPos);
            _isDragging = true;
        }
       private void OnRelease(InputAction.CallbackContext context)
        {
            if (_isDragging)
            {
                EventManager.Instance.Broadcast(GameEvents.OnClick, _clickPos);
                _isDragging = false;
            }
        }
        
        private void OnDrag(InputAction.CallbackContext context)
        {
            if (!_isDragging) return;
            
            _dragPos = _mainCamera.ScreenToWorldPoint(_playerInput.Player.Drag.ReadValue<Vector2>());
            float xDiff = _dragPos.x- _clickPos.x  ;
            float yDiff =_dragPos.y-_clickPos.y ;
            if(Mathf.Abs(yDiff) > Mathf.Abs(xDiff) && Mathf.Abs(yDiff) > 0.2f)
            {
                if(yDiff > 0)
                {
                    EventManager.Instance.Broadcast(GameEvents.OnSwipe, new Vector2Int(0, 1),_clickPos); // Up
                }
                else
                {
                    EventManager.Instance.Broadcast(GameEvents.OnSwipe, new Vector2Int(0, -1),_clickPos); // Down
                }
                _isDragging = false;
            }
            else if(Mathf.Abs(xDiff) > 0.2f)
            {
                if(xDiff > 0)
                {
                    EventManager.Instance.Broadcast(GameEvents.OnSwipe, new Vector2Int(1, 0),_clickPos); // Right
                }
                else
                {
                    EventManager.Instance.Broadcast(GameEvents.OnSwipe, new Vector2Int(-1, 0),_clickPos); // Left
                }
                _isDragging = false;
            }
        }
  
   
            
        
    }
    
       
    }