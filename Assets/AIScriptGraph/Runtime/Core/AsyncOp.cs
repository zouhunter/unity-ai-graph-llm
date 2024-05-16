using System;

namespace AIScripting
{
    public class AsyncOp
    {
        public float progress { get; protected set; }
        public string Id { get; internal set; }

        private Action<string> _onFinishNode;
        private Action<string> _onProgressNode;

        public AsyncOp()
        {
            progress = 0;
        }

        public void RegistComplete(Action<string> onFinishNode)
        {
            _onFinishNode = onFinishNode;
            if(progress == 1)
                _onFinishNode?.Invoke(Id);
        }

        public void RegistProgress(Action<string> onProgress)
        {
            _onProgressNode = onProgress;
            if(progress != 0)
                _onProgressNode?.Invoke(Id);
        }

        public void SetProgress(float progress)
        {
            this.progress = progress;
        }

        public void SetFinish()
        {
            progress = 1;
            _onFinishNode?.Invoke(Id);
        }
    }
}