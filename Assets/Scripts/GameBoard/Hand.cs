using System;
using System.Collections;
using System.Collections.Generic;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;
using UnityEngine.UI;
namespace TCGSim
{
    public class Hand : MonoBehaviour
    {

        [SerializeField]
        private GameObject cardPrefab;

        Board board;


        public List<Card> hand { get; private set; } =  new List<Card>();
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void Init(Board board)
        {
            this.board = board;
        }

        public void AddCardToHand(Card card)
        {
            card.transform.SetParent(this.transform);
            switch (board.boardName)
            {
                case ("PLAYERBOARD"):
                    card.SetCardVisibility(CardVisibility.PLAYERBOARD);
                    break;
                case ("ENEMYBOARD"):
                    card.SetCardVisibility(CardVisibility.ENEMYBOARD);
                    break;
                default:
                    card.SetCardVisibility(CardVisibility.NONE);
                    break;
            }
            hand.Add(card);      
        }

        public void RemoveCardFromHand(Card card, GameObject gameObject)
        {
            card.transform.SetParent(gameObject.transform);
            hand.Remove(card);
        }

        public void ScaleHandForStartingHand()
        {
            RectTransform rectTransform = this.transform.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, 150);
            this.transform.localScale = new Vector3(3.2f, 3f, 3f);
            HorizontalLayoutGroup horizontalLayoutGroup = this.transform.GetComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.spacing = 0; 
            foreach (Card card in hand)
            {
                card.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
            }
            Canvas.ForceUpdateCanvases();
        }

        public void ScaleHandBackFromStartingHand()
        {
            RectTransform rectTransform = this.transform.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, -105);
            this.transform.localScale = new Vector3(1, 1, 1);
            HorizontalLayoutGroup horizontalLayoutGroup = this.transform.GetComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.spacing = 5;
            foreach (Card card in hand)
            {
                card.transform.localScale = new Vector3(1, 1, 1);
            }
            Canvas.ForceUpdateCanvases();
        }
    }
}
