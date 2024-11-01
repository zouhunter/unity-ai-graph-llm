/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-12
 * Version: 1.0.0
 * Description: 行为树行为脚本
 *_*/

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace MateAI.ScriptableBehaviourTree
{
    public class BTBehaivour : MonoBehaviour
    {
        [SerializeField]
        protected BTree _bt;
        [SerializeField]
        protected bool _continueRunning;
        [SerializeField]
        protected bool _autoStartOnEnable;
        [SerializeField]
        protected float _interval = 0.1f;
        protected float _intervalTimer;
        [SerializeField]
        protected BTree _btInstance;
        [SerializeField]
        protected List<BindingInfo> _bindings;
        public UnityEvent<BTree> onCreateBTreeEvent;
        protected bool _isRunning;

        protected virtual void OnEnable()
        {
            if (!_btInstance)
            {
                _btInstance = _bt.CreateInstance();
                onCreateBTreeEvent?.Invoke(_btInstance);
            }
            if (_autoStartOnEnable)
            {
                foreach (var binding in _bindings)
                {
                    _btInstance.SetVariable(binding.name, new Variable<UnityEngine.Object>() { Value = binding.target });
                }
                _isRunning = _btInstance.StartUp();
            }
        }

        protected virtual void OnDisable()
        {
            if (_autoStartOnEnable)
            {
                _isRunning = false;
                _btInstance.Stop();
            }
        }

        protected virtual void Update()
        {
            if (!_isRunning)
                return;

            if (_interval > 0 && _intervalTimer < _interval)
            {
                _intervalTimer += Time.deltaTime;
                return;
            }
            _intervalTimer = 0;

            var result = _btInstance.Tick();
            if (result == Status.Success || result == Status.Failure)
            {
                _isRunning = _continueRunning;
            }
        }
    }
}

