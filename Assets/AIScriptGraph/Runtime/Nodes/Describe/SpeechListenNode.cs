using UFrame.NodeGraph;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace AIScripting.Export
{
    [CustomNode("TextListen", group: Define.GROUP)]
    public class TextListenNode : ScriptNodeBase
    {
        [Tooltip("info text")]
        public Ref<string> textInfo;
        private DictationRecognizer m_DictationRecognizer;
        private string m_Hypotheses;
        protected override void OnProcess()
        {
            m_DictationRecognizer = new DictationRecognizer();

            m_DictationRecognizer.DictationResult += (text, confidence) =>
            {
                Debug.LogFormat("Dictation result: {0}", text);
                textInfo.SetValue(textInfo.Value + text + "\n");
            };

            m_DictationRecognizer.DictationHypothesis += (text) =>
            {
                Debug.LogFormat("Dictation hypothesis: {0}", text);
                textInfo.SetValue(textInfo.Value + text);
            };

            m_DictationRecognizer.DictationComplete += (completionCause) =>
            {
                if (completionCause != DictationCompletionCause.Complete)
                    Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
            };

            m_DictationRecognizer.DictationError += (error, hresult) =>
            {
                Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
            };

            m_DictationRecognizer.Start();
        }
    }
}
