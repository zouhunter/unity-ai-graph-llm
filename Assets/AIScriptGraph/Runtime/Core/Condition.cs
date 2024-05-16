using System;
using System.Collections;
using System.Collections.Generic;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

using UnityEngine;

namespace AIScripting
{
    [System.Serializable]
    public class Condition
    {
        public enum CompareType
        {
            Equal,
            NotEqual,
            Greater,
            Less,
            GreaterEqual,
            LessEqual,
        }

        public enum ValueType
        {
            Int,
            Float,
            String,
            Bool,
            Enum,
            Object,
        }

        public string param;
        public ValueType type;
        public CompareType compareType;
        public string compareValue;

        /// <summary>
        /// 检查条件是否满足
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public bool Check(IVariableProvider provider)
        {
            Variable vari = provider.GetVariable(param);
            if (vari == null)
            {
                return false;
            }
            var variable = vari.GetValue();
            switch (type)
            {
                case ValueType.Int:
                    return CompareInt(variable, compareValue);
                case ValueType.Float:
                    return CompareFloat(variable, compareValue);
                case ValueType.String:
                    return CompareString(variable, compareValue);
                case ValueType.Bool:
                    return CompareBool(variable, compareValue);
                // Omitting Enum and Object because their comparison would require more context.
                default:
                    return CompareString(variable.ToString(), compareValue);
            }
        }

        private bool CompareInt(object variable, string compareValue)
        {
            if (!int.TryParse(compareValue, out int compareIntValue))
                return false; // or throw an exception

            int variableIntValue = Convert.ToInt32(variable); // assuming variable is convertible to int

            return CompareValues(variableIntValue, compareIntValue);
        }

        private bool CompareFloat(object variable, string compareValue)
        {
            if (!float.TryParse(compareValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float compareFloatValue))
                return false; // or throw an exception

            float variableFloatValue = Convert.ToSingle(variable); // assuming variable is convertible to float

            return CompareValues(variableFloatValue, compareFloatValue);
        }

        private bool CompareString(object variable, string compareValue)
        {
            string variableStringValue = variable.ToString(); // assuming variable is convertible to string

            return CompareValues(variableStringValue, compareValue);
        }

        private bool CompareBool(object variable, string compareValue)
        {
            if (!bool.TryParse(compareValue, out bool compareBoolValue))
                return false; // or throw an exception

            bool variableBoolValue = Convert.ToBoolean(variable); // assuming variable is convertible to bool

            return CompareValues(variableBoolValue, compareBoolValue);
        }

        // Generic method for comparison that works with any IComparable type
        private bool CompareValues<T>(T variableValue, T compareValue) where T : IComparable<T>
        {
            switch (compareType)
            {
                case CompareType.Equal:
                    return variableValue.CompareTo(compareValue) == 0;
                case CompareType.NotEqual:
                    return variableValue.CompareTo(compareValue) != 0;
                case CompareType.Greater:
                    return variableValue.CompareTo(compareValue) > 0;
                case CompareType.Less:
                    return variableValue.CompareTo(compareValue) < 0;
                case CompareType.GreaterEqual:
                    return variableValue.CompareTo(compareValue) >= 0;
                case CompareType.LessEqual:
                    return variableValue.CompareTo(compareValue) <= 0;
                default:
                    throw new InvalidOperationException("Unknown comparison type");
            }
        }
    }

}
