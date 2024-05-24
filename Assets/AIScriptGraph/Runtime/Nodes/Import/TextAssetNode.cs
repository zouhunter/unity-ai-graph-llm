using System.Linq;
using System.Text;

using UFrame.NodeGraph;
using UnityEngine;

namespace AIScripting.Import
{
    [CustomNode("TextAsset", 1, Define.GROUP)]
    public class TextAssetNode : ScriptNodeBase
    {
        public Ref<TextAsset> textfile;
        public Ref<string> exportfileName;
        public Ref<string> exportfileInfo;

        protected override void OnProcess()
        {
            if (!textfile.Value)
            {
                DoFinish(false);
                return;
            }
            exportfileName.SetValue(textfile.Value.name);
            exportfileInfo.SetValue(textfile.Value.text);
            DoFinish(true);
        }
    }
}
