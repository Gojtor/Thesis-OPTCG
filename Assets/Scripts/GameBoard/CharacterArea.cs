using System.Collections;
using System.Collections.Generic;
using TCGSim.CardScripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TCGSim
{
    public class CharacterArea : MonoBehaviour, IDropHandler,IPointerEnterHandler,IPointerExitHandler
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("OnEndDrag");
            eventData.pointerDrag.transform.SetParent(this.transform);
            eventData.pointerDrag.GetComponent<Card>().playerBoard.serverCon.SendMessageToServer(eventData.pointerDrag.GetComponent<Card>().cardData.customCardID);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("OnPointerEnter");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("OnPointerExit");
        }
    }
}
