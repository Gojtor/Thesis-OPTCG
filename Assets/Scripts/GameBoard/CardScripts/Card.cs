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
        public string cardID { get; protected set; } = "#";
        public string cardName { get; protected set; } = "#";
        public string effect { get; protected set; } = "#";
        public int cost { get; protected set; } = 0;
        public CardType cardType { get; protected set; } = CardType.DON;
        public Colors color { get; protected set; } = Colors.Red;
        public string customCardID { get; protected set; } = "#"; //This is needed when there is more than 1 from the same card in the deck
        public string playerName { get; protected set; } = "#";
        public string currentParent { get; protected set; } = "#";
        public bool active { get; protected set; }


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
            GameObject objectAtDragEnd = eventData.pointerEnter; // Which this object landed on
            switch (this.cardType)
            {
                case CardType.CHARACTER:
                    if (playerBoard == null)
                        Debug.LogError("playerBoard is NULL in OnEndDrag!");

                    if (canvasGroup == null)
                        Debug.LogError("canvasGroup is NULL in OnEndDrag!");

                    if (hand == null)
                        Debug.LogError("hand is NULL in OnEndDrag!");
                    Debug.Log("OnEndDrag called on: " + this.GetType().Name + " | Object Name: " + this.gameObject.name);
                    if (objectAtDragEnd.GetComponent<CharacterArea>() != this.playerBoard.gameObject.GetComponentInChildren<CharacterArea>() || objectAtDragEnd.transform.childCount == 6
                        || this.cost > playerBoard.activeDon)
                    {
                        SnapCardBackToParentPos(hand.transform);
                        Debug.Log("Cannot play the card!");
                    }
                    else
                    {
                        cardImage.raycastTarget = false;
                        this.playerBoard.RestDons(this.cost);
                    }
                    canvasGroup.alpha = 1f;
                    Debug.Log("OnEndDrag");
                    break;
                case CardType.DON:
                    Debug.Log("OnEndDrag called on: " + this.GetType().Name + " | Object Name: " + this.gameObject.name);
                    if (objectAtDragEnd.GetComponent<CostArea>() != this.playerBoard.gameObject.GetComponentInChildren<CostArea>())
                    {
                        SnapCardBackToParentPos(playerBoard.donDeckObject.transform);
                        FlipCard();
                    }
                    else
                    {
                        cardImage.raycastTarget = false;
                    }
                    canvasGroup.alpha = 1f;
                    Debug.Log("OnEndDrag");
                    break;
                case CardType.STAGE:
                    Debug.Log("OnEndDrag called on: " + this.GetType().Name + " | Object Name: " + this.gameObject.name);
                    if (objectAtDragEnd.transform != this.playerBoard.stageObject.transform || this.cost > playerBoard.activeDon)
                    {
                        SnapCardBackToParentPos(hand.transform);
                        Debug.Log("Cannot play the card!");
                    }
                    else
                    {
                        SnapCardBackToParentPos(objectAtDragEnd.transform);
                        cardImage.raycastTarget = false;
                        this.playerBoard.RestDons(this.cost);
                    }
                    canvasGroup.alpha = 1f;
                    Debug.Log("OnEndDrag");
                    break;
                default:
                    SnapCardBackToParentPos(hand.transform);
                    canvasGroup.alpha = 1f;
                    Debug.Log("OnEndDrag");
                    break;
            }

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
                if (cardType == CardType.DON)
                {
                    cardImage.sprite = Resources.Load<Sprite>("Cards/doncard");
                    isImgLoaded = !isImgLoaded;
                }
                else
                {
                    cardImage.sprite = Resources.Load<Sprite>("Cards/" + cardID.Split('-')[0] + "/" + cardID);
                    isImgLoaded = !isImgLoaded;
                }                  
            }
            else
            {
                if (cardType == CardType.DON)
                {
                    cardImage.sprite = Resources.Load<Sprite>("Cards/donback");
                    isImgLoaded = !isImgLoaded;
                }
                else
                {
                    cardImage.sprite = Resources.Load<Sprite>("Cards/cardback");
                    isImgLoaded = !isImgLoaded;
                    cardVisibility = CardVisibility.NONE;
                }   
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
            this.playerName = playerBoard.boardName;
            UpdateParent();
        }

        public virtual void LoadDataFromCardData(CardData cardData)
        {
            this.cardID = cardData.cardID;
            this.cardName = cardData.cardName;
            this.cardType = cardData.cardType;
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

        public virtual CardData TurnCardToCardData()
        {
            CardData cardData = new CardData();
            cardData.cardID = this.cardID;
            cardData.cardName = this.cardName;
            cardData.effect = this.effect;
            cardData.cost = this.cost;
            cardData.color = this.color;
            cardData.active = false;
            cardData.customCardID = this.customCardID;
            cardData.playerName = this.playerBoard.boardName;
            cardData.gameCustomID = this.customCardID;
            cardData.gameID = 1;
            cardData.currentParent = this.currentParent;
            return cardData;
        }

        public void UpdateParent()
        {
            this.currentParent = this.transform.parent.name;
        }
        
        public void SetCardActive()
        {
            this.active = true;
        }

        public void SetCardNotActive()
        {
            this.active = false;
        }

        public void SnapCardBackToParentPos(Transform newParent)
        {
            this.transform.SetParent(newParent);
            this.transform.position = this.transform.parent.position;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
