using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIScripting
{
    public class AsyncOp
    {
        public int progress { get; protected set; }

        private Action<ScriptAINodeBase> _onFinishNode;
        private Action<ScriptAINodeBase> _onProgressNode;
        private ScriptAINodeBase _node;

        public AsyncOp(ScriptAINodeBase target)
        {
            progress = 0;
            _node = target;
        }

        internal void RegistComplete(Action<ScriptAINodeBase> onFinishNode)
        {
            _onFinishNode = onFinishNode;
            if(progress == 100)
                _onFinishNode?.Invoke(_node);
        }

        internal void RegistProgress(Action<ScriptAINodeBase> onProgress)
        {
            _onProgressNode = onProgress;
            if(progress != 0)
                _onProgressNode?.Invoke(_node);
        }

        public void SetProgress(int progress)
        {
            this.progress = progress;
        }

        public void SetFinish()
        {
            progress = 100;
            _onFinishNode?.Invoke(null);
        }
    }
}