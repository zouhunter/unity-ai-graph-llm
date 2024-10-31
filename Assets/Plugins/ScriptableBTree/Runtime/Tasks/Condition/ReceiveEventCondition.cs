/*-*-* Copyright (c) webxr@wekoi
 * Author: zouhunter
 * Creation Date: 2024-03-18
 * Version: 1.0.0
 * Description: 事件接收条件
 *_*/

namespace MateAI.ScriptableBehaviourTree.Condition
{
    [NodePath("事件接收条件")]
    public class ReceiveEventCondition : ConditionNode
    {
        public Ref<string> eventName;
        public Ref<object> eventArgReceive;

        private bool _registed;
        private bool _triggger;

        //protected override void OnReset()
        //{
        //    base.OnReset();
        //    if (!_registed)
        //    {
        //        _registed = true;
        //        Owner.RegistEvent(eventName.Value, OnEventTrigger);
        //    }
        //}

        //public override void Dispose()
        //{
        //    base.Dispose();
        //    if (_registed)
        //    {
        //        _registed = false;
        //        Owner?.RemoveEvent(eventName.Value, OnEventTrigger);
        //    }
        //}

        private void OnEventTrigger(object obj)
        {
            _triggger = true;
            eventArgReceive.Value = obj;
        }

        protected override bool CheckCondition()
        {
            if (_triggger)
            {
                _triggger = false;
                return true;
            }
            return false;
        }
    }
}
