using TMPro;
using UnityEngine;

namespace Tractor.UI
{
    public class UIMessage : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI messageText;

        public void Init(string message)
        {
            messageText.text = message;
        }
    }
}
