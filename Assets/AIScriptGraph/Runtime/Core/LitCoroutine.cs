using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AIScripting
{
    public class LitCoroutine
    {
        private struct YieldProcessor
        {
            enum DataType : byte
            {
                None = 0,
                WaitForSeconds = 1,
                LitCoroutine = 2,
                AsyncOP = 3,
            }
            struct ProcessorData
            {
                public DataType type;
                public double targetTime;
                public object current;
            }

            ProcessorData data;

            public void Set(object yield)
            {
                if (yield == data.current)
                    return;

                var type = yield.GetType();
                var dataType = DataType.None;
                double targetTime = -1;

                if (type == typeof(WaitForSeconds))
                {
                    targetTime = TimeOnStartUp() + (yield as WaitForSeconds).seconds;
                    dataType = DataType.WaitForSeconds;
                }
                else if (type == typeof(LitCoroutine))
                {
                    dataType = DataType.LitCoroutine;
                }
                else if (type == typeof(AsyncOperation) || type.IsSubclassOf(typeof(AsyncOperation)))
                {
                    dataType = DataType.AsyncOP;
                }

                data = new ProcessorData { current = yield, targetTime = targetTime, type = dataType };
            }

            public bool MoveNext(IEnumerator enumerator)
            {
                bool advance = false;
                switch (data.type)
                {
                    case DataType.WaitForSeconds:
                        advance = data.targetTime <= TimeOnStartUp();
                        break;
                    case DataType.LitCoroutine:
                        advance = (data.current as LitCoroutine)._isDone;
                        break;
                    case DataType.AsyncOP:
                        advance = (data.current as AsyncOperation).isDone;
                        break;
                    default:
                        advance = data.current == enumerator.Current; //a IEnumerator or a plain object was passed to the implementation
                        break;
                }

                if (advance)
                {
                    data = default(ProcessorData);
                    return enumerator.MoveNext();
                }
                return true;
            }

            public double TimeOnStartUp()
            {
#if UNITY_EDITOR
                if(Application.isEditor && !Application.isPlaying)
                {
                    return UnityEditor.EditorApplication.timeSinceStartup;
                }
#endif
                return Time.realtimeSinceStartup;
            }
        }

        WeakReference _owner;
        IEnumerator _routine;
        Stack<IEnumerator> _processingStack = new Stack<IEnumerator>(32);
        YieldProcessor _processor;
        bool _isDone;
        public bool IsDone => _isDone;

        public LitCoroutine(IEnumerator routine)
        {
            _owner = null;
            _routine = routine;
        }

        public LitCoroutine(IEnumerator routine, object owner)
        {
            _processor = new YieldProcessor();
            _owner = new WeakReference(owner);
            _routine = routine;
        }


        public void Update()
        {
            if (!IsDone)
                MoveNext();
        }

        public void MoveNext()
        {
            if (_owner != null && !_owner.IsAlive)
            {
                return;
            }

            bool done = ProcessIEnumeratorRecursive(_routine);
            _isDone = !done;
        }

        private bool ProcessIEnumeratorRecursive(IEnumerator enumerator)
        {
            var root = enumerator;
            while (enumerator.Current as IEnumerator != null)
            {
                _processingStack.Push(enumerator);
                enumerator = enumerator.Current as IEnumerator;
            }

            //process leaf
            _processor.Set(enumerator.Current);
            var result = _processor.MoveNext(enumerator);

            while (_processingStack.Count > 1)
            {
                if (!result)
                {
                    result = _processingStack.Pop().MoveNext();
                }
                else
                    _processingStack.Clear();
            }

            if (_processingStack.Count > 0 && !result && root == _processingStack.Pop())
            {
                result = root.MoveNext();
            }

            return result;
        }

        public void Stop()
        {
            _owner = null;
            _routine = null;
        }
    }

    public sealed class WaitForSeconds : YieldInstruction
    {
        public float seconds;
        public WaitForSeconds(float seconds)
        {
            this.seconds = seconds;
        }
    }
}
