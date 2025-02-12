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

        // Start is called before the first frame update
        void Start()
        {
            
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
            Hand p1Hand = Instantiate(handPrefab, this.gameObject.transform).GetComponent<Hand>();
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
