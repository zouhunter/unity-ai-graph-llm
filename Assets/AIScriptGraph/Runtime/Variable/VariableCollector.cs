/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 变量收集器
 *_*/

using System;
using UnityEngine;

namespace AIScripting
{
    public abstract class VariableCollector : ScriptableObject
    {
        public abstract void Collect(Func<string, Variable> getFunc, Action<string, Variable> setFunc);
    }
}

