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
    public class PlayerBoard : MonoBehaviour
    {
        public string boardName { get; private set; }
        public string gameCustomID { get; private set; }


        /// <summary>
        ///  Prefabs for the player board
        /// </summary>
        #region Prefabs
        [SerializeField]
        private GameObject handPrefab;

        [SerializeField]
        private GameObject characterAreaPrefab;

        [SerializeField]
        private GameObject costAreaPrefab;

        [SerializeField]
        private GameObject stageAreaPrefab;

        [SerializeField]
        private GameObject deckPrefab;

        [SerializeField]
        private GameObject leaderPrefab;

        [SerializeField]
        private GameObject trashPrefab;

        [SerializeField]
        private GameObject cardPrefab;

        [SerializeField]
        private GameObject lifePrefab;

        [SerializeField]
        private GameObject keepBtnPrefab;

        [SerializeField]
        private GameObject mulliganBtnPrefab;

        [SerializeField]
        private GameObject donDeckPrefab;

        [SerializeField]
        private GameObject donPrefab;
        #endregion

        /// <summary>
        ///  Objects of the player board
        /// </summary>
        #region Objects
        private Transform playerHand;
        private Hand handObject;
        private GameObject deckObject;
        public GameObject stageObject { get; private set; }
        private GameObject leaderObject;
        private GameObject trashObject;
        private CostArea costAreaObject;
        public GameObject donDeckObject { get; private set; }
        private Life lifeObject;
        Button keepBtn;
        Button mulliganBtn;
        Button testBtn;
        #endregion

        /// <summary>
        ///  Cards of the player board (which doesn't stored in another object)
        /// </summary>
        #region Cards
        private List<string> deckString;
        private List<Card> deckCards = new List<Card>();
        private List<Card> donCards = new List<Card>();
        #endregion

        private ServerCon serverCon;
        public int activeDon { get; private set; } = 0;

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            Debug.Log(boardName + "- In hand:" + handObject.transform.childCount + ", in deck: " + deckObject.transform.childCount + ", in life: " + lifeObject.transform.childCount);
            Debug.Log("Active dons: " + activeDon);
            if (costAreaObject != null)
            {
                activeDon = costAreaObject.GetActiveDonCount();
            }    
        }
        private void Awake()
        {

        }
        public async void Init(string boardName, ServerCon serverCon, string gameCustomID)
        {
            this.boardName = boardName;
            this.serverCon = serverCon;
            this.gameCustomID = gameCustomID;
            if (serverCon == null)
            {
                Debug.LogError("ServerCon prefab NULL after Init!", this);
            }
            LoadBoardElements();
            Shuffle<string>(deckString);
            deckCards = await CreateCardsFromDeck();
            Shuffle<Card>(deckCards);
            Debug.Log(boardName);
            CreateStartingHand();
            if (boardName == "PLAYERBOARD")
            {
                handObject.ScaleHandForStartingHand();
                LoadMulliganKeepButtons();
            }
        }

        public void LoadBoardElements()
        {
            CreateLeaderArea();
            CreateStageArea();
            CreateTrashArea();
            CreateCostArea();
            CreateDons();
            CreateDeck();
            CreateLife();
            CreateHand();
            CreateADeck();       
        }

        public void CreateHand()
        {
            handObject = Instantiate(handPrefab, this.gameObject.transform).GetComponent<Hand>();
            playerHand = handPrefab.transform;
            handObject.Init(this);
        }

        public void CreateDeck()
        {
            deckObject = Instantiate(deckPrefab, this.gameObject.transform);
        }

        public void CreateLife()
        {
            lifeObject = Instantiate(lifePrefab, this.gameObject.transform).GetComponent<Life>();
            lifeObject.Init(this);
        }

        public void CreateDons()
        {
            donDeckObject = Instantiate(donDeckPrefab, this.gameObject.transform);
            for (int i = 0; i < 10; i++)
            {
                DonCard donCard = Instantiate(donPrefab, this.donDeckObject.transform).GetComponent<DonCard>();
                donCard.Init(this, handObject, "DONCARD"+i);
                donCards.Add(donCard);
            }
        }
        public void CreateCostArea()
        {
            costAreaObject = Instantiate(costAreaPrefab, this.gameObject.transform).GetComponent<CostArea>();
        }

        public void CreateStageArea()
        {
            stageObject = Instantiate(stageAreaPrefab, this.gameObject.transform);
        }

        public void CreateLeaderArea()
        {
            leaderObject = Instantiate(leaderPrefab, this.gameObject.transform);
        }

        public void CreateTrashArea()
        {
            trashObject = Instantiate(trashPrefab, this.gameObject.transform);
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
                CardData cardData = await serverCon.GetCardByCardID(cardNumber);
                GameObject cardObj;
                Card card;
                for (int i = 0; i < count; i++)
                {
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
                    card.Init(this, handObject, cardNumber + "-" + i);
                    card.LoadDataFromCardData(cardData);
                    card.raycastTargetChange(false);
                    deck.Add(card);
                }
                
            }
            return deck;
        }

        public void enableRaycastOnTopCard()
        {
            if (deckObject.transform.childCount > 0)
            {
                deckObject.transform.GetChild(0).GetComponent<Card>().raycastTargetChange(true);
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
            CreateStartingHand();
            foreach (Card card in handObject.hand)
            {
                card.raycastTargetChange(true);
            }
            keepBtn.gameObject.SetActive(false);
            mulliganBtn.gameObject.SetActive(false);
            CreateStartingLife();
            enableRaycastOnTopCard();
            SendAllCardToDB();
        }

        public void KeepHand()
        {
            foreach (Card card in handObject.hand)
            {
                card.raycastTargetChange(true);
            }
            handObject.ScaleHandBackFromStartingHand();
            keepBtn.gameObject.SetActive(false);
            mulliganBtn.gameObject.SetActive(false);
            CreateStartingLife();
            enableRaycastOnTopCard();
            SendAllCardToDB();
        }

        public void Shuffle<T>(IList<T> list)
        {
            int listSize = list.Count;
            System.Random random = new System.Random();
            for (int x = 0; x < 5; x++)
            {
                for (int i = listSize - 1; i >= 1; i--)
                {
                    int j = random.Next(0, listSize);
                    (list[i], list[j]) = (list[j], list[i]);
                }
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
            StartCoroutine(serverCon.AddCardToInGameStateDB(card));
        }

        public void SendAllCardToDB()
        {
            foreach  (Card card in deckCards)
            {
                card.UpdateParent();
                StartCoroutine(serverCon.AddCardToInGameStateDB(card));
            }
            foreach (Card card in handObject.hand)
            {
                card.UpdateParent();
                StartCoroutine(serverCon.AddCardToInGameStateDB(card));
            }
            foreach (Card card in lifeObject.lifeCards)
            {
                card.UpdateParent();
                StartCoroutine(serverCon.AddCardToInGameStateDB(card));
            }
        }

        public void RestDons(int donCountToRest)
        {
            costAreaObject.RestDons(donCountToRest);
        }
    }
}
