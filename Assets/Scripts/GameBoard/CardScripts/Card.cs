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
        public CardData cardData { get; protected set; } = new CardData();

        //Variables for unity handling
        private CanvasGroup canvasGroup;
        private Image cardImage;
        private bool isImgLoaded = false;
       
        //Init variables
        private Hand hand = null;
        public Board playerBoard { get; protected set; }
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
            playerBoard.ConvertTo<PlayerBoard>().enableRaycastOnTopCard();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = .8f;
            Debug.Log("OnBeginDrag");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GameObject objectAtDragEnd = eventData.pointerEnter; // Which this object landed on
            switch (this.cardData.cardType)
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
                        || this.cardData.cost > playerBoard.activeDon)
                    {
                        SnapCardBackToParentPos(hand.transform);
                        Debug.Log("Cannot play the card!");
                    }
                    else
                    {
                        cardImage.raycastTarget = false;
                        this.playerBoard.ConvertTo<PlayerBoard>().RestDons(this.cardData.cost);
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
                    if (objectAtDragEnd.transform != this.playerBoard.stageObject.transform || this.cardData.cost > playerBoard.activeDon)
                    {
                        SnapCardBackToParentPos(hand.transform);
                        Debug.Log("Cannot play the card!");
                    }
                    else
                    {
                        SnapCardBackToParentPos(objectAtDragEnd.transform);
                        cardImage.raycastTarget = false;
                        this.playerBoard.ConvertTo<PlayerBoard>().RestDons(this.cardData.cost);
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
                if (cardData.cardType == CardType.DON)
                {
                    cardImage.sprite = Resources.Load<Sprite>("Cards/doncard");
                    isImgLoaded = !isImgLoaded;
                }
                else
                {
                    cardImage.sprite = Resources.Load<Sprite>("Cards/" + cardData.cardID.Split('-')[0] + "/" + cardData.cardID);
                    isImgLoaded = !isImgLoaded;
                }                  
            }
            else
            {
                if (cardData.cardType == CardType.DON)
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

        public void Init(Board playerBoard,Hand hand, string customCardID)
        {
            this.playerBoard = playerBoard;
            this.hand = hand;
            this.cardData.customCardID = customCardID;
            this.gameObject.name = this.cardData.customCardID;
            this.cardData.playerName = playerBoard.boardName;
            UpdateParent();
        }

        public void Init(Board board)
        {
            this.playerBoard = board;
            this.gameObject.name = this.cardData.customCardID;
        }

        public virtual void LoadDataFromCardData(CardData cardData)
        {
            this.cardData = cardData;
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

        public void UpdateParent()
        {
            this.cardData.currentParent = this.transform.parent.name;
        }
        
        public void SetCardActive()
        {
            this.cardData.active = true;
        }

        public void SetCardNotActive()
        {
            this.cardData.active = false;
        }

        public void SnapCardBackToParentPos(Transform newParent)
        {
            this.transform.SetParent(newParent);
            this.transform.position = this.transform.parent.position;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
