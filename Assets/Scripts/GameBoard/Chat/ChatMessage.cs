using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TCGSim
{
    public class ChatMessage : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI messageText;

        public void SetText(string text)
        {
            messageText.text = text;
        }
    }
}
