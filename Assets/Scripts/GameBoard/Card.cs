using System;
using TCGSim.CardResources;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TCGSim
{
    public class Card : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
    {
        //Card Data
        public string cardID { get; set; }
        public string cardName { get; set; }
        public string effect { get; set; }
        public double cost { get; set; }
        public double power { get; set; }
        public double counter { get; set; }
        public string trigger { get; set; }
        public CardType cardType { get; set; }
        public CharacterType characterType { get; set; }
        public Attributes attribute { get; set; }
        public Colors color { get; set; }

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
            //Debug.Log(Enum.GetName(visibility.GetType(), visibility) + " " + cardName);
            if (cardVisibility == CardVisibility.PLAYERBOARD)
            {
                loadCardImg();
            }
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
                loadCardImg();
            }
            this.transform.SetParent(this.transform.parent.parent);
            playerBoard.enableRaycastOnTopCard();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = .8f;
            Debug.Log("OnBeginDrag");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
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

        public void loadCardImg()
        {
            cardImage.sprite = Resources.Load<Sprite>("Cards/" + cardID.Split('-')[0] + "/" + cardID);
            isImgLoaded = true;
        }
        public void raycastTargetChange(bool on)
        {
            cardImage.raycastTarget = on;
        }

        public void Init(PlayerBoard playerBoard,Hand hand)
        {
            this.playerBoard = playerBoard;
            this.hand = hand;
        }

        public void LoadDataFromCardData(CardData cardData)
        {
            this.cardID = cardData.cardID;
            this.cardName = cardData.cardName;
            this.effect = cardData.effect;
            this.cost = cardData.cost;
            this.power = cardData.power;
            this.counter = cardData.counter;
            this.trigger = cardData.trigger;
            this.cardType = cardData.cardType;
            this.characterType = cardData.characterType;
            this.attribute = cardData.attribute;
            this.color = cardData.color;
        }
        public void SetCardVisibility(CardVisibility visibility)
        {
            this.cardVisibility = visibility;
        }
    }
}
