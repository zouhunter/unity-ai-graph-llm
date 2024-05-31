using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

namespace AIScripting
{
    public class VariableProvider : IVariableProvider
    {
        private Dictionary<Type, IEnumerable<FieldInfo>> _fieldMap = new();

        #region Variables
        private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();

        public Variable GetVariable(string name)
        {
            _variables.TryGetValue(name, out var variable);
            return variable;
        }
        public Variable<T> GetVariable<T>(string name, bool createIfNotExits = true)
        {
            if (_variables.TryGetValue(name, out var variable) && variable is Variable<T> genVariable)
            {
                return genVariable;
            }
            else if (createIfNotExits)
            {
                var newVariable = new Variable<T>();
                _variables[name] = newVariable;
                return newVariable;
            }
            return null;
        }
        public T GetVariableValue<T>(string name)
        {
            if (_variables.TryGetValue(name, out var variable) && variable.GetValue() is T value)
            {
                return value;
            }
            return default(T);
        }
        public bool TryGetVariable<T>(string name, out Variable<T> variable)
        {
            if (_variables.TryGetValue(name, out var variableObj) && variableObj is Variable<T> genVariable && genVariable != null)
            {
                variable = genVariable;
                return true;
            }
            variable = null;
            return false;
        }
        public bool TryGetVariable(string name, out Variable variable)
        {
            return _variables.TryGetValue(name, out variable);
        }
        public void SetVariable(Variable variable)
        {
            _variables[variable.Name] = variable;
        }
        public bool SetVariableValue(string name, object data)
        {
            if (_variables.TryGetValue(name, out var variable))
            {
                variable.SetValue(data);
                return true;
            }
            return false;
        }
        public void SetVariableValue<T>(string name, T data)
        {
            var variable = GetVariable<T>(name, true);
            variable.Value = data;
        }
        #endregion Variables

        /// <summary>
        /// 反射获取所有的引用变量
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<FieldInfo> GetTypeRefs(Type type)
        {
            if (!_fieldMap.TryGetValue(type, out var fields))
            {
                fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).Where(f => typeof(IRef).IsAssignableFrom(f.FieldType));
                _fieldMap[type] = fields;
            }
            return fields;
        }

    }
}
