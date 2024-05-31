/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-28
 * Version: 1.0.0
 * Description: 引用类型变量
 *_*/

using System;
using UnityEngine;

namespace AIScripting
{
    public interface IRef
    {
        bool Exists { get; }
        void Binding(IVariableProvider provider);
    }

    [Serializable]
    public class Ref<T> : IRef
    {
        [SerializeField]
        private string _key;
        [SerializeField]
        private T _default;
        [SerializeField]
        private bool _autoCreate = true;
        private T _value;

        private IVariableProvider _variablePrivider;
        private Variable<T> _variable;
        public bool Exists
        {
            get
            {
                if (_variable != null)
                    return true;

                else if (_variablePrivider != null && !string.IsNullOrEmpty(_key))
                {
                    this._variable = _variablePrivider.GetVariable<T>(_key, false);
                }
                return false;
            }
        }

        public Ref() { }

        public Ref(string key, T value = default)
        {
            this._key = key;
            this._default = value;
        }
        public Ref(string key, IVariableProvider provider)
        {
            this._key = key;
            Binding(provider);
        }

        public void Binding(IVariableProvider provider)
        {
            this._variablePrivider = provider;
            if (!string.IsNullOrEmpty(_key))
            {
                _variable = _variablePrivider.GetVariable<T>(_key, false);
                if (_variable == null && _autoCreate)
                {
                    if (_default == null)
                        _default = Activator.CreateInstance<T>();
                    _variable = new Variable<T>(_key,_default);
                    _variablePrivider.SetVariable(_variable);
                }
            }
            else
            {
                _variable = null;
            }
            if (_variable == null)
                _value = _default;
        }

        public void SetValue(T value)
        {
            if (Exists)
                _variable.Value = value;
            else
                _value = value;
        }

        public T Value
        {
            get
            {
                if (Exists)
                    return _variable.Value;
                return _value;
            }
        }

        public static implicit operator T(Ref<T> r)
        {
            return r.Value;
        }
        public override string ToString()
        {
            return Value?.ToString()??base.ToString();
        }
    }
}
