using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace TCGSim
{
    public class Hand : MonoBehaviour
    {
        [SerializeField]
        private GameObject cardPrefab;

        [SerializeField]
        private GameObject keepBtnPrefab;

        [SerializeField]
        private GameObject mulliganBtnPrefab;

        PlayerBoard playerBoard;
        Button keepBtn;
        Button mulliganBtn;

        List<Card> hand = new List<Card>();
        // Start is called before the first frame update
        void Start()
        {
            
            keepBtn = Instantiate(keepBtnPrefab, this.transform.parent).GetComponent<Button>();
            mulliganBtn = Instantiate(mulliganBtnPrefab, this.transform.parent).GetComponent<Button>();
            if (keepBtn != null)
            {
                keepBtn.onClick.AddListener(ScaleHandBackFromStartingHand);
            }
            if (mulliganBtn != null)
            {
                mulliganBtn.onClick.AddListener(ScaleHandForStartingHand);
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void Init(PlayerBoard playerBoard)
        {
            this.playerBoard = playerBoard;
        }

        public void AddCardToHand(Card card)
        {
            card.transform.SetParent(this.transform);
            card.raycastTargetChange(true);
            switch (playerBoard.boardName)
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

        public void ScaleHandForStartingHand()
        {
            Debug.Log("Click");
            RectTransform rectTransform = this.transform.GetComponent<RectTransform>();
            Debug.Log($"Eredeti pozíció: {rectTransform.anchoredPosition}");
            Debug.Log($"Eredeti méret: {this.transform.localScale}");

            rectTransform.anchoredPosition = new Vector2(0, 150);
            this.transform.localScale = new Vector3(3, 3, 3);

            Debug.Log($"Új pozíció: {rectTransform.anchoredPosition}");
            Debug.Log($"Új méret: {this.transform.localScale}");

            foreach (Card card in hand)
            {
                card.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            }
            Canvas.ForceUpdateCanvases();
        }

        public void ScaleHandBackFromStartingHand()
        {
            RectTransform rectTransform = this.transform.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, -105);
            this.transform.localScale = new Vector3(1, 1, 1);
            foreach (Card card in hand)
            {
                card.transform.localScale = new Vector3(1, 1, 1);
            }
            Canvas.ForceUpdateCanvases();
        }
    }
}
