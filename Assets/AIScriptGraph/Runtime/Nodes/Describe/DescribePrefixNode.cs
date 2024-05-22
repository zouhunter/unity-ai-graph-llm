using System.Collections;
using System.Collections.Generic;
using System.Text;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

using UnityEngine;

namespace AIScripting.Describe
{
    public enum TextAppendType
    {
        Overwrite,
        Append,
        InsetBegin
    }

    public abstract class DescribePrefixNode : ScriptNodeBase
    {
        [TextArea,Tooltip("前缀信息")]
        public string prefix;
        [Tooltip("导出文本")]
        public Ref<string> export_text;
        public TextAppendType append;
        private StringBuilder _sb;

        protected override void OnProcess()
        {
            _sb = new StringBuilder();
            if(!string.IsNullOrEmpty(prefix))
                _sb.AppendLine(prefix);
            var op = WriteContent(_sb);
            if(op == null)
            {
                DoFinish(false);
            }
            else
            {
                op.RegistComplete((x) => 
                {
                    if(base.status == Status.Running)
                    {
                        DoFinish(true);
                    }
                });
            }
        }

        protected override void DoFinish(bool success = true)
        {
            if(success)
            {
                Debug.Log("DoFinish:" + name);
                if(append == TextAppendType.Overwrite)
                {
                    export_text.SetValue(_sb.ToString());
                }
                else if(append == TextAppendType.InsetBegin)
                {                     
                    export_text.SetValue(_sb.ToString() + export_text.Value);
                }
                else if(append == TextAppendType.Append)
                {
                    export_text.SetValue(export_text.Value + _sb.ToString());
                }
                else
                {
                    export_text.SetValue(_sb.ToString());
                }
            }
            base.DoFinish(success);
        }

        protected abstract AsyncOp WriteContent(StringBuilder sb);
    }

    public abstract class DescribeSuffixNode : ScriptNodeBase
    {
        [TextArea,Tooltip("后缀信息")]
        public string suffix;
        [Tooltip("导出文本")]
        public Ref<string> export_text;
        public TextAppendType append;
        private StringBuilder _sb;

        protected override void OnProcess()
        {
            _sb = new StringBuilder();
            var op = WriteContent(_sb);
            _sb.AppendLine(suffix);
            if(op == null)
            {
                DoFinish(false);
            }
            else
            {
                op.RegistComplete((x) => 
                {
                    if(base.status == Status.Running)
                    {
                        _sb.AppendLine(suffix);
                        DoFinish(true);
                    }
                });
            }
        }

        protected override void DoFinish(bool success = true)
        {
            if(success)
            {
                if (append == TextAppendType.Overwrite)
                {
                    export_text.SetValue(_sb.ToString());
                }
                else if (append == TextAppendType.InsetBegin)
                {
                    export_text.SetValue(_sb.ToString() + export_text.Value);
                }
                else if (append == TextAppendType.Append)
                {
                    export_text.SetValue(export_text.Value + _sb.ToString());
                }
                else
                {
                    export_text.SetValue(_sb.ToString());
                }
            }
            base.DoFinish(success);
        }

        protected abstract AsyncOp WriteContent(StringBuilder sb);
    }
}
