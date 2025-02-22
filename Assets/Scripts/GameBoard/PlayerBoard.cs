using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using TCGSim.CardResources;
using UnityEngine;
using UnityEngine.EventSystems;
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

        private Transform playerHand;
        private Hand handObject;
        private GameObject deckObject;
        private GameObject lifeObject;

        private List<string> deckString;
        private List<Card> deckCards = new List<Card>();

        private ServerCon serverCon;

        // Start is called before the first frame update
        private async void Start()
        {
            playerHand = handPrefab.transform;
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
            deckCards = await CreateCardsFromDeck();
            enableRaycastOnTopCard();
            handObject.AddCardToHand(deckCards[5]);
            handObject.AddCardToHand(deckCards[10]);
            handObject.AddCardToHand(deckCards[15]);
            handObject.AddCardToHand(deckCards[20]);
            handObject.AddCardToHand(deckCards[25]);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        private void Awake()
        {
            
        }
        public void CreateHand() 
        {
            handObject = Instantiate(handPrefab, this.gameObject.transform).GetComponent<Hand>();
        }

        public void CreateDeck()
        {
            deckObject = Instantiate(deckPrefab, this.gameObject.transform);
        }

        public void CreateLife()
        {
            lifeObject = Instantiate(lifePrefab, this.gameObject.transform);
        }

        public void Init(string boardName, ServerCon serverCon)
        {
            this.name = boardName;
            this.serverCon = serverCon;
            if (serverCon == null)
            {
                Debug.LogError("ServerCon prefab NULL after Init!", this);
            }
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
                Card card = Instantiate(cardPrefab, deckObject.transform).GetComponent<Card>();
                card.Init(this,handObject);
                CardData cardData = await serverCon.GetCardByCardID(cardNumber);
                Debug.Log(cardData.cardID);
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
    }
}
