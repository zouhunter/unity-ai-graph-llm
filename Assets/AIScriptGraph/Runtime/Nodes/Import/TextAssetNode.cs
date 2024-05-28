using System.Linq;
using System.Text;

using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting.Import
{
    [CustomNode("TextAsset", 1, Define.GROUP)]
    public class TextAssetNode : ScriptNodeBase
    {
        public Ref<TextAsset> inputTextfile;
        public Ref<string> exportfileName;
        public Ref<string> exportfileInfo;

        protected override void OnProcess()
        {
            if (!inputTextfile.Value)
            {
                DoFinish(false);
                return;
            }
            exportfileName.SetValue(inputTextfile.Value.name);
            exportfileInfo.SetValue(inputTextfile.Value.text);
            DoFinish(true);
        }
    }
}
