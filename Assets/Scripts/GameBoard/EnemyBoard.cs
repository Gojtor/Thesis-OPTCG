using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCGSim;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;

namespace TCGSim
{
    public class EnemyBoard : Board
    {
        public static EnemyBoard Instance { get; private set; }
        public List<Card> cards { get; private set; } = new List<Card>();
        // Start is called before the first frame update
        void Start()
        {

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

        // Update is called once per frame
        void Update()
        {
            if (deckObject != null)
            {
                //Debug.Log(boardName + "- In hand:" + handObject.transform.childCount + ", in deck: " + deckObject.transform.childCount + ", in life: " + lifeObject.transform.childCount);
            }
        }
        private void OnDestroy()
        {
            GameManager.OnGameStateChange -= GameManagerOnGameStateChange;
            GameManager.OnPlayerTurnPhaseChange -= GameManagerOnPlayerTurnPhaseChange;
            GameManager.OnBattlePhaseChange -= GameManagerOnBattlePhaseChange;
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

        public override void GameManagerOnGameStateChange(GameState state)
        {
            if (state == GameState.STARTINGPHASE)
            {
                LoadBoardElements();
                CreateStartingPhaseBoard();
            }
        }

        public void CreateStartingPhaseBoard()
        {
            GameObject deckCardObj = Instantiate(cardPrefab, deckObject.transform);
            deckCardObj.AddComponent<CharacterCard>();
            Card deckCard = deckCardObj.GetComponent<CharacterCard>();
            cards.Add(deckCard);
            for (int i = 0; i < 5; i++)
            {
                GameObject cardObj = Instantiate(cardPrefab, handObject.transform);
                cardObj.AddComponent<CharacterCard>();
                Card card = cardObj.GetComponent<CharacterCard>();
                cards.Add(card);
            }
        }

        public void AddCounterPower(string toCardID, string counterCardID, int counterValue)
        {
            UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                LeaderCard leaderCard = leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
                if (leaderCard.cardData.customCardID == toCardID)
                {
                    leaderCard.AddToPlusPower(counterValue);
                    leaderCard.MakeOrUpdatePlusPowerSeenOnCard();
                }
                else
                {
                    Card characterCard = cards.Where(x => x.cardData.customCardID == toCardID).Single();
                    characterCard.AddToPlusPower(counterValue);
                    characterCard.MakeOrUpdatePlusPowerSeenOnCard();
                }
            });
        }

        public void AddPlusPowerToCard(string fromCardID, string toCardID, int plusPower)
        {
            UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                LeaderCard leaderCard = leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
                Card fromCard = cards.Where(x => x.cardData.customCardID == fromCardID).Single();
                if (leaderCard.cardData.customCardID == toCardID)
                {
                    leaderCard.AddToPlusPower(plusPower);
                    leaderCard.MakeOrUpdatePlusPowerSeenOnCard();
                }
                else
                {
                    Card characterCard = cards.Where(x => x.cardData.customCardID == toCardID).Single();
                    characterCard.AddToPlusPower(plusPower);
                    characterCard.MakeOrUpdatePlusPowerSeenOnCard();
                }
            });
        }

        public List<Card> GetCharacterAreaCardsUnderThisPower(int underThisPower)
        {
            List<Card> cardsUnderGivenPower = new List<Card>();
            foreach (Card card in cards)
            {
                if (card.transform.parent == EnemyBoard.Instance.characterAreaObject.transform && card.cardData.power <= underThisPower)
                {
                    cardsUnderGivenPower.Add(card);
                }
            }
            return cardsUnderGivenPower;
        }

        public async Task UpdateBoardFromGameDB()
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    Destroy(cards[i].gameObject);
                }
                cards.Clear();
            });

            UnityMainThreadDispatcher.Enqueue(async () =>
            {
                List<Card> newCards = await GetEnemyCardsFromGameDB();
                cards.AddRange(newCards);
            });
            await Task.CompletedTask;
        }

        public async Task UpdateCardFromGameDB(string customCardID)
        {
            UnityMainThreadDispatcher.Enqueue(async () =>
            {
                CardData newCardData = await ServerCon.Instance.GetCardByFromGameDBByGameIDAndPlayerAndCustomCardID(this.gameCustomID, this.playerName, customCardID);
                Card card;
                switch (newCardData.cardType)
                {
                    case CardType.DON:
                        card = donCardsInDeck.Where(x => x.cardData.customCardID == newCardData.customCardID).Single();
                        break;
                    case CardType.LEADER:
                        card = leaderObject.transform.GetChild(0).GetComponent<LeaderCard>();
                        break;
                    default:
                        card = cards.Where(x => x.cardData.customCardID == newCardData.customCardID).Single();
                        break;
                }
                card.LoadDataFromCardData(newCardData);
                card.UpdateEnemyCardAfterDataLoad();
            });

            await Task.CompletedTask;
        }

        public async void CreateOrUpdateLeaderCardFromGameDB(string customCardID)
        {
            await UnityMainThreadDispatcher.RunOnMainThread(async () =>
            {
                if (leaderObject == null)
                {
                    this.CreateLeaderArea();
                }
                if (leaderObject.transform.childCount == 0)
                {
                    CardData leaderCardData = await ServerCon.Instance.GetCardByFromGameDBByGameIDAndPlayerAndCustomCardID(this.gameCustomID, this.playerName, customCardID);
                    GameObject cardObj = Instantiate(cardPrefab, this.leaderObject.transform);
                    cardObj.AddComponent<LeaderCard>();
                    LeaderCard card = cardObj.GetComponent<LeaderCard>();
                    card.LoadDataFromCardData(leaderCardData);
                    card.Init();
                    card.UpdateEnemyCardAfterDataLoad();
                }
                else
                {
                    await UpdateCardFromGameDB(customCardID);
                }
            });
        }

        public async Task<List<Card>> GetEnemyCardsFromGameDB()
        {
            List<Card> deck = new List<Card>();
            List<CardData> cardsFromDB = await ServerCon.Instance.GetAllCardByGameIDAndPlayerName(this.gameCustomID, this.playerName);
            foreach (CardData cardData in cardsFromDB)
            {
                GameObject cardObj;
                Card card;
                switch (cardData.cardType)
                {
                    case CardType.CHARACTER:
                        cardObj = Instantiate(cardPrefab, EnemyBoard.Instance.GetParentByNameString(cardData.currentParent).transform);
                        cardObj.AddComponent<CharacterCard>();
                        card = cardObj.GetComponent<CharacterCard>();
                        break;
                    case CardType.STAGE:
                        cardObj = Instantiate(cardPrefab, EnemyBoard.Instance.GetParentByNameString(cardData.currentParent).transform);
                        cardObj.AddComponent<StageCard>();
                        card = cardObj.GetComponent<StageCard>();
                        break;
                    case CardType.EVENT:
                        cardObj = Instantiate(cardPrefab, EnemyBoard.Instance.GetParentByNameString(cardData.currentParent).transform);
                        cardObj.AddComponent<EventCard>();
                        card = cardObj.GetComponent<EventCard>();
                        break;
                    default:
                        card = null;
                        break;
                }
                if (card != null)
                {
                    card.LoadDataFromCardData(cardData);
                    card.Init();
                    this.SetCardParentByNameString(cardData.currentParent, card);
                    deck.Add(card);
                }
            }
            return deck;
        }

        public void CreateMockCard()
        {
            GameObject cardObj = Instantiate(cardPrefab, characterAreaObject.transform);
            cardObj.AddComponent<CharacterCard>();
            CharacterCard card = cardObj.GetComponent<CharacterCard>();

            CardData fakeData = new CardData
            {

                cardID = "ST01-002",
                customCardID = "ST01-002-MOCKED",
                cardName = "Test Card " + 2,
                cardType = CardType.CHARACTER,
                power = 1000,
                cost = 1
            };
            card.Init(handObject, fakeData.cardID);
            card.LoadDataFromCardData(fakeData);
            card.cardData.playerName = this.playerName;
            card.SetCardActive();
            card.FlipCard();
            cards.Add(card);
        }

        public void CreateMockLeaderForTesting()
        {
            GameObject cardObj = Instantiate(cardPrefab, leaderObject.transform);
            cardObj.AddComponent<LeaderCard>();
            LeaderCard leaderCard = cardObj.GetComponent<LeaderCard>();

            CardData fakeData = new CardData
            {

                cardID = "ST01-001",
                customCardID = "ST01-001-MOCKEDLEADER",
                cardName = "ST01-001-MOCKEDLEADER",
                cardType = CardType.LEADER,
                power = 1000,
                cost = 0
            };
            leaderCard.Init(handObject, fakeData.cardID);
            leaderCard.LoadDataFromCardData(fakeData);
            leaderCard.cardData.playerName = this.playerName;
            leaderCard.SetCardActive();
            leaderCard.FlipCard();
        }
    }
}
