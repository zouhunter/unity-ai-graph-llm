
using UnityEngine;
using TMPro;

namespace UAI{
    public class UIMsgElement : MonoBehaviour
    {  
        public TMP_Text txtRole;
        public TMP_Text txtContent;
        public TMP_Text txtTime;

        public void updateUI(string role, string content, string time)
        {
            txtRole.text = role;
            txtContent.text = content;
            txtTime.text = time;
        }
    }
}