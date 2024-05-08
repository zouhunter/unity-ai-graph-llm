using System;
using System.Collections.Generic;

using UnityEngine;

namespace AIScripting
{
    public class EventProvider
    {
        #region Events
        private Dictionary<string, List<Action<object>>> _events = new Dictionary<string, List<Action<object>>>();
        public void ResetEventMap(Dictionary<string, List<Action<object>>> map)
        {
            this._events = map;
        }
        public void RegistEvent(string eventKey, Action<object> callback)
        {
            if (!_events.TryGetValue(eventKey, out var actions))
            {
                _events[eventKey] = new List<Action<object>>() { callback };
            }
            else
            {
                actions.Add(callback);
            }
        }
        public void RemoveEvent(string eventKey, Action<object> callback)
        {
            if (_events.TryGetValue(eventKey, out var actions))
            {
                actions.Remove(callback);
            }
        }
        public void SendEvent(string eventKey, object arg = null)
        {
            if (_events.TryGetValue(eventKey, out var actions))
            {
                for (int i = actions.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        actions[i]?.Invoke(arg);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }
        }
        #endregion

    }
}