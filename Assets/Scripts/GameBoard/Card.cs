using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TCGSim {
    public class Card : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
    {
        private CanvasGroup canvasGroup;
        private Transform hand = null;
        private bool draggable = true;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            
        }

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData pointerEventData) 
        {
            if (draggable)
            {
                hand = this.transform.parent;
                this.transform.SetParent(this.transform.parent.parent);
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = .8f;
                Debug.Log("OnBeginDrag");
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GameObject objectAtDragEnd = eventData.pointerEnter; // Which this object landed on
            Transform playerHandParent = this.transform.parent.parent;
            if (eventData.pointerEnter == null || objectAtDragEnd.GetComponent<CharacterArea>() == null || objectAtDragEnd.transform.parent != playerHandParent)
            {
                this.transform.SetParent(hand);
                canvasGroup.blocksRaycasts = true;
            }
            else
            {
                draggable = false;
            }
            canvasGroup.alpha = 1f;
            Debug.Log("OnEndDrag");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (draggable)
            {
                this.transform.position = eventData.position;
            }
            Debug.Log("OnDrag");
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("OnDrop");
        }
    }
}
