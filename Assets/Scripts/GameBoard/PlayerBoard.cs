using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

namespace TCGSim
{
    public class PlayerBoard : Board
    {
        public static PlayerBoard Instance { get; private set; }

        private bool firstRound = true;

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
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        private void OnDestroy()
        {
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
            endOfTurnBtn.gameObject.SetActive(false);
            noBlockBtn.gameObject.SetActive(false);
            noMoreCounterBtn.gameObject.SetActive(false);
            if (endOfTurnBtn != null)
            {
                endOfTurnBtn.onClick.AddListener(EndOfTurnTrigger);
            }
        }
        public void CreateADeck()
        {
            deckString = new List<string>
            {
            "1xST01-001",
            "4xST01-002",
            "4xST01-003",
            "4xST01-004",
            "4xST01-005",
            "4xST01-006",
            "4xST01-007",
            "4xST01-008",
            "4xST01-009",
            "4xST01-010",
            "2xST01-011",
            "2xST01-012",
            "2xST01-013",
            "2xST01-014",
            "2xST01-015",
            "2xST01-016",
            "2xST01-017"};
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
        }
        public void MoveDonFromDeckToCostArea(Card card)
        {
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
            }
        }
        public void MoveStageFromHandToStageArea(Card card)
        {
            card.SetCardVisibility(CardResources.CardVisibility.BOTH);
            handObject.RemoveCardFromHand(card, this.stageObject.transform);
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
            Shuffle<Card>(deckCards);
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
            LeaderCard leader = leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
            if (leader == null) { return; }
            for (int i = 0; i < leader.life; i++)
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
            GameManager.Instance.ChangeBattlePhase(BattlePhases.COUNTERSTEP, attacker, attacked);
        }

        public void NoMoreCounterTrigger(Card attacker, Card attacked)
        {

            GameManager.Instance.ChangeBattlePhase(BattlePhases.DAMAGESTEP, attacker, attacked);
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
            Shuffle<string>(deckString);
            deckCards = await CreateCardsFromDeck();
            UnityMainThreadDispatcher.Enqueue(async () =>
            {
                Shuffle<Card>(deckCards);
                ReassingOrderAfterShuffle();
                Debug.Log(boardName);
                CreateStartingHand();
                if (boardName == "PLAYERBOARD")
                {
                    handObject.ScaleHandForStartingHand();
                    LoadMulliganKeepButtons();
                    LeaderCard leader = leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
                    if (leader != null)
                    {
                        await ServerCon.Instance.UpdateMyLeaderCardAtEnemy(leader.cardData.customCardID);

                    }
                }
                Card.CorrectAmountCardsDrawn += CorrectAmountOfCardDrawn;
            });

        }
        private void HandlePlayerPhase()
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.REFRESHPHASE);
            });
        }
        private void HandleEnemyPhase()
        {
            ChatManager.Instance.AddMessage("Enemy turn!");
        }
        private void HandleMatchLost()
        {
            ChatManager.Instance.AddMessage("You lost the match!");
            GameBoard.Instance.GameLost();
        }
        private void HandleMatchWon()
        {
            ChatManager.Instance.AddMessage("You won the game!");
            GameBoard.Instance.GameWon();
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
            }
            foreach (CharacterCard card in characterAreaCards)
            {
                if (card.rested)
                {
                    card.Restand(true, false);
                    card.SendCardToServer();
                }
            }
            LeaderCard leader = leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
            if (leader.rested)
            {
                leader.Restand(true, false);
                leader.SendCardToServer();
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
        private void HandleDONPhase()
        {
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
            string attackerID = attacker.cardData.customCardID;
            string attackedID = attacked.cardData.customCardID;
            ChatManager.Instance.AddMessage("You attacked the following card: " + attackedID + " with the following card: " + attackerID);
            attacker.Rest();
            attacker.SendCardToServer();
            endOfTurnBtn.gameObject.SetActive(false);
            await ServerCon.Instance.AttackedEnemyCard(attackerID, attackedID, attacker.cardData.power);
        }
        private void HandleBlockStep(Card attacker, Card attacked)
        {
            string attackerID = attacker.cardData.customCardID;
            string attackedID = attacked.cardData.customCardID;
            ChatManager.Instance.AddMessage("The enemy attacked your card: " + attackedID + " with the following card: " + attackerID);
            ChatManager.Instance.AddMessage("Select a blocker or click on no block button!");
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
        private void HandleCounterStep(Card attacker, Card attacked)
        {
            ChatManager.Instance.AddMessage("Select cards you want to counter with or click on no more counter button!");
            noBlockBtn.gameObject.SetActive(false);
            noBlockBtn.onClick.RemoveAllListeners();
            if (noMoreCounterBtn != null)
            {
                noMoreCounterBtn.onClick.AddListener(() => NoMoreCounterTrigger(attacker, attacked));
            }
            noMoreCounterBtn.gameObject.SetActive(true);
        }
        private void HandleDamageStep(Card attacker, Card attacked)
        {
            noMoreCounterBtn.gameObject.SetActive(false);
            noMoreCounterBtn.onClick.RemoveAllListeners();
            if (attacked.cardData.power <= attacker.cardData.power)
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
            switch (attacker.cardData.cardType)
            {
                case CardType.CHARACTER:
                    attacker.GetComponent<CharacterCard>().RemoveAttackLine();
                    break;
                case CardType.LEADER:
                    attacker.GetComponent<LeaderCard>().RemoveAttackLine();
                    break;
            }
            SendBattleHasEnded(attacker.cardData.customCardID, attacked.cardData.customCardID);
            GameManager.Instance.ChangeBattlePhase(BattlePhases.ENDOFBATTLE, attacker, attacked);
        }
        private void HandleEndOfBattleStep()
        {
            LeaderCard leader = leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
            if (leader.life >= 0)
            {
                GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
            }
            else
            {
                GameManager.Instance.ChangeGameState(GameState.MATCHLOST);
            }
        }
        private void HandleNoBattle()
        {
            if (GameManager.Instance.currentState == GameState.PLAYERPHASE)
            {
                endOfTurnBtn.gameObject.SetActive(true);
            }
        }
        private async void SendBattleHasEnded(string attackerID, string attackedID)
        {
            await ServerCon.Instance.BattleEnded(attackerID, attackerID);
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
            LeaderCard leader = leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
            if (!firstRound)
            {
                leader.CardCanAttack();
            }
        }

        private void DeactivateAttackOnLeader()
        {
            LeaderCard leader = leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
            leader.CardCannotAttack();
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

        public async Task EnemyAttacked(string attackerCardID, string attackedCardID)
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
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                LeaderCard leader = leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
                if (leader.cardData.customCardID == attackedCardID)
                {
                    attackedCard = leader;
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
                GameManager.Instance.ChangeBattlePhase(BattlePhases.BLOCKSTEP, cardThatAttacks, attackedCard);
            });
            await Task.CompletedTask;
        }
        private void TakeLife()
        {
            Card topLife = lifeObject.lifeCards.Last();
            AddCardToHandFromLife(topLife, true, true);
            LeaderCard leader = leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
            ChatManager.Instance.AddMessage("Enemy successfully damaged your leader! Taking " + 1 + " life!");
            leader.ReduceLife(1);
        }
        private void TrashCharacter(Card card)
        {
            MoveCardToTrash(card);
            ChatManager.Instance.AddMessage("Enemy successfully damaged your character! Trashing character!");

        }
    }
}
