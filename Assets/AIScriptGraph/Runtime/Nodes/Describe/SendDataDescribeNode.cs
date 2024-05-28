using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UFrame.NodeGraph;

using UnityEngine;

namespace AIScripting.Describe
{
    [CustomNode("SendData", group: Define.GROUP)]
    public class SendDataDescribeNode : ScriptNodeBase
    {
        [Tooltip("input role")]
        public Ref<string> input_role;
        [Tooltip("input data")]
        public Ref<string> input_text;
        [Tooltip("output data list")]
        public Ref<List<SendData>> output_list = new Ref<List<SendData>>("output_data_list", new List<SendData>());
        [Tooltip("modify data")]
        public TextModifyType modifyType;

        protected override void OnProcess()
        {
            if (!string.IsNullOrEmpty(input_text) && !string.IsNullOrEmpty(input_role))
            {
                var data = new SendData() { role = input_role.Value, content = input_text.Value };
                switch (modifyType)
                {
                    case TextModifyType.Overwrite:
                        output_list.Value.Clear();
                        output_list.Value.Add(data);
                        break;
                    case TextModifyType.Append:
                        output_list.Value.Add(data);
                        break;
                    case TextModifyType.InsetBegin:
                        output_list.Value.Insert(0, data);
                        break;
                    default:
                        break;
                }
            }
            DoFinish(true);
        }
    }
}
