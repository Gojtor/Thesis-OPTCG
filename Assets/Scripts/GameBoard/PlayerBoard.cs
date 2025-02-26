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
        public string boardName;

        [SerializeField]
        private GameObject handPrefab;

        [SerializeField]
        private GameObject characterAreaPrefab;

        [SerializeField]
        private GameObject deckPrefab;

        [SerializeField]
        private GameObject cardPrefab;

        [SerializeField]
        private GameObject lifePrefab;

        [SerializeField]
        private GameObject keepBtnPrefab;

        [SerializeField]
        private GameObject mulliganBtnPrefab;

        private Transform playerHand;
        private Hand handObject;
        private GameObject deckObject;
        private Life lifeObject;
        Button keepBtn;
        Button mulliganBtn;

        private List<string> deckString;
        private List<Card> deckCards = new List<Card>();

        private ServerCon serverCon;

        // Start is called before the first frame update
        private void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Log(boardName + "- In hand:" + handObject.transform.childCount + ", in deck: " + deckObject.transform.childCount + ", in life: " + lifeObject.transform.childCount);
        }
        private void Awake()
        {

        }
        public async void Init(string boardName, ServerCon serverCon)
        {
            this.boardName = boardName;
            this.serverCon = serverCon;
            if (serverCon == null)
            {
                Debug.LogError("ServerCon prefab NULL after Init!", this);
            }
            CreateDeck();
            CreateLife();
            CreateHand();
            CreateADeck();
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
            {"ST01-002", "ST01-002", "ST01-002", "ST01-002",
            "ST01-003", "ST01-003", "ST01-003", "ST01-003",
            "ST01-004", "ST01-004", "ST01-004", "ST01-004",
            "ST01-005","ST01-005","ST01-005","ST01-005",
            "ST01-006","ST01-006","ST01-006","ST01-006",
            "ST01-007","ST01-007","ST01-007","ST01-007",
            "ST01-008","ST01-008","ST01-008","ST01-008",
            "ST01-009","ST01-009","ST01-009","ST01-009",
            "ST01-010","ST01-010","ST01-010","ST01-010",
            "ST01-011","ST01-011",
            "ST01-012","ST01-012",
            "ST01-013","ST01-013",
            "ST01-014","ST01-014",
            "ST01-015","ST01-015",
            "ST01-016","ST01-016",
            "ST01-017","ST01-017"};
        }

        public Transform getPlayerHand()
        {
            return playerHand;
        }

        public async Task<List<Card>> CreateCardsFromDeck()
        {
            List<Card> deck = new List<Card>();
            foreach (string cardNumber in deckString)
            {
                CardData cardData = await serverCon.GetCardByCardID(cardNumber);
                GameObject cardObj;
                Card card;
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
                card.Init(this, handObject);
                card.LoadDataFromCardData(cardData);
                card.raycastTargetChange(false);
                deck.Add(card);
            }
            return deck;
        }

        public void enableRaycastOnTopCard()
        {
            deckObject.transform.GetChild(0).GetComponent<Card>().raycastTargetChange(true);
        }

        public void AddCardToHand(Card card)
        {
            this.handObject.AddCardToHand(card);
            deckCards.Remove(card);
        }

        public void PutCardBackToDeck(Card card)
        {
            card.transform.SetParent(this.deckObject.transform);
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
            foreach (Card card in handObject.hand)
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
    }
}
