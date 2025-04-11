using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace TCGSim
{
    public class PlayerBoard : Board
    {
        public static PlayerBoard Instance { get; private set; }
        public LeaderCard leaderCard { get; private set; }

        private bool firstRound = true;
        private bool havingBlockReq = false;
        private bool overThisBlocking = false;
        private int blockPowerReq = -1;
        public bool effectInProgress { get; private set; }
        public Card currentlyAttackedCard { get; private set; }

        //These are needed to be able to make room for cards to place on character area if its full
        private Card cardToMoveCharArea;
        private List<Card> possibleTargetsForRemovinToMakeRoom;
        private List<Card> cardsThatCouldAttackBeforeRemove;
        public bool enemyFinishedStartingHand { get; private set; } = false;


        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (deckObject != null)
            {
                //Debug.Log(boardName + "- In hand:" + handObject.transform.childCount + ", in deck: " + deckObject.transform.childCount + ", in life: " + lifeObject.transform.childCount);
            }
            Debug.Log("Active dons: " + activeDon);
            if (costAreaObject != null)
            {
                activeDon = costAreaObject.GetActiveDonCount();
            }
        }
        private void Awake()
        {
            if (Instance == null || Instance == this)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
        }
        private void OnDestroy()
        {
            GameManager.OnGameStateChange -= GameManagerOnGameStateChange;
            GameManager.OnPlayerTurnPhaseChange -= GameManagerOnPlayerTurnPhaseChange;
            GameManager.OnBattlePhaseChange -= GameManagerOnBattlePhaseChange;
            Card.CorrectAmountCardsDrawn -= CorrectAmountOfCardDrawn;
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public override void Init(string boardName, string gameCustomID, string playerName)
        {
            this.boardName = boardName;
            this.gameCustomID = gameCustomID;
            this.playerName = playerName;
            Debug.Log(boardName + " called init " + playerName);
            GameManager.OnGameStateChange += GameManagerOnGameStateChange;
            GameManager.OnPlayerTurnPhaseChange += GameManagerOnPlayerTurnPhaseChange;
            GameManager.OnBattlePhaseChange += GameManagerOnBattlePhaseChange;
        }

        public override void LoadBoardElements()
        {
            base.LoadBoardElements();
            CreateADeck();
        }


        public void LoadMulliganKeepButtons()
        {
            keepBtn = Instantiate(keepBtnPrefab, this.transform).GetComponent<Button>();
            mulliganBtn = Instantiate(mulliganBtnPrefab, this.transform).GetComponent<Button>();
            if (keepBtn != null)
            {
                keepBtn.onClick.AddListener(KeepHand);
            }
            if (mulliganBtn != null)
            {
                mulliganBtn.onClick.AddListener(Mulligan);
            }
        }
        public void CreateGameButtons()
        {
            endOfTurnBtn = Instantiate(endOfTurnBtnPrefab, this.gameObject.transform).GetComponent<Button>();
            noBlockBtn = Instantiate(noBlockBtnPrefab, this.gameObject.transform).GetComponent<Button>();
            noMoreCounterBtn = Instantiate(noMoreCounterBtnPrefab, this.gameObject.transform).GetComponent<Button>();
            cancelBtn = Instantiate(cancelBtnPrefab, this.gameObject.transform).GetComponent<Button>();
            endOfTurnBtn.gameObject.SetActive(false);
            noBlockBtn.gameObject.SetActive(false);
            noMoreCounterBtn.gameObject.SetActive(false);
            cancelBtn.gameObject.SetActive(false);
            if (endOfTurnBtn != null)
            {
                endOfTurnBtn.onClick.AddListener(EndOfTurnTrigger);
            }
        }
        public void CreateADeck()
        {
            deckString = new List<string>
            {
            "2xST01-011",
            "1xST01-001",
            "2xST01-014",
            "4xST01-002",
            "4xST01-003",
            "4xST01-004",
            "4xST01-005",
            "4xST01-007",
            "4xST01-008",
            "4xST01-009",
            "4xST01-010",
            "2xST01-015",
            "2xST01-016",
            "2xST01-017",
            "2xST01-013",
            "4xST01-006",
            "2xST01-012"
            };
        }

        public Transform getPlayerHand()
        {
            return playerHand;
        }

        public async Task<List<Card>> CreateCardsFromDeck()
        {
            List<Card> deck = new List<Card>();
            foreach (string sameCards in deckString)
            {
                string cardNumber = sameCards.Split("x")[1];
                int count = Convert.ToInt32(sameCards.Split("x")[0]);
                for (int i = 0; i < count; i++)
                {
                    CardData cardData = await ServerCon.Instance.GetCardByCardID(cardNumber);
                    UnityMainThreadDispatcher.Enqueue(() =>
                    {
                        GameObject cardObj = null;
                        Card card = null;
                        switch (cardData.cardType)
                        {
                            case CardType.CHARACTER:
                                cardObj = Instantiate(cardPrefab, deckObject.transform);
                                cardObj.AddComponent<CharacterCard>();
                                card = cardObj.GetComponent<CharacterCard>();
                                break;
                            case CardType.STAGE:
                                cardObj = Instantiate(cardPrefab, deckObject.transform);
                                cardObj.AddComponent<StageCard>();
                                card = cardObj.GetComponent<StageCard>();
                                break;
                            case CardType.EVENT:
                                cardObj = Instantiate(cardPrefab, deckObject.transform);
                                cardObj.AddComponent<EventCard>();
                                card = cardObj.GetComponent<EventCard>();
                                break;
                            case CardType.LEADER:
                                cardObj = Instantiate(cardPrefab, leaderObject.transform);
                                cardObj.AddComponent<LeaderCard>();
                                card = cardObj.GetComponent<LeaderCard>();
                                break;
                            default:
                                cardObj = Instantiate(cardPrefab, deckObject.transform);
                                cardObj.AddComponent<Card>();
                                card = cardObj.GetComponent<Card>();
                                break;
                        }
                        card.LoadDataFromCardData(cardData);
                        card.Init(handObject, cardNumber + "-" + i);
                        card.SetCardActive();
                        if (card.cardData.cardType == CardType.LEADER)
                        {
                            card.cardData.cardVisibility = CardVisibility.BOTH;
                            card.UpdateParent();
                            SendCardToDB(card);
                            leaderCard = card.GetComponent<LeaderCard>();
                        }
                        else
                        {
                            deck.Add(card);
                        }
                    });
                }
            }
            return deck;
        }

        public void enableDraggingOnTopDeckCard()
        {
            if (deckObject.transform.childCount > 0)
            {
                Card topCard = deckObject.transform.GetChild(deckObject.transform.childCount - 1).GetComponent<Card>();
                topCard.ChangeDraggable(true);
            }
        }

        public void AddCardToHandFromDeck(Card card, bool turnOffDraggable, bool sendToServer)
        {
            this.handObject.AddCardToHand(card);
            deckCards.Remove(card);
            card.UpdateParent();
            if (turnOffDraggable)
            {
                card.ChangeDraggable(false);
            }
            if (sendToServer)
            {
                card.SendCardToServer();
            }
        }

        public void AddCardToHandFromLife(Card card, bool turnOffDraggable, bool sendToServer)
        {
            this.handObject.AddCardToHand(card);
            lifeObject.TakeLife(card);
            card.UpdateParent();
            if (turnOffDraggable)
            {
                card.ChangeDraggable(false);
            }
            if (sendToServer)
            {
                card.SendCardToServer();
            }
        }


        public void PutCardBackToDeck(Card card)
        {
            handObject.RemoveCardFromHand(card, this.deckObject.transform);
            card.SetCardVisibility(CardResources.CardVisibility.NONE);
            card.transform.position = this.deckObject.transform.position;
            card.FlipCard();
            deckCards.Add(card);
        }
        public void MoveCardFromHandToCharacterArea(Card card)
        {
            card.SetCardActive();
            card.SetCardVisibility(CardResources.CardVisibility.BOTH);
            handObject.RemoveCardFromHand(card, this.characterAreaObject.transform);
            characterAreaCards.Add(card);
            card.ChangeDraggable(false);
            card.UpdateParent();
            card.SendCardToServer();
            CheckForEffect(card);
        }

        private async void CheckForEffect(Card card)
        {
            if (card.effects != null)
            {
                foreach (Effects effect in card.effects)
                {
                    if (effect.triggerType == EffectTriggerTypes.OnPlay)
                    {
                        await UnityMainThreadDispatcher.RunOnMainThread(() =>
                        {
                            effect.cardEffect?.Activate(card);
                        });
                    }
                    if (effect.triggerType == EffectTriggerTypes.Rush)
                    {
                        await UnityMainThreadDispatcher.RunOnMainThread(() =>
                        {
                            effect.cardEffect?.Activate(card);
                        });
                    }
                }
            }
        }

        public void MoveDonFromDeckToCostArea(Card card)
        {
            card.ResetCanvasOverrideSorting();
            if (!donInCostArea.Contains(card))
            {
                card.SetCardVisibility(CardResources.CardVisibility.BOTH);
                donCardsInDeck.Remove(card);
                donInCostArea.Add(card);
                card.transform.SetParent(this.costAreaObject.transform);
                card.SetCardActive();
                card.ChangeDraggable(true);
                card.UpdateParent();
                card.SendCardToServer();
            }
            else
            {
                card.SetCardVisibility(CardResources.CardVisibility.BOTH);
                card.transform.SetParent(this.costAreaObject.transform);
                card.SetCardActive();
                card.ChangeDraggable(true);
                card.UpdateParent();
                card.SendCardToServer();
            }
        }
        public void MoveStageFromHandToStageArea(Card card)
        {
            card.SetCardVisibility(CardResources.CardVisibility.BOTH);
            handObject.RemoveCardFromHand(card, this.stageObject.transform);
            card.transform.position = card.transform.parent.position;
            card.SetCardActive();
            card.ChangeDraggable(false);
            card.UpdateParent();
            card.SendCardToServer();
        }
        public void MoveCardToTrash(Card card)
        {
            if (characterAreaCards.Contains(card)) { characterAreaCards.Remove(card); }
            if (handObject.hand.Contains(card)) { handObject.RemoveCardFromHand(card, this.trashObject.transform); }
            else
            {
                card.transform.SetParent(this.trashObject.transform);
            }
            card.RemoveBorderForThisCard();
            card.HidePlusPowerOnCard();
            card.SetCardNotActive();
            card.ChangeDraggable(false);
            card.UpdateParent();
            card.transform.position = card.transform.parent.position;
            card.transform.rotation = Quaternion.Euler(0, 0, 0);
            card.SendCardToServer();
        }

        public void CreateStartingHand()
        {
            for (int i = 0; i < 5; i++)
            {
                AddCardToHandFromDeck(deckCards[i], true, false);
            }
        }

        public async void Mulligan()
        {
            handObject.ScaleHandBackFromStartingHand();
            List<Card> cardsInHandCurrently = handObject.hand.ToList();
            foreach (Card card in cardsInHandCurrently)
            {
                PutCardBackToDeck(card);
            }
            //Shuffle<Card>(deckCards);
            ReassingOrderAfterShuffle();
            CreateStartingHand();
            keepBtn.gameObject.SetActive(false);
            mulliganBtn.gameObject.SetActive(false);
            CreateStartingLife();
            CreateGameButtons();
            await SendAllCardToDB();
            await ServerCon.Instance.DoneWithMulliganOrKeep();
        }

        public async void KeepHand()
        {
            handObject.ScaleHandBackFromStartingHand();
            keepBtn.gameObject.SetActive(false);
            mulliganBtn.gameObject.SetActive(false);
            CreateStartingLife();
            CreateGameButtons();
            await SendAllCardToDB();
            await ServerCon.Instance.DoneWithMulliganOrKeep();
        }

        public void Shuffle<T>(IList<T> list)
        {
            int listSize = list.Count;
            System.Random random = new System.Random();
            for (int x = 0; x < 1000; x++)
            {
                for (int i = listSize - 1; i >= 1; i--)
                {
                    int j = random.Next(0, listSize);
                    (list[i], list[j]) = (list[j], list[i]);
                }
            }
        }

        public void ReassingOrderAfterShuffle()
        {
            for (int i = 0; i < deckCards.Count; i++)
            {
                deckCards[i].transform.SetSiblingIndex(i);
            }
        }

        public void AddCardToLife(Card card)
        {
            this.lifeObject.AddCardToLife(card);
            deckCards.Remove(card);
        }

        public void CreateStartingLife()
        {
            if (leaderCard == null) { return; }
            for (int i = 0; i < leaderCard.life; i++)
            {
                AddCardToLife(deckCards[i]);
            }
        }

        public async void SendCardToDB(Card card)
        {
            await ServerCon.Instance.AddCardToInGameStateDB(card);
        }

        public async Task SendAllCardToDB()
        {
            foreach (Card card in deckCards)
            {
                card.UpdateParent();
                await ServerCon.Instance.AddCardToInGameStateDB(card);
            }
            foreach (Card card in handObject.hand)
            {
                card.UpdateParent();
                card.SetCardVisibility(CardVisibility.NONE);
                await ServerCon.Instance.AddCardToInGameStateDB(card);
            }
            foreach (Card card in lifeObject.lifeCards)
            {
                card.UpdateParent();
                await ServerCon.Instance.AddCardToInGameStateDB(card);
            }
            foreach (Card card in donCardsInDeck)
            {
                card.UpdateParent();
                await ServerCon.Instance.AddCardToInGameStateDB(card);
            }
        }

        public void RestDons(int donCountToRest)
        {
            costAreaObject.RestDons(donCountToRest);
        }

        public void EndOfTurnTrigger()
        {
            GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.ENDPHASE);
        }
        public void NoBlockTrigger(Card attacker, Card attacked)
        {
            blockPowerReq = -1;
            havingBlockReq = false;
            overThisBlocking = false;
            GameManager.Instance.ChangeBattlePhase(BattlePhases.COUNTERSTEP, attacker, attacked);
        }

        public async void NoMoreCounterTrigger(Card attacker, Card attacked)
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                foreach (Card cardInHand in handObject.hand)
                {
                    if (cardInHand.effects != null)
                    {
                        foreach (Effects effect in cardInHand.effects)
                        {
                            if (effect.triggerType == EffectTriggerTypes.Counter)
                            {
                                cardInHand.IsTargetForEffect(false);
                                cardInHand.ClearClickAction();
                                cardInHand.RemoveBorderForThisCard();
                            }
                        }
                    }
                }
                GameManager.Instance.ChangeBattlePhase(BattlePhases.DAMAGESTEP, attacker, attacked);
            });
        }

        public override void GameManagerOnGameStateChange(GameState state)
        {
            switch (state)
            {
                case GameState.STARTINGPHASE:
                    HandleStartingPhase();
                    break;
                case GameState.PLAYERPHASE:
                    HandlePlayerPhase();
                    break;
                case GameState.ENEMYPHASE:
                    HandleEnemyPhase();
                    break;
                case GameState.MATCHLOST:
                    HandleMatchLost();
                    break;
                case GameState.MATCHWON:
                    HandleMatchWon();
                    break;
                default:
                    break;
            }
        }
        private async void HandleStartingPhase()
        {

            this.LoadBoardElements();
            //Shuffle<string>(deckString);
            deckCards = await CreateCardsFromDeck();
            UnityMainThreadDispatcher.Enqueue(async () =>
            {
                //Shuffle<Card>(deckCards);
                ReassingOrderAfterShuffle();
                Debug.Log(boardName);
                CreateStartingHand();
                if (boardName == "PLAYERBOARD")
                {
                    handObject.ScaleHandForStartingHand();
                    LoadMulliganKeepButtons();
                    if (leaderCard != null)
                    {
                        await ServerCon.Instance.UpdateMyLeaderCardAtEnemy(leaderCard.cardData.customCardID);

                    }
                }
                Card.CorrectAmountCardsDrawn += CorrectAmountOfCardDrawn;
            });

        }
        private async void HandlePlayerPhase()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                DisableEnemyDonPlusEffectInMyTurn();
                GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.REFRESHPHASE);
            });

        }

        public void DisableEnemyDonPlusEffectInMyTurn()
        {
            LeaderCard enemyLeader = EnemyBoard.Instance.leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
            enemyLeader.ResetPlusPower();
            enemyLeader.HidePlusPowerOnCard();
            for (int i = 0; i < EnemyBoard.Instance.characterAreaObject.transform.childCount; i++)
            {
                Card card = EnemyBoard.Instance.characterAreaObject.transform.GetChild(i).gameObject.GetComponent<Card>();
                if (card != null)
                {
                    card.ResetPlusPower();
                    card.HidePlusPowerOnCard();
                }
            }
        }
        private void HandleEnemyPhase()
        {
            foreach (DonCard donCard in donInCostArea)
            {
                Card donParentCard = donCard.transform.parent.GetComponent<Card>();
                if (donParentCard != null)
                {
                    donParentCard.ResetPlusPower();
                    donParentCard.HidePlusPowerOnCard();
                }
            }
            ChatManager.Instance.AddMessage("Enemy turn!");
        }
        private async void HandleMatchLost()
        {
            ChatManager.Instance.AddMessage("You lost the match!");
            await ServerCon.Instance.EnemyWon();
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                GameBoard.Instance.GameLost();
            });
        }
        private async void HandleMatchWon()
        {
            ChatManager.Instance.AddMessage("You won the game!");
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                GameBoard.Instance.GameWon();
            });

        }


        public override void GameManagerOnPlayerTurnPhaseChange(PlayerTurnPhases turnPhase)
        {
            switch (turnPhase)
            {
                case PlayerTurnPhases.REFRESHPHASE:
                    HandleRefreshPhase();
                    break;
                case PlayerTurnPhases.DRAWPHASE:
                    HandleDrawPhase();
                    break;
                case PlayerTurnPhases.DONPHASE:
                    HandleDONPhase();
                    break;
                case PlayerTurnPhases.MAINPHASE:
                    HandleMainPhase();
                    break;
                case PlayerTurnPhases.ENDPHASE:
                    HandleEndPhase();
                    break;
                default:
                    break;
            }
        }

        private void HandleRefreshPhase()
        {
            foreach (DonCard donCard in donCardsInDeck)
            {
                if (!(donCard.transform.parent != PlayerBoard.Instance.costAreaObject.transform || donCard.transform.parent != PlayerBoard.Instance.donDeckObject.transform))
                {
                    donCard.transform.SetParent(PlayerBoard.Instance.costAreaObject.transform);
                    donCard.UpdateParent();
                    donCard.SetCardActive();
                    donCard.ChangeDraggable(true);
                }
            }
            foreach (DonCard donCard in donInCostArea)
            {
                if (donCard.transform.parent == PlayerBoard.Instance.costAreaObject.transform)
                {
                    donCard.SetCardActive();
                    donCard.ChangeDraggable(true);
                    donCard.RestandDon();
                    donCard.SendCardToServer();
                }
                else
                {
                    Card donParentCard = donCard.transform.parent.GetComponent<Card>();
                    if (donParentCard != null)
                    {
                        donParentCard.ResetPlusPower();
                        donParentCard.HidePlusPowerOnCard();
                        MoveDonFromDeckToCostArea(donCard);
                        donParentCard.ChangeDonAttached(false);
                        donCard.RestandDon();
                        donCard.ResetCanvasOverrideSorting();
                        donCard.SendCardToServer();
                    }
                }
            }
            foreach (CharacterCard card in characterAreaCards)
            {
                if (card.rested)
                {
                    card.Restand(true, false);
                    card.SendCardToServer();
                }
            }
            if (leaderCard.rested)
            {
                leaderCard.Restand(true, false);
                leaderCard.SendCardToServer();
            }
            if (stageObject.transform.childCount != 0)
            {
                StageCard stageCard = stageObject.transform.GetChild(0).GetComponent<StageCard>();
                if (stageCard.rested)
                {
                    stageCard.Restand(true, false);
                    stageCard.SendCardToServer();
                }
            }
            GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.DRAWPHASE);
        }
        private void HandleDrawPhase()
        {
            if (!firstRound || !ServerCon.Instance.firstTurnIsMine)
            {
                ChatManager.Instance.AddMessage("Please draw 1 card from your deck!");
                enableDraggingOnTopDeckCard();
                Card.SetHowManyCardNeedsToBeDrawn(1);
                Card.NeedToWatchHowManyCardsDrawn(true, deckObject.transform);
            }
            else
            {
                GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.DONPHASE);
            }
        }
        private async void HandleDONPhase()
        {
            if (deckCards.Count == 0)
            {
                await ServerCon.Instance.EnemyWon();
                ChatManager.Instance.AddMessage("Your deck count has reached zero! You lost!");
                GameBoard.Instance.GameLost();
            }
            if (this.activeDon == 10)
            {
                ChatManager.Instance.AddMessage("You alrady drawn all of your DON!! card. Proceed to your main phase!");
                GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
            }
            else
            {
                if ((!firstRound || !ServerCon.Instance.firstTurnIsMine) && this.activeDon != 9)
                {
                    ChatManager.Instance.AddMessage("Please draw 2 DON!! card!");
                    enableDraggingOnTopTwoDonCard();
                    Card.SetHowManyCardNeedsToBeDrawn(2);
                    Card.NeedToWatchHowManyCardsDrawn(true, donDeckObject.transform);
                }
                else
                {
                    ChatManager.Instance.AddMessage("Please draw 1 DON!! card!");
                    enableDraggingOnTopDonCard();
                    Card.SetHowManyCardNeedsToBeDrawn(1);
                    Card.NeedToWatchHowManyCardsDrawn(true, donDeckObject.transform);
                }
            }
        }
        private void HandleMainPhase()
        {

            endOfTurnBtn.gameObject.SetActive(true);
            MakeLeaderAttackActive();
            MakeHandCardsDraggable();
            ActivateCharacterAreaCards();

        }
        private async void HandleEndPhase()
        {
            firstRound = false;
            DeactivateDraggableHandCards();
            DeactivateAttackOnLeader();
            DeactivateAttackOnCharacterAreaCards();
            TurnOffDraggableOnAllDon();
            await ServerCon.Instance.ChangeEnemyGameStateToPlayerPhase(gameCustomID);
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                GameManager.Instance.ChangeGameState(GameState.ENEMYPHASE);
                endOfTurnBtn.gameObject.SetActive(false);
            });
        }

        public override void GameManagerOnBattlePhaseChange(BattlePhases battlePhase, Card attacker, Card attacked)
        {
            switch (battlePhase)
            {
                case BattlePhases.ATTACKDECLARATION:
                    HandleAttackDeclaration(attacker, attacked);
                    break;
                case BattlePhases.BLOCKSTEP:
                    HandleBlockStep(attacker, attacked);
                    break;
                case BattlePhases.COUNTERSTEP:
                    HandleCounterStep(attacker, attacked);
                    break;
                case BattlePhases.DAMAGESTEP:
                    HandleDamageStep(attacker, attacked);
                    break;
                case BattlePhases.ENDOFBATTLE:
                    HandleEndOfBattleStep();
                    break;
                case BattlePhases.NOBATTLE:
                    HandleNoBattle();
                    break;
                default:
                    break;
            }
        }

        private async void HandleAttackDeclaration(Card attacker, Card attacked)
        {
            await UnityMainThreadDispatcher.RunOnMainThread(async () =>
            {
                string attackerID = attacker.cardData.customCardID;
                string attackedID = attacked.cardData.customCardID;
                bool thereIsWhenAttacking = false;
                ChatManager.Instance.AddMessage("You attacked the following card: " + attackedID + " with the following card: " + attackerID);
                attacker.Rest();
                attacker.SetCurrentlyAttacking(true);
                attacker.SendCardToServer();
                endOfTurnBtn.gameObject.SetActive(false);
                RemoveOtherCardsBorderForBattle();
                if (attacker.effects != null)
                {
                    foreach (Effects effect in attacker.effects)
                    {
                        if (effect.triggerType == EffectTriggerTypes.WhenAttacking)
                        {
                            effect.cardEffect?.Activate(attacker);
                            thereIsWhenAttacking = true;
                        }
                    }
                }
                await ServerCon.Instance.AttackedEnemyCard(attackerID, attackedID, attacker.cardData.power + attacker.plusPower, thereIsWhenAttacking);
            });
        }

        public void RemoveOtherCardsBorderForBattle()
        {
            leaderCard.RemoveBorderForThisCard();
            foreach (Card card in characterAreaCards)
            {
                card.RemoveBorderForThisCard();
            }
        }
        private void HandleBlockStep(Card attacker, Card attacked)
        {
            currentlyAttackedCard = attacked;
            string attackerID = attacker.cardData.customCardID;
            string attackedID = attacked.cardData.customCardID;
            ChatManager.Instance.AddMessage("The enemy attacked your card: " + attackedID + " with the following card: " + attackerID);
            ChatManager.Instance.AddMessage("Select a blocker or click on no block button!");
            List<CharacterCard> useableBlockers = CheckForBlockers();
            MakeBlockersActiveForBlocking(useableBlockers, attacker);
            if (noBlockBtn != null)
            {
                noBlockBtn.onClick.AddListener(() => NoBlockTrigger(attacker, attacked));
            }
            endOfTurnBtn.gameObject.SetActive(false);
            noBlockBtn.gameObject.SetActive(true);
            switch (attacker.cardData.cardType)
            {
                case CardType.CHARACTER:
                    attacker.GetComponent<CharacterCard>().DrawAttackLine(attacked);
                    break;
                case CardType.LEADER:
                    attacker.GetComponent<LeaderCard>().DrawAttackLine(attacked);
                    break;
                default:
                    break;
            }

        }

        private List<CharacterCard> CheckForBlockers()
        {
            List<CharacterCard> activeBlockersOnField = new List<CharacterCard>();
            foreach (CharacterCard card in characterAreaCards)
            {
                if (card.cardData.effect.Contains("[Blocker] (After your opponent declares an attack, you may rest this card to make it the new target of the attack.)") && !card.rested)
                {
                    if (havingBlockReq && blockPowerReq == -1)
                    {
                        break;
                    }
                    else if (havingBlockReq && overThisBlocking)
                    {
                        if (card.cardData.power < blockPowerReq)
                        {
                            activeBlockersOnField.Add(card);
                        }
                    }
                    else if (havingBlockReq && !overThisBlocking)
                    {
                        if (card.cardData.power > blockPowerReq)
                        {
                            activeBlockersOnField.Add(card);
                        }
                    }
                    else
                    {
                        activeBlockersOnField.Add(card);
                    }
                }
            }
            return activeBlockersOnField;
        }

        private void MakeBlockersActiveForBlocking(List<CharacterCard> useableBlockers, Card attacker)
        {
            foreach (CharacterCard card in useableBlockers)
            {
                card.SetAttacker(attacker);
                card.CardClickedWithLeftMouseForBlocking += BlockerCard_CardClickedWithLeftMouse;
                card.MakeBorderForThisCard(Color.yellow,"block");
            }
        }

        private void DectiveBlockersForBlocking(List<CharacterCard> useableBlockers)
        {
            foreach (CharacterCard card in useableBlockers)
            {
                card.CardClickedWithLeftMouseForBlocking -= BlockerCard_CardClickedWithLeftMouse;
                card.RemoveBorderForThisCard();
            }
        }

        private void BlockerCard_CardClickedWithLeftMouse(Card attacker, Card blocker)
        {
            blocker.Rest();
            blocker.SendCardToServer();
            blocker.GetComponent<CharacterCard>().CardClickedWithLeftMouseForBlocking -= BlockerCard_CardClickedWithLeftMouse;
            blocker.GetComponent<CharacterCard>().RemoveBorderForThisCard();
            switch (attacker.cardData.cardType)
            {
                case CardType.CHARACTER:
                    attacker.GetComponent<CharacterCard>().RemoveAttackLine();
                    attacker.GetComponent<CharacterCard>().DrawAttackLine(blocker);
                    break;
                case CardType.LEADER:
                    attacker.GetComponent<LeaderCard>().RemoveAttackLine();
                    attacker.GetComponent<LeaderCard>().DrawAttackLine(blocker);
                    break;
                default:
                    break;
            }
            GameManager.Instance.ChangeBattlePhase(BattlePhases.COUNTERSTEP, attacker, blocker);
        }

        private void HandleCounterStep(Card attacker, Card attacked)
        {
            this.blockPowerReq = -1;
            this.havingBlockReq = false;
            foreach(Card cardInHand in handObject.hand)
            {
                if (cardInHand.effects != null)
                {
                    foreach (Effects effect in cardInHand.effects)
                    {
                        if (effect.triggerType==EffectTriggerTypes.Counter)
                        {
                                effect.cardEffect?.Activate(cardInHand);
                        }
                    }
                }
            }
            DectiveBlockersForBlocking(CheckForBlockers());
            ChatManager.Instance.AddMessage("Select cards you want to counter with or click on no more counter button!");
            noBlockBtn.gameObject.SetActive(false);
            noBlockBtn.onClick.RemoveAllListeners();
            List<Card> counterCards = GetCardsWithCounterFromHand();
            MakeCounterCardsActiveForCountering(counterCards, attacked);
            if (noMoreCounterBtn != null)
            {
                noMoreCounterBtn.onClick.AddListener(() => NoMoreCounterTrigger(attacker, attacked));
            }
            noMoreCounterBtn.gameObject.SetActive(true);
        }

        private List<Card> GetCardsWithCounterFromHand()
        {
            List<Card> cardsWithCounter = new List<Card>();
            foreach (Card card in handObject.hand)
            {
                if (card.cardData.counter != 0)
                {
                    cardsWithCounter.Add(card);
                }
            }
            return cardsWithCounter;
        }

        private void MakeCounterCardsActiveForCountering(List<Card> counterCards, Card attacked)
        {
            foreach (Card card in counterCards)
            {
                card.SetAttacked(attacked);
                card.CardClickedWithLeftMouseForCountering += Card_CardClickedWithLeftMouseForCountering;
                card.MakeBorderForThisCard(Color.green,"counter");
            }
        }

        private void DectiveCounterCardsForCountering(List<Card> counterCards)
        {
            foreach (Card card in counterCards)
            {
                card.CardClickedWithLeftMouseForCountering -= Card_CardClickedWithLeftMouseForCountering;
                card.RemoveBorderForThisCard();
            }
        }
        private async void Card_CardClickedWithLeftMouseForCountering(Card attacked, Card counterCard)
        {
            await UnityMainThreadDispatcher.RunOnMainThread(async () =>
            {
                counterCard.CardClickedWithLeftMouseForCountering -= Card_CardClickedWithLeftMouseForCountering;
                counterCard.RemoveBorderForThisCard();
                MoveCardToTrash(counterCard);
                attacked.AddToPlusPower(counterCard.cardData.counter);
                attacked.MakeOrUpdatePlusPowerSeenOnCard();
                await ServerCon.Instance.AddPlusPowerFromCounterToEnemy(attacked.cardData.customCardID, counterCard.cardData.customCardID, counterCard.cardData.counter);
            });
        }

        private void HandleDamageStep(Card attacker, Card attacked)
        {
            DectiveCounterCardsForCountering(GetCardsWithCounterFromHand());
            noMoreCounterBtn.gameObject.SetActive(false);
            noMoreCounterBtn.onClick.RemoveAllListeners();
            if (attacked.cardData.power + attacked.plusPower <= attacker.cardData.power + attacker.plusPower)
            {
                switch (attacked.cardData.cardType)
                {
                    case CardType.CHARACTER:
                        TrashCharacter(attacked);
                        break;
                    case CardType.LEADER:
                        TakeLife();
                        break;
                }
            }
            attacked.ResetPlusPower();
            attacked.HidePlusPowerOnCard();
            SendBattleHasEnded(attacker.cardData.customCardID, attacked.cardData.customCardID);
            GameManager.Instance.ChangeBattlePhase(BattlePhases.ENDOFBATTLE, attacker, attacked);
            currentlyAttackedCard = null;
        }

        public async void ResetAttackedCardBeforeEndOfBattle(string attackedID)
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                Card attackedCard = null;
                LeaderCard enemyLeader = EnemyBoard.Instance.leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
                if (enemyLeader != null)
                {
                    if (enemyLeader.cardData.customCardID == attackedID)
                    {
                        attackedCard = enemyLeader;
                    }
                    else
                    {
                        attackedCard = EnemyBoard.Instance.cards.Where(x => x.cardData.customCardID == attackedID).Single();
                    }
                    if (attackedCard != null)
                    {
                        attackedCard.ResetPlusPower();
                        attackedCard.HidePlusPowerOnCard();
                    }
                }
                GameManager.Instance.ChangeBattlePhase(BattlePhases.ENDOFBATTLE);
            });
        }
        private void HandleEndOfBattleStep()
        {
            if (leaderCard.life >= 0)
            {
                GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
            }
            else
            {
                GameManager.Instance.ChangeGameState(GameState.MATCHLOST);
            }
            MakeBorderForCardsThatCanAttack();
        }

        public void MakeBorderForCardsThatCanAttack()
        {
            if (leaderCard.canAttack)
            {
                leaderCard.MakeBorderForThisCard(Color.green,"attack");
            }
            foreach (Card card in characterAreaCards)
            {
                if (card.canAttack)
                {
                    card.MakeBorderForThisCard(Color.green, "attack");
                }
            }
        }
        private async void HandleNoBattle()
        {
            if (GameManager.Instance.currentState == GameState.PLAYERPHASE)
            {
                await UnityMainThreadDispatcher.RunOnMainThread(() =>
                {
                    endOfTurnBtn.gameObject.SetActive(true);
                });
            }
        }
        private async void SendBattleHasEnded(string attackerID, string attackedID)
        {
            await ServerCon.Instance.BattleEnded(attackerID, attackedID);
        }

        private void CorrectAmountOfCardDrawn()
        {
            Card.NeedToWatchHowManyCardsDrawn(false, handObject.transform);
            if (GameManager.Instance.currentPlayerTurnPhase == PlayerTurnPhases.DRAWPHASE)
            {
                GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.DONPHASE);
            }
            else if (GameManager.Instance.currentPlayerTurnPhase == PlayerTurnPhases.DONPHASE)
            {
                GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
            }
        }
        private void MakeHandCardsDraggable()
        {
            foreach (Card card in handObject.hand)
            {
                card.ChangeDraggable(true);
            }
        }
        private void ActivateCharacterAreaCards()
        {
            foreach (CharacterCard card in characterAreaCards)
            {
                card.SetCardActive();
                if (!firstRound)
                {
                    card.CardCanAttack();
                }
            }
        }
        private void MakeLeaderAttackActive()
        {
            if (!firstRound)
            {
                leaderCard.CardCanAttack();
            }
        }

        private void DeactivateAttackOnLeader()
        {
            leaderCard.CardCannotAttack();
        }

        private void DeactivateDraggableHandCards()
        {
            foreach (Card card in handObject.hand)
            {
                card.ChangeDraggable(false);
            }
        }

        private void DeactivateAttackOnCharacterAreaCards()
        {
            foreach (CharacterCard card in characterAreaCards)
            {
                card.CardCannotAttack();
            }
        }

        private void TurnOffDraggableOnAllDon()
        {
            foreach (DonCard donCard in donCardsInDeck)
            {
                donCard.ChangeDraggable(false);
            }
        }

        public async Task EnemyAttacked(string attackerCardID, string attackedCardID, bool thereIsWhenAttacking)
        {
            Card cardThatAttacks = null;
            Card attackedCard = null;
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                LeaderCard enemyLeader = EnemyBoard.Instance.leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
                if (enemyLeader.cardData.customCardID == attackerCardID)
                {
                    cardThatAttacks = enemyLeader;
                }
                else
                {
                    cardThatAttacks = EnemyBoard.Instance.cards.Where(x => x.cardData.customCardID == attackerCardID).Single();
                }
            });
            UnityMainThreadDispatcher.Enqueue(async () =>
            {
                if (leaderCard.cardData.customCardID == attackedCardID)
                {
                    attackedCard = leaderCard;
                }
                else
                {
                    foreach (Card card in characterAreaCards)
                    {
                        if (card.cardData.customCardID == attackedCardID)
                        {
                            attackedCard = card;
                            break;
                        }
                    }
                }
                if (thereIsWhenAttacking)
                {
                    await ServerCon.Instance.ReceivedtAttackDeclaration();
                    await ServerCon.Instance.WaitForEnemyToFinishWhenAttacking();
                }
                GameManager.Instance.ChangeBattlePhase(BattlePhases.BLOCKSTEP, cardThatAttacks, attackedCard);
            });
            await Task.CompletedTask;
        }
        private void TakeLife()
        {
            if (lifeObject.lifeCards.Count > 0)
            {
                Card topLife = lifeObject.lifeCards.Last();
                AddCardToHandFromLife(topLife, true, true);
                ChatManager.Instance.AddMessage("Enemy successfully damaged your leader! Taking " + 1 + " life!");
            }
            else
            {
                ChatManager.Instance.AddMessage("Enemy successfully took down you leader!");
            }
            leaderCard.ReduceLife(1);
        }
        private void TrashCharacter(Card card)
        {
            if (card.hasDonAttached)
            {
                List<Card> donCardsOfCard = new List<Card>();
                for (int i = 0; i < card.transform.childCount; i++)
                {
                    Card donCard = card.transform.GetChild(i).gameObject.GetComponent<Card>();
                    if (donCard != null)
                    {
                        donCardsOfCard.Add(donCard);
                    }
                }
                foreach (Card donCard in donCardsOfCard)
                {
                    MoveDonFromDeckToCostArea(donCard);
                    donCard.SetCardNotActive();
                    donCard.SetRested(true);
                }
            }
            card.ChangeDonAttached(false);
            card.RemoveBorderForThisCard();
            card.ResetPlusPower();
            card.HidePlusPowerOnCard();
            MoveCardToTrash(card);
            ChatManager.Instance.AddMessage("Enemy successfully damaged your character! Trashing character!");
        }

        public List<Card> GetRestedDons()
        {
            List<Card> restedDons = new List<Card>();
            foreach (Card donCard in donInCostArea)
            {
                if (donCard.rested && !donCard.cardData.active)
                {
                    restedDons.Add(donCard);
                }
            }
            return restedDons;
        }

        public List<Card> GetCardsThatCouldAttack()
        {
            List<Card> cardThatCanAttack = new List<Card>();
            if (leaderCard.canAttack)
            {
                cardThatCanAttack.Add(leaderCard);
            }
            foreach (Card card in characterAreaCards)
            {
                if (card.canAttack)
                {
                    cardThatCanAttack.Add(card);
                }
            }
            return cardThatCanAttack;
        }

        public List<Card> GetCharacterAreaCards()
        {
            List<Card> characterAreaCard = new List<Card>();
            foreach (Card card in this.characterAreaCards)
            {
                characterAreaCard.Add(card);
            }
            return characterAreaCard;
        }

        public Card GetCurrentLeader()
        {
            Card leader = leaderCard;
            return leader;
        }

        public void ICantActivateBlockerOverThis(int overThis)
        {
            blockPowerReq = overThis;
            havingBlockReq = true;
            overThisBlocking = true;
        }

        public void RemoveCardToMakeRoomForNewOne(Card card)
        {
            cardToMoveCharArea = card;
            cardToMoveCharArea.transform.position = leaderCard.transform.position;
            cardToMoveCharArea.transform.Translate(-150, 0, 0);

            possibleTargetsForRemovinToMakeRoom = GetCharacterAreaCards();
            cardsThatCouldAttackBeforeRemove = GetCardsThatCouldAttack();
            cardsThatCouldAttackBeforeRemove.Remove(card);

            foreach (Card cardCanAttack in cardsThatCouldAttackBeforeRemove)
            {
                switch (cardCanAttack.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        cardCanAttack.GetComponent<CharacterCard>().CardCannotAttack();
                        break;
                    case CardResources.CardType.LEADER:
                        cardCanAttack.GetComponent<LeaderCard>().CardCannotAttack();
                        break;
                    default:
                        UnityEngine.Debug.LogError("Missing card type");
                        break;
                }
            }
            foreach (Card target in possibleTargetsForRemovinToMakeRoom)
            {
                if (target != card)
                {
                    target.IsTargetForEffect(true);
                    target.SetClickAction(OnTargetSelectedForRemoving);
                }
            }
        }

        private void OnTargetSelectedForRemoving(Card target)
        {
            target.ClearClickAction();
            target.IsTargetForEffect(false);
            MoveCardToTrash(target);

            foreach (Card card in possibleTargetsForRemovinToMakeRoom)
            {
                card.ClearClickAction();
                card.IsTargetForEffect(false);
            }
            foreach (Card card in cardsThatCouldAttackBeforeRemove)
            {
                switch (card.cardData.cardType)
                {
                    case CardResources.CardType.CHARACTER:
                        card.GetComponent<CharacterCard>().CardCanAttack();
                        break;
                    case CardResources.CardType.LEADER:
                        card.GetComponent<LeaderCard>().CardCanAttack();
                        break;
                    default:
                        break;
                }
            }
            MoveCardFromHandToCharacterArea(cardToMoveCharArea);

            cardToMoveCharArea = null;
            possibleTargetsForRemovinToMakeRoom = null;
            cardsThatCouldAttackBeforeRemove = null;
        }

        public void SetEffectInProgress(bool inProgress)
        {
            this.effectInProgress = inProgress;
        }

        public async void GiveCounterToCurrentlyAttackedCard(Card counterGiverCard, int counterPower)
        {
            if (currentlyAttackedCard != null && GameManager.Instance.currentBattlePhase==BattlePhases.COUNTERSTEP)
            {
                currentlyAttackedCard.AddToPlusPower(counterPower);
                currentlyAttackedCard.MakeOrUpdatePlusPowerSeenOnCard();
                await ServerCon.Instance.AddPlusPowerFromCounterToEnemy(currentlyAttackedCard.cardData.customCardID, counterGiverCard.cardData.customCardID,counterPower);
            }
        }

        public void KoThisCard(string koThisID, string effectCallerID)
        {
            foreach (Card card in characterAreaCards)
            {
                if (card.cardData.customCardID == koThisID)
                {
                    TrashCharacter(card);
                    break;
                }
            }
        }

        public void SetEnemyFinishedStartingHand(bool finished)
        {
            this.enemyFinishedStartingHand = finished;
        }
    }
}
