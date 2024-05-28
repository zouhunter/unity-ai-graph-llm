using System.Collections;
using System.Collections.Generic;
using System.Text;

using UFrame.NodeGraph;
using UFrame.NodeGraph.DataModel;

using UnityEngine;

namespace AIScripting.Describe
{
    public abstract class DescribeTextAttachNode : ScriptNodeBase
    {
        [TextArea, Tooltip("前后缀信息")]
        public string textfix;
        [Tooltip("导出文本")]
        public Ref<string> export_text;
        [Tooltip("附加类型,前缀还是后缀")]
        public TextAttachType attachType;
        [Tooltip("修改类型,重写还是添加")]
        public TextModifyType modifyType;
        private StringBuilder _sb;

        protected override void OnProcess()
        {
            _sb = new StringBuilder();
            if (attachType == TextAttachType.Prefix)
            {
                if (!string.IsNullOrEmpty(textfix))
                    _sb.AppendLine(textfix);
            }
            var op = WriteContent(_sb);
            if (op == null)
            {
                DoFinish(false);
            }
            else
            {
                op.RegistComplete((x) =>
                {
                    if (base.status == Status.Running)
                    {
                        DoFinish(true);
                    }
                });
            }
        }
        protected override void DoFinish(bool success = true)
        {
            if (attachType == TextAttachType.Suffix)
            {
                _sb.AppendLine(textfix);
            }
            else if (attachType == TextAttachType.Format)
            {
                var text = _sb.ToString().Trim();
                _sb.Clear();
                _sb.AppendLine(string.Format(textfix, text));
            }
            if (success)
            {
                Debug.Log("DoFinish:" + name);
                if (modifyType == TextModifyType.Overwrite)
                {
                    export_text.SetValue(_sb.ToString());
                }
                else if (modifyType == TextModifyType.InsetBegin)
                {
                    export_text.SetValue(_sb.ToString() + export_text.Value);
                }
                else if (modifyType == TextModifyType.Append)
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
