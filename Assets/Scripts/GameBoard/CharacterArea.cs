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
        public async void OnDrop(PointerEventData eventData)
        {
            Card card = eventData.pointerDrag.GetComponent<Card>();
            if (!card.draggable || card == null || 
                eventData.pointerDrag.gameObject.GetComponent<DonCard>() != null ||
                eventData.pointerDrag.gameObject.GetComponent<StageCard>() != null
                || card.cardData.cost>PlayerBoard.Instance.activeDon) 
            { return; }

            //Debug.Log("OnEndDrag");
            card.transform.SetParent(this.transform);
            card.UpdateParent();
            card.SetCardVisibility(CardResources.CardVisibility.BOTH);
            if (card.transform.parent == PlayerBoard.Instance.characterAreaObject.transform)
            {
                ServerCon.Instance.SendMessageToServer(eventData.pointerDrag.GetComponent<Card>().cardData.customCardID);
                await ServerCon.Instance.UpdateCardAtInGameStateDB(card);
                await ServerCon.Instance.UpdateMyCardAtEnemy(card.cardData.customCardID);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("OnPointerEnter");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("OnPointerExit");
        }
    }
}
