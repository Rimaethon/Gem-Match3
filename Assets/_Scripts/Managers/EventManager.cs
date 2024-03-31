using System;
using System.Collections.Generic;
using System.Linq;
using Rimaethon.Scripts.Utility;
using UnityEngine;

namespace Rimaethon.Scripts.Managers
{
    public class EventManager : PersistentSingleton<EventManager>
    {
        #region Fields And Properties
        private readonly Dictionary<GameEvents, List<Delegate>> _eventHandlers = new();
        [SerializeField] private bool showEventNames; 
        #endregion


        #region Unity Methods

        [SerializeField] private List<string> eventNames = new();

        private void Start()
        {
            if (!showEventNames) return;
            foreach (var events in _eventHandlers.Keys)
            {
                eventNames.Add(events.ToString());
                foreach (var value in _eventHandlers[events].ToList())
                {
                    Type type = value.Method.DeclaringType;
                    if (type == null) continue;
                    string className = type.Name;
                    eventNames.Add(className+ "." + value.Method.Name);
                }
               
            }

        }

        private void OnApplicationQuit()
        {
            _eventHandlers.Clear();
            Debug.LogWarning("Event Manager is cleared");
        }

        #endregion

        #region Event Handlers

        public void AddHandler(GameEvents gameEvent, Action handler)
        {
            if (!_eventHandlers.ContainsKey(gameEvent)) _eventHandlers[gameEvent] = new List<Delegate>();

            _eventHandlers[gameEvent].Add(handler);
            Debug.Log($"Added handler {handler.Method.Name} for game event {gameEvent}");
        }

        public void AddHandler<T>(GameEvents gameEvent, Action<T> handler)
        {
            if (!_eventHandlers.ContainsKey(gameEvent)) _eventHandlers[gameEvent] = new List<Delegate>();

            _eventHandlers[gameEvent].Add(handler);
            Debug.Log($"Added handler {handler.Method.Name} for game event {gameEvent}");
        }
        public void AddHandler<T,T1>(GameEvents gameEvent, Action<T,T1> handler)
        {
            if (!_eventHandlers.ContainsKey(gameEvent)) _eventHandlers[gameEvent] = new List<Delegate>();

            _eventHandlers[gameEvent].Add(handler);
            Debug.Log($"Added handler {handler.Method.Name} for game event {gameEvent}");
        }

        public void RemoveHandler(GameEvents gameEvent, Action handler)
        {
            if (_eventHandlers.TryGetValue(gameEvent, out var handlers))
            {
                handlers.Remove(handler);
                Debug.Log($"Removed handler {handler.Method.Name} for game event {gameEvent}");

                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(gameEvent);
                    Debug.Log($"No more handlers for game event {gameEvent}");
                }
            }
        }

        public void RemoveHandler<T>(GameEvents gameEvent, Action<T> handler)
        {
            if (_eventHandlers.TryGetValue(gameEvent, out var handlers))
            {
                handlers.Remove(handler);
                Debug.Log($"Removed handler {handler.Method.Name} for game event {gameEvent}");

                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(gameEvent);
                    Debug.Log($"No more handlers for game event {gameEvent}");
                }
            }
        }
        public void RemoveHandler<T,T1>(GameEvents gameEvent, Action<T,T1> handler)
        {
            if (_eventHandlers.TryGetValue(gameEvent, out var handlers))
            {
                handlers.Remove(handler);
                Debug.Log($"Removed handler {handler.Method.Name} for game event {gameEvent}");

                if (handlers.Count == 0)
                {
                    _eventHandlers.Remove(gameEvent);
                    Debug.Log($"No more handlers for game event {gameEvent}");
                }
            }
        }
        #endregion

        #region Event Broadcasting

        public void Broadcast(GameEvents gameEvents)
        {
            ProcessEvent(gameEvents);
        }

        public void Broadcast<T>(GameEvents gameEvent, T arg)
        {
            ProcessEvent(gameEvent, arg);
        }

        public void Broadcast<T,T1>(GameEvents gameEvent, T arg, T1 arg1)
        {
            ProcessEvent(gameEvent, arg, arg1);
        }
    
        private void ProcessEvent(GameEvents gameEvents, params object[] args)
        {
            if (_eventHandlers.TryGetValue(gameEvents, out var eventHandler))
                foreach (var handler in eventHandler)
                {
                    handler.DynamicInvoke(args);
                    //Debug.Log($"Broadcasted event {gameEvents} with arguments {string.Join(", ", args.Select(arg => arg.ToString()))} to handler {handler.Method.Name}");
                }
        }

        #endregion
    }
}