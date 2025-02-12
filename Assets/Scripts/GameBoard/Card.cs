using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TCGSim {
    public class Card : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
    {
        private CanvasGroup canvasGroup;
        private Transform hand = null;
        private bool draggable = true;
        private SpriteRenderer spriteRenderer;
        private Image image;
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
            hand = this.GetComponentInParent<Hand>().transform;
            image = this.gameObject.GetComponent<Image>();
        }

        public void OnBeginDrag(PointerEventData pointerEventData) 
        {
            if (draggable)
            {
                this.transform.SetParent(this.transform.parent.parent);
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = .8f;
                Debug.Log("OnBeginDrag");
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GameObject objectAtDragEnd = eventData.pointerEnter; // Which this object landed on
            if (eventData.pointerEnter == null || objectAtDragEnd.GetComponent<CharacterArea>() == null || objectAtDragEnd.transform.parent != hand.parent)
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

        public void changeSpriteImgTo(string cardNumber)
        {
            image.sprite = Resources.Load<Sprite>("Cards/" + cardNumber.Split('-')[0]+"/"+cardNumber);
        }
    }
}
