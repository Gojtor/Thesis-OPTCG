using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TCGSim.CardResources;
using TCGSim.CardScripts;
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

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (deckObject != null)
            {
                Debug.Log(boardName + "- In hand:" + handObject.transform.childCount + ", in deck: " + deckObject.transform.childCount + ", in life: " + lifeObject.transform.childCount);
            }
            Debug.Log("Active dons: " + activeDon);
            if (costAreaObject != null)
            {
                activeDon = costAreaObject.GetActiveDonCount();
            }    
        }
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        public override async void GameManagerOnGameStateChange(GameState state)
        {
            if (state == GameState.STARTINGPHASE)
            {
                this.LoadBoardElements();
                Shuffle<string>(deckString);
                deckCards = await CreateCardsFromDeck();
                Shuffle<Card>(deckCards);
                ReassingOrderAfterShuffle();
                Debug.Log(boardName);
                CreateStartingHand();
                if (boardName == "PLAYERBOARD")
                {
                    handObject.ScaleHandForStartingHand();
                    LoadMulliganKeepButtons();
                }
            }
        }

        public override void Init(string boardName, string gameCustomID)
        {
            base.Init(boardName, gameCustomID); 
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

        public void CreateADeck()
        {
            deckString = new List<string>
            {
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
                        default:
                            cardObj = Instantiate(cardPrefab, deckObject.transform);
                            cardObj.AddComponent<Card>();
                            card = cardObj.GetComponent<Card>();
                            break;
                    }
                    card.LoadDataFromCardData(cardData);
                    card.SetCardNotActive();
                    card.Init(handObject, cardNumber + "-" + i);
                    deck.Add(card);
                } 
            }
            return deck;
        }

        public void enableDraggingOnTopDeckCard()
        {
            if (deckObject.transform.childCount > 0)
            {
                deckObject.transform.GetChild(deckObject.transform.childCount-1).GetComponent<Card>().ChangeDraggable(true);
            }
        }

        public void AddCardToHand(Card card)
        {
            this.handObject.AddCardToHand(card);
            deckCards.Remove(card);
        }
        

        public void PutCardBackToDeck(Card card)
        {
            handObject.RemoveCardFromHand(card, this.deckObject);
            card.transform.position = this.deckObject.transform.position;
            card.FlipCard();
            deckCards.Add(card);
        }

        public void CreateStartingHand()
        {
            for (int i = 0; i < 5; i++)
            {
                AddCardToHand(deckCards[i]);
            }
        }

        public void Mulligan()
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
            foreach (Card card in handObject.hand)
            {
                card.ChangeDraggable(true);
            }
            keepBtn.gameObject.SetActive(false);
            mulliganBtn.gameObject.SetActive(false);
            CreateStartingLife();
            enableDraggingOnTopDeckCard();
            SendAllCardToDB();
        }

        public void KeepHand()
        {
            foreach (Card card in handObject.hand)
            {
                card.ChangeDraggable(true);
            }
            handObject.ScaleHandBackFromStartingHand();
            keepBtn.gameObject.SetActive(false);
            mulliganBtn.gameObject.SetActive(false);
            CreateStartingLife();
            enableDraggingOnTopDeckCard();
            SendAllCardToDB();
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
            for (int i = 0; i < 5; i++)
            {
                AddCardToLife(deckCards[i]);
            }
        }

        public void SendCardToDB(Card card)
        {
            StartCoroutine(ServerCon.Instance.AddCardToInGameStateDB(card));
        }

        public void SendAllCardToDB()
        {
            foreach  (Card card in deckCards)
            {
                card.UpdateParent();
                StartCoroutine(ServerCon.Instance.AddCardToInGameStateDB(card));
            }
            foreach (Card card in handObject.hand)
            {
                card.UpdateParent();
                StartCoroutine(ServerCon.Instance.AddCardToInGameStateDB(card));
            }
            foreach (Card card in lifeObject.lifeCards)
            {
                card.UpdateParent();
                StartCoroutine(ServerCon.Instance.AddCardToInGameStateDB(card));
            }
        }

        public void RestDons(int donCountToRest)
        {
            costAreaObject.RestDons(donCountToRest);
        }
    }
}
