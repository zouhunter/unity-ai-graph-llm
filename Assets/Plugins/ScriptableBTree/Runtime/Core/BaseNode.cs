/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 节点基类
 *_*/
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MateAI.ScriptableBehaviourTree
{
    public abstract class BaseNode
    {
        private BTree _owner;
        [HideInInspector]
        public string name;
        public BTree Owner => _owner;
        public virtual int Priority => 0;

        public static implicit operator bool(BaseNode instance) => instance != null;

        public virtual void SetOwner(BTree owner)
        {
            if (_owner != owner)
            {
                _owner = owner;
                BindingRefVars(GetRefVars());
            }
            OnReset();
        }

        protected virtual IEnumerable<IRef> GetRefVars()
        {
            return Owner.GetTypeRefs(GetType())
                .Select(f => f.GetValue(this) as IRef)
                .Where(r => r != null);
        }

        public Status Execute(TreeInfo info)
        {
            if (info == null)
            {
                info.status = Status.Inactive;
                Debug.LogError(info.node.name + ",TreeInfo == null");
                return info.status;
            }
            if (!info.enable)
                return Status.Inactive;

            info.tickCount = Owner.TickCount;
            if (info.node == this && info.condition != null && info.condition.enable)
            {
                if (!Owner.CheckConditions(info))
                {
                    info.status = Status.Failure;
#if UNITY_EDITOR
                    if (Owner.LogInfo)
                        Debug.Log("condition failed:" + info.node.name);
#endif
                    return info.status;
                }
                else
                {
#if UNITY_EDITOR
                    if (Owner.LogInfo)
                        Debug.Log("condition success:" + info.node.name);
#endif
                }
            }

            if (info.status != Status.Running)
            {
                OnStart(info);
            }

            info.status = OnUpdate(info);

            if (info.status != Status.Running)
            {
                OnEnd(info);
            }
            return info.status;
        }
        public void Clean()
        {
            OnClear();
        }
        protected void BindingRefVars(IEnumerable<IRef> refVars)
        {
            if (refVars != null)
            {
                foreach (var refVar in refVars)
                {
                    refVar?.Binding(Owner);
                }
            }
        }
        protected virtual void OnStart() { }
        protected virtual void OnReset() { }
        protected virtual void OnEnd() { }
        protected virtual void OnStart(TreeInfo info)
        {
            info.subIndex = 0;
            OnStart();
        }
        protected virtual void OnEnd(TreeInfo info)
        {
            OnEnd();
        }
        protected virtual void OnClear() { }
        protected virtual Status OnUpdate(TreeInfo info)
        {
            return OnUpdate();
        }
        protected virtual Status OnUpdate() => Status.Inactive;
    }
}
