using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TCGSim {
    public class Card : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        Transform hand = null;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void OnBeginDrag(PointerEventData pointerEventData) 
        {
            hand = this.transform.parent;
            this.transform.SetParent(this.transform.parent.parent);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            this.transform.SetParent(hand);
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;   
        }
    }
}
