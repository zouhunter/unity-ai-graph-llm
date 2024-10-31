using System;
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

        private bool _started;

        protected bool _conditionFaliure;

        public static implicit operator bool(BaseNode instance) => instance != null;

        public virtual void SetOwner(BTree owner)
        {
            _owner = owner;
            BindingRefVars(GetRefVars());
            OnReset();
        }

        protected virtual IEnumerable<IRef> GetRefVars()
        {
            return Owner.GetTypeRefs(GetType())
                .Select(f => f.GetValue(this) as IRef)
                .Where(r => r != null);
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

        protected virtual void OnStart()
        {
        }
        protected virtual void OnReset()
        {
        }
        protected virtual void OnEnd()
        {
        }

        protected virtual void OnStart(TreeInfo info)
        {
            OnStart();
        }
      
        protected virtual void OnEnd(TreeInfo info)
        {
            OnEnd();
        }

        protected virtual void OnClear()
        {

        }

        protected virtual Status OnUpdate(TreeInfo info)
        {
            return OnUpdate();
        }

        protected virtual Status OnUpdate()
        {
            return Status.Inactive;
        }

        public virtual Status Execute(TreeInfo info)
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
            _conditionFaliure = false;
            if (info.node == this && info.condition != null && info.condition.enable)
            {
                if (!Owner.CheckConditions(info))
                {
                    info.status = Status.Failure;
#if UNITY_EDITOR
                    if(Owner.LogInfo)
                        Debug.Log("condition failed:" + info.node.name);
#endif
                    _conditionFaliure = true;
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

            if (!_started)
            {
                _started = true;
                OnStart(info);
            }

            info.status = OnUpdate(info);

            if (info.status != Status.Running)
            {
                _started = false;
                OnEnd(info);
            }
            return info.status;
        }

        public void Clean()
        {
            _started = false;
            OnClear();
        }
    }
}
