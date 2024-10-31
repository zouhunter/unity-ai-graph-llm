using System.Collections.Generic;
using System;

using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    public class VariableCenter
    {
        protected Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();
        protected HashSet<string> _persistentVariables = new HashSet<string>();
        protected Func<string, Variable> _outVariables;
        public IEnumerator<KeyValuePair<string, Variable>> GetGetEnumerator() => _variables.GetEnumerator();
        public virtual void BindingExtraVariable(Func<string, Variable> variableGetter)
        {
            _outVariables = variableGetter;
        }
        /// <summary>
        /// 持久变量
        /// </summary>
        /// <param name="variableName"></param>
        public virtual void SetPersistentVariable(string variableName)
        {
            _persistentVariables.Add(variableName);
        }
        public virtual Variable GetVariable(string name)
        {
            _variables.TryGetValue(name, out var variable);
            if (variable == null)
                variable = _outVariables?.Invoke(name);
            return variable;
        }

        public virtual Variable<T> GetVariable<T>(string name)
        {
            return GetVariable<T>(name, false);
        }

        public virtual Variable<T> GetVariable<T>(string name, bool createIfNotExits)
        {
            if (!_variables.TryGetValue(name, out var variable) || variable == null)
            {
                variable = _outVariables?.Invoke(name);
            }
            if (variable != null)
            {
                if (variable is Variable<T> genVariable)
                    return genVariable;
                else
                {
                    Debug.LogError("variable type miss match:" + name + "," + typeof(T));
                }
            }
            else if (createIfNotExits)
            {
                var newVariable = new Variable<T>();
                _variables[name] = newVariable;
                if (newVariable.Value == null && (typeof(T).IsArray || typeof(T).IsGenericType))
                {
                    newVariable.SetValue(Activator.CreateInstance<T>());
                }
                return newVariable;
            }
            return null;
        }

        public virtual T GetVariableValue<T>(string name)
        {
            if (!_variables.TryGetValue(name, out var variable))
            {
                variable = _outVariables?.Invoke(name);
            }
            if (variable != null && variable.GetValue() is T value)
            {
                return value;
            }
            return default(T);
        }

        public virtual bool TryGetVariable<T>(string name, out Variable<T> variable)
        {
            if (!_variables.TryGetValue(name, out var variableObj))
            {
                variableObj = _outVariables?.Invoke(name);
            }
            if (variableObj != null && variableObj is Variable<T> genVariable && genVariable != null)
            {
                variable = genVariable;
                return true;
            }
            variable = null;
            return false;
        }

        public virtual bool TryGetVariable(string name, out Variable variable)
        {
            if (!_variables.TryGetValue(name, out variable))
            {
                variable = _outVariables?.Invoke(name); ;
            }
            return variable != null;
        }

        public virtual void SetVariable(string name, Variable variable)
        {
            _variables[name] = variable;
        }
        public virtual bool SetVariableValue(string name, object data)
        {
            if (TryGetVariable(name, out var variable))
            {
                variable.SetValue(data);
                return true;
            }
            return false;
        }
        public virtual void SetVariableValue<T>(string name, T data)
        {
            if (TryGetVariable<T>(name, out var variable) && variable != null)
            {
                variable.SetValue(data);
            }
            else
            {
                GetVariable<T>(name, true).SetValue(data);
            }
        }

        /// <summary>
        /// 清理上下文
        /// </summary>
        /// <param name="includePersistent"></param>
        public virtual void ClearCondition(bool includePersistent = true)
        {
            if (includePersistent)
            {
                _variables.Clear();
            }
            else
            {
                var keys = new List<string>(_variables.Keys);
                foreach (var key in keys)
                {
                    if (!_persistentVariables.Contains(key))
                    {
                        _variables.Remove(key);
                    }
                }
            }
        }
    }
}
