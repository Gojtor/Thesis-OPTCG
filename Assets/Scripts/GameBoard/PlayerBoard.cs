using System.Collections;
using System.Collections.Generic;
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

        private Transform playerHand;

        private Hand handObject;

        private List<string> deck;

        // Start is called before the first frame update
        void Start()
        {
            deck = new List<string>
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

            handObject.CreateStartingHand(new List<string> { deck[5], deck[10], deck[15], deck[20], deck[25] });
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
            Instantiate(deckPrefab, this.gameObject.transform);
        }
        public void Init(string boardName)
        {
            this.name = boardName;
        }

        public Transform getPlayerHand()
        {
            return playerHand;
        }
    }
}
