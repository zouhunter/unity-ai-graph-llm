using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.Networking;

namespace AIScripting.Ollama
{
    public class DownloadHandlerMessageQueue : DownloadHandlerScript
    {
        public StringBuilder allText = new StringBuilder();
        private Action<ReceiveData> _onReceive;
        private StringBuilder _textInProcess = new StringBuilder();

        protected override void ReceiveContentLengthHeader(ulong contentLength)
        {
            base.ReceiveContentLengthHeader(contentLength);
            Debug.Log("ReceiveContentLengthHeader:" + contentLength);
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            var text = Encoding.UTF8.GetString(data, 0, dataLength);
            if (text.Contains('\n'))
            {
                var lines = text.Trim().Split('\n');
                foreach (var line in lines)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        OnReceiveOne(line.Trim());
                    }
                }
            }
            else
            {
                OnReceiveOne(text.Trim());
            }
            //_textInProcess.Append(Encoding.UTF8.GetString(data, 0, dataLength));
            //Debug.Log("ReceiveData:" + _textInProcess);
            //int index = -1;
            //var startIndex = -1;
            //var endIndex = -1;
            //var paired = 0;
            //while (++index < _textInProcess.Length)
            //{
            //    var charItem = _textInProcess[index];
            //    if (charItem == '{')
            //    {
            //        if(startIndex < 0)
            //            startIndex = index;
            //        paired++;
            //    }
            //    if (charItem == '}')
            //    {
            //        paired--;
            //    }
            //    if (paired == 0 && startIndex >= 0)
            //    {
            //        endIndex = index;
            //        var oneMessage = _textInProcess.ToString(startIndex, endIndex - startIndex + 1);
            //        OnReceiveOne(oneMessage);
            //        startIndex = -1;
            //    }
            //}
            //if (endIndex > 0 && endIndex < _textInProcess.Length - 1)
            //{
            //    _textInProcess.Remove(0, endIndex+1);
            //}
            return base.ReceiveData(data, dataLength);
        }

        private void OnReceiveOne(string text)
        {
            //Debug.Log("OnReceiveOne:" + text);
            var receiveData = JsonUtility.FromJson<ReceiveData>(text);
            allText.Append(receiveData.message.content);
            _onReceive?.Invoke(receiveData);
        }

        internal void RegistReceive(Action<ReceiveData> onReceive)
        {
            this._onReceive = onReceive;
        }
    }
}
