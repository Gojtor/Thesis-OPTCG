using System;
using System.Collections.Generic;
using System.Linq;
using TCGSim.CardResources;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TCGSim.CardScripts
{
    public abstract class Card : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        //Card Data
        public CardData cardData { get; protected set; } = new CardData();
        public List<Effects> effects { get; protected set; }

        //Variables for unity handling
        private CanvasGroup canvasGroup;
        private Image cardImage;
        private bool isImgLoaded = false;
        public int originalPower { get; protected set; } = -1;
        public bool draggable { get; protected set; } = false;
        public bool canAttack { get; protected set; } = false;
        public bool rested { get; protected set; } = false;
        public bool hasDonAttached { get; protected set; } = false;
        public static int howManyCardsNeedToBeDrawn { get; protected set; } = 0;
        public static bool needToWatchHowManyDrawn { get; protected set; } = false;
        public static Transform fromWhereTheCardNeedsToBeDrawn { get; protected set; }
        protected static int onBeginDragCounter { get; set; } = 0;
        public static event Action CorrectAmountCardsDrawn;
        public event Action<Card, Card> CardClickedWithLeftMouseForCountering;
        public Action<Card> onCardClicked { get; protected set; }
        protected Card attacked;
        public int plusPower { get; protected set; } = 0;
        protected GameObject border;
        protected GameObject powerText;
        public bool currentlyAttacking { get; protected set; } = false;
        public bool targetForEffect { get; protected set; } = false;

        private List<Effects> whenAttackingEffectsAddedByOtherCards = new List<Effects>();

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
            GameManager.OnGameStateChange += GameManager_OnGameStateChange;
        }

        private void OnDestroy()
        {
            ClearClickAction();
        }

        private void GameManager_OnGameStateChange(GameState state)
        {
            if (state == GameState.MATCHWON || state == GameState.MATCHLOST)
            {
                this.RemoveBorderForThisCard();
            }
            this.ResetPlusPower();
            this.HidePlusPowerOnCard();
        }

        public void OnBeginDrag(PointerEventData pointerEventData)
        {
            if (!draggable || GameManager.Instance.currentBattlePhase != BattlePhases.NOBATTLE || PlayerBoard.Instance.effectInProgress || !PlayerBoard.Instance.enemyFinishedStartingHand) { return; }
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
            this.EnableCanvasOverrideSortingOnDraggingCard();
            //Debug.Log("OnBeginDrag");
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!draggable || GameManager.Instance.currentBattlePhase != BattlePhases.NOBATTLE || PlayerBoard.Instance.effectInProgress || !PlayerBoard.Instance.enemyFinishedStartingHand) { return; }
            this.ResetCanvasOverrideSorting();
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
                    if ((objectAtDragEnd.transform != PlayerBoard.Instance.characterAreaObject.transform && objectAtDragEnd.transform.GetComponent<CharacterCard>() == null) || objectAtDragEnd.transform.parent == PlayerBoard.Instance.handObject.transform || this.cardData.cost > PlayerBoard.Instance.activeDon || GameManager.Instance.currentPlayerTurnPhase != PlayerTurnPhases.MAINPHASE)
                    {
                        SnapCardBackToParentPos();
                        Debug.Log("Cannot play the card!");
                    }
                    else if (objectAtDragEnd.transform == PlayerBoard.Instance.characterAreaObject.transform && objectAtDragEnd.transform.childCount >= 5)
                    {
                        PlayerBoard.Instance.RemoveCardToMakeRoomForNewOne(this);
                    }
                    else if (objectAtDragEnd.transform.GetComponent<CharacterCard>() != null && objectAtDragEnd.transform.parent == PlayerBoard.Instance.characterAreaObject.transform && objectAtDragEnd.transform.parent.childCount >= 5)
                    {
                        PlayerBoard.Instance.RemoveCardToMakeRoomForNewOne(this);
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
                    Card cardAtEnd = objectAtDragEnd.gameObject.GetComponent<Card>();
                    if (cardAtEnd != null && cardAtEnd.transform.parent.parent.GetComponent<PlayerBoard>() != null
                        && ((cardAtEnd.GetComponent<CharacterCard>() != null && cardAtEnd.transform.parent.GetComponent<CharacterArea>() != null) ||
                        (cardAtEnd.GetComponent<LeaderCard>() != null && cardAtEnd.transform.parent == PlayerBoard.Instance.leaderObject.transform)))
                    {
                        AttachDon(objectAtDragEnd.gameObject.GetComponent<Card>(), this);
                        this.SendCardToServer();
                    }
                    else if (objectAtDragEnd.transform != PlayerBoard.Instance.costAreaObject.transform || GameManager.Instance.currentPlayerTurnPhase != PlayerTurnPhases.MAINPHASE
                        || objectAtDragEnd.gameObject.GetComponent<DonCard>() != null || objectAtDragEnd.gameObject.GetComponent<CharacterCard>() != null)
                    {
                        SnapCardBackToParentPos();
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
                    else if (objectAtDragEnd.transform.GetComponent<StageCard>() != null && objectAtDragEnd.transform.GetComponent<StageCard>().transform.parent == PlayerBoard.Instance.stageObject.transform)
                    {
                        if (this.cardData.cost <= PlayerBoard.Instance.activeDon)
                        {
                            Card currentStage = objectAtDragEnd.transform.GetComponent<StageCard>();
                            PlayerBoard.Instance.MoveCardToTrash(currentStage);
                            PlayerBoard.Instance.RestDons(this.cardData.cost);
                            PlayerBoard.Instance.MoveStageFromHandToStageArea(this);
                        }
                        else
                        {
                            SnapCardBackToParentPos();
                            Debug.Log("Cannot play the card!");
                        }
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
                    if ((objectAtDragEnd.transform != PlayerBoard.Instance.characterAreaObject.transform && objectAtDragEnd.transform.GetComponent<CharacterCard>() == null) || objectAtDragEnd.transform.parent == PlayerBoard.Instance.handObject.transform || this.cardData.cost > PlayerBoard.Instance.activeDon || GameManager.Instance.currentPlayerTurnPhase != PlayerTurnPhases.MAINPHASE)
                    {
                        SnapCardBackToParentPos();
                        Debug.Log("Cannot play the card!");
                    }
                    else
                    {
                        bool effectActivated = false;
                        if (this.effects != null)
                        {
                            foreach (Effects effect in effects)
                            {
                                if (effect.triggerType == EffectTriggerTypes.Main)
                                {
                                    PlayerBoard.Instance.RestDons(this.cardData.cost);
                                    effect.cardEffect?.Activate(this);
                                    effectActivated = true;
                                }
                            }
                        }
                        if (!effectActivated)
                        {
                            PlayerBoard.Instance.MoveCardToTrash(this);
                            PlayerBoard.Instance.RestDons(this.cardData.cost);
                        }
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

        public async void AttachDon(Card toCard, Card donCard)
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                toCard.EnableCanvasOverrideSorting();
                Canvas thisCardCanvas = donCard.GetComponent<Canvas>();
                thisCardCanvas.overrideSorting = true;
                thisCardCanvas.sortingOrder = 1;
                toCard.GetComponent<LineRenderer>().sortingOrder = 4;
                donCard.transform.SetParent(toCard.transform);
                donCard.transform.position = donCard.transform.parent.position;
                if (toCard.transform.parent.parent == EnemyBoard.Instance.transform)
                {
                    donCard.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                else
                {
                    donCard.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                donCard.transform.Translate(0, -30, 0);
                donCard.SetCardNotActive();
                donCard.draggable = false;
                donCard.rested = true;
                donCard.UpdateParent();
                toCard.AddToPlusPower(1000);
                toCard.MakeOrUpdatePlusPowerSeenOnCard();
                toCard.ChangeDonAttached(true);
            });
        }

        public async void SendCardToServer()
        {
            if (GameManager.Instance.currentState == GameState.TESTING) { return; }
            await UnityMainThreadDispatcher.RunOnMainThread(async () =>
            {
                await ServerCon.Instance.UpdateCardAtInGameStateDB(this);
                await ServerCon.Instance.UpdateMyCardAtEnemy(this.cardData.customCardID);
            });
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!draggable || GameManager.Instance.currentBattlePhase != BattlePhases.NOBATTLE || PlayerBoard.Instance.effectInProgress || !PlayerBoard.Instance.enemyFinishedStartingHand) { return; }
            this.transform.position = eventData.position;
            //Debug.Log("OnDrag");
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!draggable || GameManager.Instance.currentBattlePhase != BattlePhases.NOBATTLE || PlayerBoard.Instance.effectInProgress || !PlayerBoard.Instance.enemyFinishedStartingHand) { return; }
            //Debug.Log("OnDrop");
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isImgLoaded && this.cardData.cardType != CardType.DON && GameManager.Instance.currentState != GameState.STARTINGPHASE)
            {
                Sprite currentSprite = cardImage.sprite;
                GameBoard.Instance.SetCardMagnifierActiveWithImage(currentSprite);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GameBoard.Instance.HideCardMagnifier();
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
                    if (this.cardData.cardType == CardType.LEADER)
                    {
                        cardImage.sprite = Resources.Load<Sprite>("Cards/Leaders/" + cardData.cardID);
                        isImgLoaded = !isImgLoaded;
                    }
                    else
                    {
                        cardImage.sprite = Resources.Load<Sprite>("Cards/Cards/" + cardData.cardID);
                        isImgLoaded = !isImgLoaded;
                    }

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
            if (this.cardData.effect != "")
            {
                PopulateEffects(this.cardData.effect);
            }
        }
        public void SetCardVisibility(CardVisibility visibility)
        {
            this.cardData.cardVisibility = visibility;
        }

        public void CheckCardVisibility()
        {
            if (cardData.cardVisibility == CardVisibility.PLAYERBOARD && !isImgLoaded && this.transform.parent.parent != EnemyBoard.Instance.transform)
            {
                FlipCard();
            }
            else if (cardData.cardVisibility == CardVisibility.BOTH && !isImgLoaded)
            {
                FlipCard();
            }
        }

        public void CheckCardForDonEffect()
        {
            if (this.effects != null)
            {
                foreach (Effects effect in this.effects)
                {
                    if (effect.triggerType == EffectTriggerTypes.DON)
                    {
                        effect.cardEffect?.Activate(this);
                    }
                }
            }
        }
        public void UpdateParent()
        {
            this.cardData.currentParent = this.transform.parent.name;
        }

        public void UpdateEnemyCardAfterDataLoad()
        {
            GameObject parentObj = EnemyBoard.Instance.GetParentByNameString(this.cardData.currentParent);
            Card parentCard = parentObj.GetComponent<Card>();
            if (this.GetComponent<DonCard>() != null && parentCard != null)
            {
                AttachDon(parentCard, this);
            }
            else
            {
                this.transform.SetParent(parentObj.transform);
                this.transform.position = this.transform.parent.position;
                this.draggable = false;
                if (this.transform.parent != EnemyBoard.Instance.lifeObject.transform)
                {
                    this.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                if (this.GetComponent<DonCard>() != null && this.transform.parent != EnemyBoard.Instance.donDeckObject.transform)
                {
                    if (this.cardData.active)
                    {
                        this.GetComponent<DonCard>().EnemyRestandDon();
                        this.ResetCanvasOverrideSorting();
                    }
                    else
                    {
                        this.GetComponent<DonCard>().EnemyRestDon();
                        this.ResetCanvasOverrideSorting();
                    }
                }
                if (this.transform.parent == EnemyBoard.Instance.trashObject.transform)
                {
                    if (!isImgLoaded)
                    {
                        this.FlipCard();
                    }
                }
                else
                {
                    if (this.cardData.active)
                    {
                        this.Restand(true, false);
                    }
                    else
                    {
                        this.Rest();
                    }
                }
            }
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
                else if (this.GetComponent<LeaderCard>() != null)
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

        public void Restand(bool active, bool canAttack)
        {
            if (rested && this.transform.parent.parent == EnemyBoard.Instance.transform)
            {
                this.cardData.active = active;
                this.canAttack = canAttack;
                this.transform.rotation = Quaternion.Euler(0, 0, 180);
                this.rested = false;
            }
            else if (rested && this.transform.parent.parent == PlayerBoard.Instance.transform)
            {
                this.cardData.active = active;
                this.canAttack = canAttack;
                this.transform.rotation = Quaternion.Euler(0, 0, 0);
                this.rested = false;
            }
        }

        public async void MakeBorderForThisCard(Color colorOfBorder, string borderType)
        {
            if (border != null)
            {
                RemoveBorderForThisCard();
            }
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                switch (borderType)
                {
                    case "attack":
                        border = new GameObject("Outline");
                        border.transform.SetParent(this.gameObject.transform);
                        Image attackborderIMG = border.AddComponent<Image>();
                        attackborderIMG.sprite = Resources.Load<Sprite>("BoardImages/attackArrow");
                        RectTransform attackRectTransform = border.GetComponent<RectTransform>();
                        border.transform.position = this.transform.position;
                        border.transform.Translate(0, 10, 0);
                        if (this.rested)
                        {
                            border.transform.rotation = Quaternion.Euler(0, 0, 90);
                        }
                        else
                        {
                            border.transform.rotation = Quaternion.Euler(0, 0, 0);
                        }
                        attackRectTransform.sizeDelta = new Vector2(150, 200);
                        attackborderIMG.color = Color.red;
                        Canvas attackcanvas = border.AddComponent<Canvas>();
                        attackcanvas.overrideSorting = true;
                        attackcanvas.sortingOrder = 2;
                        EnableCanvasOverrideSorting();
                        break;
                    case "block":
                        border = new GameObject("Outline");
                        border.transform.SetParent(this.gameObject.transform);
                        Image blockerborderIMG = border.AddComponent<Image>();
                        blockerborderIMG.sprite = Resources.Load<Sprite>("BoardImages/blockRect");
                        RectTransform blockerRectTransform = border.GetComponent<RectTransform>();
                        border.transform.position = this.transform.position;
                        if (this.rested)
                        {
                            border.transform.rotation = Quaternion.Euler(0, 0, 90);
                        }
                        else
                        {
                            border.transform.rotation = Quaternion.Euler(0, 0, 0);
                        }
                        blockerRectTransform.sizeDelta = new Vector2(150, 180);
                        blockerborderIMG.color = Color.yellow;
                        Canvas blockercanvas = border.AddComponent<Canvas>();
                        blockercanvas.overrideSorting = true;
                        blockercanvas.sortingOrder = 2;
                        EnableCanvasOverrideSorting();
                        break;
                    default:
                        border = new GameObject("Outline");
                        border.transform.SetParent(this.gameObject.transform);
                        Image borderIMG = border.AddComponent<Image>();
                        RectTransform rectTransform = border.GetComponent<RectTransform>();
                        border.transform.position = this.transform.position;
                        if (this.rested)
                        {
                            border.transform.rotation = Quaternion.Euler(0, 0, 90);
                        }
                        else
                        {
                            border.transform.rotation = Quaternion.Euler(0, 0, 0);
                        }
                        rectTransform.sizeDelta = new Vector2(110, 150);
                        borderIMG.color = colorOfBorder;
                        Outline borderOutline = border.AddComponent<Outline>();
                        borderOutline.enabled = true;
                        borderOutline.effectColor = colorOfBorder;
                        borderOutline.effectDistance = new Vector2(5, 5);
                        Canvas canvas = border.AddComponent<Canvas>();
                        canvas.overrideSorting = true;
                        canvas.sortingOrder = 2;
                        EnableCanvasOverrideSorting();
                        break;
                }
            });
        }

        public async void RemoveBorderForThisCard()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                if (border != null)
                {
                    Destroy(border);
                    Debug.Log("Destroyed border on this card: " + this.cardData.customCardID);
                }
                ResetCanvasOverrideSorting();
            });
        }

        public async void ResetCanvasOverrideSorting()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                if (this != null && (!hasDonAttached || GameManager.Instance.currentState == GameState.MATCHWON || GameManager.Instance.currentState == GameState.MATCHLOST))
                {
                    Canvas thisCardCanvas = this.gameObject.GetComponent<Canvas>();
                    thisCardCanvas.overrideSorting = false;
                }
            });

        }

        public async void EnableCanvasOverrideSorting()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                if (this != null)
                {
                    Canvas thisCardCanvas = this.gameObject.GetComponent<Canvas>();
                    thisCardCanvas.overrideSorting = true;
                    thisCardCanvas.sortingOrder = 3;
                }
            });
        }

        public async void EnableCanvasOverrideSortingOnDraggingCard()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                if (this != null)
                {
                    Canvas thisCardCanvas = this.gameObject.GetComponent<Canvas>();
                    thisCardCanvas.overrideSorting = true;
                    thisCardCanvas.sortingOrder = 5;
                }
            });
        }

        public void SetAttacked(Card card)
        {
            this.attacked = card;
        }

        public void SetClickAction(Action<Card> onClick)
        {
            onCardClicked = onClick;
        }

        public void ClearClickAction()
        {
            onCardClicked = null;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (onCardClicked != null)
            {
                onCardClicked.Invoke(this);
                return;
            }
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (attacked != null && GameManager.Instance.currentBattlePhase == BattlePhases.COUNTERSTEP && this.cardData.cardType != CardType.EVENT)
                {
                    this.CardClickedWithLeftMouseForCountering?.Invoke(attacked, this);
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (this.effects != null)
                {
                    foreach (Effects effect in this.effects)
                    {
                        if (effect.triggerType == EffectTriggerTypes.ActivateMain)
                        {
                            effect.cardEffect?.Activate(this);
                        }
                    }
                }
            }
        }

        public async void MakeOrUpdatePlusPowerSeenOnCard()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                if (powerText == null)
                {

                    powerText = new GameObject("PowerTextContainer");
                    powerText.transform.SetParent(this.gameObject.transform);
                    Image backgroundImage = powerText.AddComponent<Image>();
                    backgroundImage.color = Color.white;
                    RectTransform containerRect = powerText.GetComponent<RectTransform>();
                    containerRect.sizeDelta = new Vector2(100, 30);
                    powerText.transform.position = powerText.transform.parent.position;
                    GameObject text = new GameObject("PowerText");
                    text.transform.SetParent(powerText.transform);
                    TextMeshProUGUI textMeshProUGUI = text.AddComponent<TextMeshProUGUI>();
                    textMeshProUGUI.text = "+" + plusPower;
                    RectTransform textRect = text.GetComponent<RectTransform>();
                    textRect.sizeDelta = new Vector2(100, 30);
                    textMeshProUGUI.enableAutoSizing = true;
                    textMeshProUGUI.color = Color.black;
                    textMeshProUGUI.horizontalAlignment = HorizontalAlignmentOptions.Center;
                    textMeshProUGUI.verticalAlignment = VerticalAlignmentOptions.Middle;
                    text.transform.position = text.transform.parent.position;
                    Canvas canvas = powerText.AddComponent<Canvas>();
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 4;
                    EnableCanvasOverrideSorting();
                }
                else
                {
                    powerText.transform.GetComponentInChildren<TextMeshProUGUI>().text = "+" + plusPower;
                }
            });
        }

        public async void HidePlusPowerOnCard()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                if (powerText != null)
                {
                    Destroy(powerText);
                }
            });
        }

        public void AddToPlusPower(int power)
        {
            plusPower = plusPower + power;
        }

        public void ResetPlusPower()
        {
            plusPower = 0;
        }

        public void ChangeDonAttached(bool b)
        {
            hasDonAttached = b;
        }

        public void ResetAttackedCardAfterCounter()
        {
            this.attacked.ResetPlusPower();
            this.attacked.HidePlusPowerOnCard();
        }

        public void SetRested(bool rested)
        {
            this.rested = rested;
        }

        public int GetAttachedDonCount()
        {
            if (this == null) { return -1; }
            int donCounter = 0;

            for (int i = 0; i < this.transform.childCount; i++)
            {
                DonCard possibleDon = this.transform.GetChild(i).gameObject.GetComponent<DonCard>();
                if (possibleDon != null)
                {
                    donCounter++;
                }
            }

            return donCounter;
        }

        public void IsTargetForEffect(bool target)
        {
            this.targetForEffect = target;
            if (target)
            {
                this.MakeBorderForThisCard(Color.blue, "target");
            }
            else
            {
                this.RemoveBorderForThisCard();
            }
        }

        public void PopulateEffects(string cardEffects)
        {
            this.effects = StringToEffectsParser.Parser(cardEffects);
        }

        public void SetCurrentlyAttacking(bool attacking)
        {
            this.currentlyAttacking = attacking;
        }

        public void AddWhenAttackingEffectToCard(Effects effect)
        {
            this.effects.Add(effect);
            whenAttackingEffectsAddedByOtherCards.Add(effect);
        }
        protected void CheckAddedWhenAttackingEffects()
        {
            if (whenAttackingEffectsAddedByOtherCards.Count > 0 && GameManager.Instance.currentState == GameState.ENEMYPHASE)
            {
                foreach (Effects effect in whenAttackingEffectsAddedByOtherCards)
                {
                    effects.Remove(effect);
                }
                whenAttackingEffectsAddedByOtherCards = new List<Effects>();
            }
        }

    }
}
