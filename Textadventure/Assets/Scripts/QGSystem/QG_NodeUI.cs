using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.QGSystem
{
    class QG_NodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Color origColor;
        private Color highlightColor;
        private Color pressedColor;
        private Color downColor;

        private Color currentColor;

        private bool isInside = false;
        private bool toggle = false;
        private bool isDown = false;

        public Action onClickAction = () => { };

        private void Start()
        {
            origColor = GetComponent<Image>().color;

            highlightColor = origColor + new Color(0.15f, 0.15f, 0.15f);
            pressedColor = origColor + new Color(0.075f, 0.075f, 0.075f);

            downColor = origColor - new Color(0.1f, 0.1f, 0.1f);

            currentColor = origColor;
        }

        // Called every frame while the mouse stays over this object
        public void OnPointerEnter(PointerEventData eventData)
        {
            //if (!toggle && !isDown)
            GetComponent<Image>().color = highlightColor;
            transform.Find("Text").gameObject.SetActive(true);

            //isInside = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //if (!toggle && !isDown)
            GetComponent<Image>().color = origColor;
            transform.Find("Text").gameObject.SetActive(false);

            //isInside = false;
        }

        public void OnMouseDown()
        {
            Debug.Log("clicked");
            onClickAction();
            /*
                if (isInside)
                {
                    GetComponent<Image>().color = pressedColor;
                    toggle = true;
                }
            */
        }

        /*
        private void OnMouseUp()
        {
            if (isInside && toggle)
            {
                if (isDown)
                {
                    GetComponent<Image>().color = origColor;
                    toggle = false;
                    isDown = false;
                }
                else
                {
                    GetComponent<Image>().color = downColor;
                    toggle = false;
                    isDown = true;
                }
            }
        }
        */
    }
    }
