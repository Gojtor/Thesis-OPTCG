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
        public bool canAttack { get; protected set; } = false;
        public bool rested { get; protected set; } = false;
        public static int howManyCardsNeedToBeDrawn { get; protected set; } = 0;
        public static bool needToWatchHowManyDrawn { get; protected set; } = false;
        public static Transform fromWhereTheCardNeedsToBeDrawn { get; protected set; }
        protected static int onBeginDragCounter { get; set; } = 0;
        public static event Action CorrectAmountCardsDrawn;
        protected GameObject border;
        //Init variables
        private Hand hand = null;

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
            if (needToWatchHowManyDrawn && this.gameObject.transform.parent == fromWhereTheCardNeedsToBeDrawn)
            {
                onBeginDragCounter++;
                if (onBeginDragCounter == howManyCardsNeedToBeDrawn)
                {
                    CorrectAmountCardsDrawn?.Invoke();
                }
            }
            if (!isImgLoaded)
            {
                FlipCard();
            }
            if (this.cardData.cardType != CardType.DON)
            {
                this.transform.SetParent(PlayerBoard.Instance.handObject.transform);
                this.UpdateParent();
            }
            else
            {
                this.transform.SetParent(PlayerBoard.Instance.costAreaObject.transform);
                this.UpdateParent();
            }
            canvasGroup.blocksRaycasts = false;
            this.transform.SetParent(this.transform.parent.parent);
            canvasGroup.alpha = .8f;
            //Debug.Log("OnBeginDrag");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!draggable) { return; }
            GameObject objectAtDragEnd = eventData.pointerEnter; // Which this object landed on
            if (objectAtDragEnd == null)
            {
                Debug.Log("objectAtDragEnd is NULL - Dropped outside the scene!");
                SnapCardBackToParentPos();
                canvasGroup.alpha = 1f;
                return;
            }
            switch (this.cardData.cardType)
            {
                case CardType.CHARACTER:
                    if (objectAtDragEnd.transform != PlayerBoard.Instance.characterAreaObject.transform || objectAtDragEnd.transform.childCount == 6
                        || this.cardData.cost > PlayerBoard.Instance.activeDon || GameManager.Instance.currentPlayerTurnPhase != PlayerTurnPhases.MAINPHASE)
                    {
                        SnapCardBackToParentPos();
                        Debug.Log("Cannot play the card!");
                    }
                    else
                    {
                        PlayerBoard.Instance.RestDons(this.cardData.cost);
                        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(this);
                    }
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.alpha = 1f; //100% opacity
                    break;
                case CardType.DON:
                    if (objectAtDragEnd.transform != PlayerBoard.Instance.costAreaObject.transform || GameManager.Instance.currentPlayerTurnPhase != PlayerTurnPhases.MAINPHASE || objectAtDragEnd.gameObject.GetComponent<DonCard>() != null)
                    {
                        SnapCardBackToParentPos();
                    }
                    CharacterCard charCard = objectAtDragEnd.gameObject.GetComponent<CharacterCard>();
                    if(charCard != null && charCard.transform.parent.parent.GetComponent<PlayerBoard>() != null && charCard.transform.parent.GetComponent<CharacterArea>() != null)
                    {
                        AttachDon(objectAtDragEnd.gameObject.GetComponent<Card>());
                    }
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.alpha = 1f; //100% opacity
                    break;
                case CardType.STAGE:
                    Debug.Log("OnEndDrag called on: " + this.GetType().Name + " | Object Name: " + this.gameObject.name);
                    if (objectAtDragEnd.transform != PlayerBoard.Instance.stageObject.transform || GameManager.Instance.currentPlayerTurnPhase != PlayerTurnPhases.MAINPHASE || this.cardData.cost > PlayerBoard.Instance.activeDon)
                    {
                        SnapCardBackToParentPos();
                        Debug.Log("Cannot play the card!");
                    }
                    else
                    {
                        PlayerBoard.Instance.RestDons(this.cardData.cost);
                        PlayerBoard.Instance.MoveStageFromHandToStageArea(this);   
                    }
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.alpha = 1f;
                    break;
                case CardType.EVENT:
                    Debug.Log("OnEndDrag called on: " + this.GetType().Name + " | Object Name: " + this.gameObject.name);
                    if (objectAtDragEnd.transform != PlayerBoard.Instance.characterAreaObject.transform || GameManager.Instance.currentPlayerTurnPhase!=PlayerTurnPhases.MAINPHASE || this.cardData.cost > PlayerBoard.Instance.activeDon)
                    {
                        SnapCardBackToParentPos();
                        Debug.Log("Cannot play the card!");
                    }
                    else
                    {
                        PlayerBoard.Instance.RestDons(this.cardData.cost);
                        PlayerBoard.Instance.MoveCardToTrash(this);
                    }
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.alpha = 1f;
                    break;
                default:
                    SnapCardBackToParentPos();
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.alpha = 1f;
                    break;
            }

        }

        private void AttachDon(Card card)
        {
            card.EnableCanvasOverrideSorting();
            Canvas thisCardCanvas = this.GetComponent<Canvas>();
            thisCardCanvas.overrideSorting = true;
            thisCardCanvas.sortingOrder = 1;
            card.GetComponent<LineRenderer>().sortingOrder = 4;
            SnapCardBackToParentPos(card.transform);
            this.transform.Translate(0, -30, 0);
            this.SetCardNotActive();
            this.draggable = false;
        }

        public async void SendCardToServer()
        {
            await ServerCon.Instance.UpdateCardAtInGameStateDB(this);
            await ServerCon.Instance.UpdateMyCardAtEnemy(this.cardData.customCardID);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!draggable) { return; }
            this.transform.position = eventData.position;
            //Debug.Log("OnDrag");
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!draggable) { return; }
            //Debug.Log("OnDrop");
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
                    cardData.cardVisibility = CardVisibility.NONE;
                }
                else
                {
                    cardImage.sprite = Resources.Load<Sprite>("Cards/cardback");
                    isImgLoaded = !isImgLoaded;
                    cardData.cardVisibility = CardVisibility.NONE;
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
            this.cardData.playerName = PlayerBoard.Instance.playerName;
            this.cardData.gameCustomID = PlayerBoard.Instance.gameCustomID;
            UpdateParent();
        }

        public void Init()
        {
            this.gameObject.name = this.cardData.customCardID;
            UpdateParent();
            
        }

        public virtual void LoadDataFromCardData(CardData cardData)
        {
            this.cardData = cardData;
        }
        public void SetCardVisibility(CardVisibility visibility)
        {
            this.cardData.cardVisibility = visibility;
        }

        public void CheckCardVisibility()
        {
            if (cardData.cardVisibility == CardVisibility.PLAYERBOARD && !isImgLoaded && this.transform.parent.parent!=EnemyBoard.Instance.transform)
            {
                FlipCard();
            }
            else if(cardData.cardVisibility == CardVisibility.BOTH && !isImgLoaded)
            {
                FlipCard();
            }
        }

        public void UpdateParent()
        {
            this.cardData.currentParent = this.transform.parent.name;
        }

        public void UpdateEnemyCardAfterDataLoad()
        {
            this.transform.SetParent(EnemyBoard.Instance.GetParentByNameString(this.cardData.currentParent).transform);
            Transform newParent = this.transform.parent;
            if (!this.cardData.currentParent.Contains("LifeArea"))
            {
                this.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            if (this.GetComponent<DonCard>()!=null && !this.cardData.active && this.transform.parent!=PlayerBoard.Instance.donDeckObject.transform)
            {
                this.GetComponent<DonCard>().EnemyRestDon();
            }
            if (this.GetComponent<DonCard>() != null && this.cardData.active && this.transform.parent != PlayerBoard.Instance.donDeckObject.transform)
            {
                this.GetComponent<DonCard>().EnemyRestandDon();
            }
            if (this.cardData.active)
            {
                this.Restand(true, false);
            }
            else
            {
                this.Rest();
            }
            this.transform.position = this.transform.parent.position;
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
        }

        public void SnapCardBackToParentPos()
        {
            if (this.cardData.cardType == CardType.DON)
            {
                PlayerBoard.Instance.MoveDonFromDeckToCostArea(this);
            }
            else
            {
                if (GameManager.Instance.currentPlayerTurnPhase == PlayerTurnPhases.DONPHASE)
                {
                    PlayerBoard.Instance.AddCardToHandFromDeck(this, true, true);
                }
                else
                {
                    this.transform.SetParent(PlayerBoard.Instance.handObject.transform);
                }
            } 
            this.transform.position = this.transform.parent.position;
        }

        public void SetDraggable(bool isDraggable)
        {
            this.draggable = isDraggable;
        }

        public static void SetHowManyCardNeedsToBeDrawn(int thisMany)
        {
            howManyCardsNeedToBeDrawn = thisMany;
        }

        public static void NeedToWatchHowManyCardsDrawn(bool need, Transform fromWhere)
        {
            onBeginDragCounter = 0;
            needToWatchHowManyDrawn = need;
            fromWhereTheCardNeedsToBeDrawn = fromWhere;
        }

        public void Rest()
        {
            if (!rested)
            {
                this.cardData.active = false;
                if (this.GetComponent<CharacterCard>() != null)
                {
                    this.GetComponent<CharacterCard>().CardCannotAttack();
                }
                else if (this.GetComponent<LeaderCard>()!=null)
                {
                    this.GetComponent<LeaderCard>().CardCannotAttack();
                }
                {
                    this.canAttack = false;
                }
                this.transform.rotation = Quaternion.Euler(0, 0, 90);
                this.rested = true;
            }
        }

        public void Restand(bool active,bool canAttack)
        {
            if (rested && this.transform.parent.parent==EnemyBoard.Instance.transform)
            {
                this.cardData.active = active;
                this.canAttack = canAttack;
                this.transform.rotation = Quaternion.Euler(0, 0, 180);
                this.rested = false;
            }
            else if(rested && this.transform.parent.parent == PlayerBoard.Instance.transform)
            {
                this.cardData.active = active;
                this.canAttack = canAttack;
                this.transform.rotation = Quaternion.Euler(0, 0, 0);
                this.rested = false;
            }
        }

        public void MakeBordedForThisCard()
        {
            border = new GameObject("Outline");
            border.transform.SetParent(this.gameObject.transform);
            Image borderIMG = border.AddComponent<Image>();
            RectTransform rectTransform = border.GetComponent<RectTransform>();
            border.transform.position = this.transform.position;
            rectTransform.sizeDelta = new Vector2(110, 150);
            borderIMG.color = Color.green;
            Outline borderOutline = border.AddComponent<Outline>();
            borderOutline.enabled = true;
            borderOutline.effectColor = Color.green;
            borderOutline.effectDistance = new Vector2(10, 10);
            Canvas canvas = border.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 2;
            EnableCanvasOverrideSorting();
        }

        public async void RemoveBorderForThisCard()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                Destroy(border);
                ResetCanvasOverrideSorting();
            });
        }

        public void ResetCanvasOverrideSorting()
        {
            Canvas thisCardCanvas = this.gameObject.GetComponent<Canvas>();
            thisCardCanvas.overrideSorting = false;
        }

        public void EnableCanvasOverrideSorting()
        {
            Canvas thisCardCanvas = this.gameObject.GetComponent<Canvas>();
            thisCardCanvas.overrideSorting = true;
            thisCardCanvas.sortingOrder = 3;
        }
    }
}
