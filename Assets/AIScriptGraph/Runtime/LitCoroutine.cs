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
                        advance = (data.current as LitCoroutine).m_IsDone;
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

        public WeakReference m_Owner;
        IEnumerator m_Routine;
        YieldProcessor m_Processor;
        bool m_IsDone;
        public bool IsDone => m_IsDone;

        public LitCoroutine(IEnumerator routine)
        {
            m_Owner = null;
            m_Routine = routine;
        }

        public LitCoroutine(IEnumerator routine, object owner)
        {
            m_Processor = new YieldProcessor();
            m_Owner = new WeakReference(owner);
            m_Routine = routine;
        }


        public void Update()
        {
            if (!IsDone)
                MoveNext();
        }

        public void MoveNext()
        {
            if (m_Owner != null && !m_Owner.IsAlive)
            {
                return;
            }

            bool done = ProcessIEnumeratorRecursive(m_Routine);
            m_IsDone = !done;
        }

        static Stack<IEnumerator> kIEnumeratorProcessingStack = new Stack<IEnumerator>(32);
        private bool ProcessIEnumeratorRecursive(IEnumerator enumerator)
        {
            var root = enumerator;
            while (enumerator.Current as IEnumerator != null)
            {
                kIEnumeratorProcessingStack.Push(enumerator);
                enumerator = enumerator.Current as IEnumerator;
            }

            //process leaf
            m_Processor.Set(enumerator.Current);
            var result = m_Processor.MoveNext(enumerator);

            while (kIEnumeratorProcessingStack.Count > 1)
            {
                if (!result)
                {
                    result = kIEnumeratorProcessingStack.Pop().MoveNext();
                }
                else
                    kIEnumeratorProcessingStack.Clear();
            }

            if (kIEnumeratorProcessingStack.Count > 0 && !result && root == kIEnumeratorProcessingStack.Pop())
            {
                result = root.MoveNext();
            }

            return result;
        }

        public void Stop()
        {
            m_Owner = null;
            m_Routine = null;
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