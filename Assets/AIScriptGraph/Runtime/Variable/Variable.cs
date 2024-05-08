using System;
using System.Diagnostics;

namespace AIScripting
{
    public abstract class Variable
    {
        public event Action<string> onValueChanged;
        public string Name { get; set; }
        public abstract object GetValue();
        public abstract void SetValue(object value);
        protected void OnValueChanged()
        {
            onValueChanged?.Invoke(Name);
        }
    }


    [System.Serializable]
    public class Variable<T> : Variable
    {
        [UnityEngine.SerializeField]
        private T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != null)
                {
                    if (!_value.Equals(value))
                    {
                        _value = value;
                        OnValueChanged();
                    }
                }
                else
                {
                    if (value != null)
                    {
                        _value = value;
                        OnValueChanged();
                    }
                }
            }
        }
        public Variable(string name, T value)
        {
            Name = name;
            Value = value;
        }
        public Variable(string name)
        {
            Name = name;
        }
        public Variable() { }
        public override object GetValue()
        {
            return Value;
        }
        public override void SetValue(object value)
        {
            if (value is T)
                Value = (T)value;
            else if (value == null)
                Value = default(T);
        }

        public static explicit operator T(Variable<T> variable) { return variable == null ? variable.Value : default(T); }
    }
}
