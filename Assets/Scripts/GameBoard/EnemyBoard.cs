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

        // Update is called once per frame
        void Update()
        {
            if (deckObject != null)
            {
                //Debug.Log(boardName + "- In hand:" + handObject.transform.childCount + ", in deck: " + deckObject.transform.childCount + ", in life: " + lifeObject.transform.childCount);
            }
        }

        public override void Init(string boardName, string gameCustomID)
        {
            base.Init(boardName, gameCustomID);
            Debug.Log(boardName);
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

        public async Task CreateOrUpdateLeaderCardFromGameDB(string customCardID)
        {
            UnityMainThreadDispatcher.Enqueue(async () =>
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

            await Task.CompletedTask;
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
                if(card != null)
                {
                    card.LoadDataFromCardData(cardData);
                    card.Init();
                    this.SetCardParentByNameString(cardData.currentParent, card);
                    deck.Add(card);
                }  
            }
            return deck;
        }
    }
}
