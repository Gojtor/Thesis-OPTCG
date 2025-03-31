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
        public bool draggable { get; protected set; } = false;
       
        //Init variables
        private Hand hand = null;
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
            if (!draggable) { return; }
            if (!isImgLoaded)
            {
                FlipCard();
            }
            this.transform.SetParent(this.transform.parent.parent);
            PlayerBoard.Instance.enableDraggingOnTopDeckCard();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = .8f;
            Debug.Log("OnBeginDrag");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!draggable) { return; }
            GameObject objectAtDragEnd = eventData.pointerEnter; // Which this object landed on
            if (objectAtDragEnd == null)
            {
                Debug.Log("objectAtDragEnd is NULL - Dropped outside the scene!");
                SnapCardBackToParentPos(hand.transform);
                canvasGroup.alpha = 1f;
                return;
            }
            switch (this.cardData.cardType)
            {
                case CardType.CHARACTER:
                    if (hand == null)
                        Debug.LogError("hand is NULL in OnEndDrag!");
                    Debug.Log("OnEndDrag called on: " + this.GetType().Name + " | Object Name: " + this.gameObject.name);
                    if (objectAtDragEnd.GetComponent<CharacterArea>() != PlayerBoard.Instance.gameObject.GetComponentInChildren<CharacterArea>() || objectAtDragEnd.transform.childCount == 6
                        || this.cardData.cost > PlayerBoard.Instance.activeDon)
                    {
                        SnapCardBackToParentPos(hand.transform);
                        Debug.Log("Cannot play the card!");
                    }
                    else
                    {
                        this.SetCardActive();
                        PlayerBoard.Instance.RestDons(this.cardData.cost);
                    }
                    canvasGroup.alpha = 1f;
                    Debug.Log("OnEndDrag");
                    this.draggable = false;
                    break;
                case CardType.DON:
                    Debug.Log("OnEndDrag called on: " + this.GetType().Name + " | Object Name: " + this.gameObject.name);
                    if (objectAtDragEnd.GetComponent<CostArea>() != PlayerBoard.Instance.gameObject.GetComponentInChildren<CostArea>())
                    {
                        SnapCardBackToParentPos(PlayerBoard.Instance.donDeckObject.transform);
                        FlipCard();
                    }
                    else
                    {
                        cardImage.raycastTarget = false;
                        this.SetCardActive();
                    }
                    canvasGroup.alpha = 1f;
                    Debug.Log("OnEndDrag");
                    break;
                case CardType.STAGE:
                    Debug.Log("OnEndDrag called on: " + this.GetType().Name + " | Object Name: " + this.gameObject.name);
                    if (objectAtDragEnd.transform != PlayerBoard.Instance.stageObject.transform || this.cardData.cost > PlayerBoard.Instance.activeDon)
                    {
                        SnapCardBackToParentPos(hand.transform);
                        Debug.Log("Cannot play the card!");
                    }
                    else
                    {
                        SnapCardBackToParentPos(objectAtDragEnd.transform);
                        cardImage.raycastTarget = false;
                        PlayerBoard.Instance.RestDons(this.cardData.cost);
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
            if (!draggable) { return; }
            this.transform.position = eventData.position;
            Debug.Log("OnDrag");
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!draggable) { return; }
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

        public void ChangeDraggable(bool isDraggable)
        {
            this.draggable = isDraggable;
        }

        public void raycastTargetChange(bool on)
        {
            cardImage.raycastTarget = on;
        }

        public void Init(Hand hand, string customCardID)
        {
            this.hand = hand;
            this.cardData.customCardID = customCardID;
            this.gameObject.name = this.cardData.customCardID;
            this.cardData.playerName = PlayerBoard.Instance.boardName;
            UpdateParent();
        }

        public void Init()
        {
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

        public void SetDraggable(bool isDraggable)
        {
            this.draggable = isDraggable;
        }
    }
}
