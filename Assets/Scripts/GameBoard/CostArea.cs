using System;
using System.Collections;
using System.Collections.Generic;
using TCGSim.CardScripts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace TCGSim
{
    public class CostArea : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // Start is called before the first frame update

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            CheckForResize();
        }

        private void CheckForResize()
        {
            if (this.transform.childCount > 6)
            {
                this.GetComponent<HorizontalLayoutGroup>().childControlWidth = true;
            }
            else
            {
                this.GetComponent<HorizontalLayoutGroup>().childControlWidth = false;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            Card card = eventData.pointerDrag.GetComponent<Card>();
            if (!card.draggable || eventData.pointerDrag.gameObject.GetComponent<DonCard>() == null) { return; }
            PlayerBoard.Instance.MoveDonFromDeckToCostArea(card);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log("OnPointerEnter");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log("OnPointerExit");
        }

        public void RestDons(int donCountToRest)
        {
            if (donCountToRest <= this.transform.childCount)
            {
                List<DonCard> activeDons = GetActiveDons();
                for (int i = 0; i < donCountToRest; i++)
                {
                    activeDons[i].RestDon();
                    activeDons[i].SendCardToServer();
                }
            }
            else
            {
                Debug.Log("You want to rest more dons than the available amount in cost area! The amount you want to rest: " + donCountToRest + ", the amount you have: " + this.transform.childCount);
            }
        }

        public int GetActiveDonCount()
        {
            int activeDonCount = 0;
            for (int i = 0; i < this.transform.childCount; i++)
            {
                if (this.transform.GetChild(i).GetComponent<DonCard>().cardData.active)
                {
                    activeDonCount++;
                }
            }
            return activeDonCount;
        }

        public List<DonCard> GetActiveDons()
        {
            List<DonCard> activeDons = new List<DonCard>();
            for (int i = 0; i < this.transform.childCount; i++)
            {
                DonCard currentCard = this.transform.GetChild(i).GetComponent<DonCard>();
                if (currentCard.cardData.active)
                {
                    activeDons.Add(currentCard);
                }
            }
            return activeDons;
        }
    }
}
