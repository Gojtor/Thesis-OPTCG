using System;
using TCGSim.CardResources;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TCGSim.CardScripts
{
    public abstract class Card : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
    {
        //Card Data
        public string cardID { get; set; }
        public string cardName { get; set; }
        public string effect { get; set; }
        public double cost { get; set; }
        public CardType cardType { get; set; }
        public Colors color { get; set; }
        public string customCardID { get; set; } //This is needed when there is more than 1 from the same card in the deck

        //Variables for unity handling
        private CanvasGroup canvasGroup;
        private Image cardImage;
        private bool isImgLoaded = false;
       
        //Init variables
        private Hand hand = null;
        private PlayerBoard playerBoard;
        public CardVisibility cardVisibility { get; private set; } = CardVisibility.NONE;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            CheckCardVisibility();
        }

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            hand = this.GetComponentInParent<Hand>();
            cardImage = this.gameObject.GetComponent<Image>();
        }

        public void OnBeginDrag(PointerEventData pointerEventData)
        {
            if (!isImgLoaded)
            {
                FlipCard();
            }
            this.transform.SetParent(this.transform.parent.parent);
            playerBoard.enableRaycastOnTopCard();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = .8f;
            Debug.Log("OnBeginDrag");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (playerBoard == null)
                Debug.LogError("playerBoard is NULL in OnEndDrag!");

            if (canvasGroup == null)
                Debug.LogError("canvasGroup is NULL in OnEndDrag!");

            if (hand == null)
                Debug.LogError("hand is NULL in OnEndDrag!");
            Debug.Log("OnEndDrag called on: " + this.GetType().Name + " | Object Name: " + this.gameObject.name);
            GameObject objectAtDragEnd = eventData.pointerEnter; // Which this object landed on
            if (eventData.pointerEnter == null || objectAtDragEnd.GetComponent<CharacterArea>() == null 
                || objectAtDragEnd.transform.parent != hand.transform.parent || objectAtDragEnd.transform.childCount==6)
            {
                this.transform.SetParent(hand.transform);
                canvasGroup.blocksRaycasts = true;
                Debug.Log("Cannot play the card!");
            }
            else
            {
                cardImage.raycastTarget = false;
            }
            canvasGroup.alpha = 1f;
            Debug.Log("OnEndDrag");
        }

        public void OnDrag(PointerEventData eventData)
        {
            this.transform.position = eventData.position;
            Debug.Log("OnDrag");
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("OnDrop");
        }

        public void FlipCard()
        {
            //Debug.Log("FlipCard called on: " + this.GetType().Name + " | Object Name: " + this.gameObject.name);
            if (!isImgLoaded)
            {
                cardImage.sprite = Resources.Load<Sprite>("Cards/" + cardID.Split('-')[0] + "/" + cardID);
                isImgLoaded = !isImgLoaded;
            }
            else
            {
                cardImage.sprite = Resources.Load<Sprite>("Cards/cardback");
                isImgLoaded = !isImgLoaded;
                cardVisibility = CardVisibility.NONE;
            }
        }

        public void raycastTargetChange(bool on)
        {
            cardImage.raycastTarget = on;
        }

        public void Init(PlayerBoard playerBoard,Hand hand, string customCardID)
        {
            this.playerBoard = playerBoard;
            this.hand = hand;
            this.customCardID = customCardID;
            this.gameObject.name = this.customCardID;
        }

        public virtual void LoadDataFromCardData(CardData cardData)
        {
            this.cardID = cardData.cardID;
            this.cardName = cardData.cardName;
            this.effect = cardData.effect;
            this.cost = cardData.cost;
            this.color = cardData.color;
        }
        public void SetCardVisibility(CardVisibility visibility)
        {
            this.cardVisibility = visibility;
        }

        public void CheckCardVisibility()
        {
            if (cardVisibility == CardVisibility.PLAYERBOARD && !isImgLoaded)
            {
                FlipCard();
            }
        }
    }
}
