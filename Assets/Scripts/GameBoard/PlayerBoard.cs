using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private Transform playerHand;

        private Hand handObject;
        private GameObject deckObject;


        private List<string> deckString;
        private List<Card> deckCards = new List<Card>();

        // Start is called before the first frame update
        void Start()
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
            CreateCardsFromDeck();
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
            playerHand = handPrefab.transform;
        }
        public void CreateHand() 
        {
            handObject = Instantiate(handPrefab, this.gameObject.transform).GetComponent<Hand>();
        }

        public void CreateDeck()
        {
            deckObject = Instantiate(deckPrefab, this.gameObject.transform);
        }
        public void Init(string boardName)
        {
            this.name = boardName;
        }

        public Transform getPlayerHand()
        {
            return playerHand;
        }

        public void CreateCardsFromDeck() 
        {
            foreach (string cardNumber in deckString)
            {
                Card card = Instantiate(cardPrefab, deckObject.transform).GetComponent<Card>();
                card.Init(this,handObject,cardNumber);
                card.raycastTargetChange(false);
                deckCards.Add(card);
            }
        }

        public void enableRaycastOnTopCard()
        {
            deckObject.transform.GetChild(0).GetComponent<Card>().raycastTargetChange(true);
        }
    }
}
