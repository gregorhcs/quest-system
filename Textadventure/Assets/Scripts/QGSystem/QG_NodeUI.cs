using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.QGSystem
{
    /* Handles responsive highlighting of quest UI nodes. */
    class QG_NodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Color origColor;
        private Color highlightColor;

        private void Start()
        {
            origColor      = GetComponent<Image>().color;
            highlightColor = origColor + new Color(0.15f, 0.15f, 0.15f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GetComponent<Image>().color = highlightColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GetComponent<Image>().color = origColor;
        }
    }
}
