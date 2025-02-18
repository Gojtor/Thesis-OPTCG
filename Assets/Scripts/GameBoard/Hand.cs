using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TCGSim
{
    public class Hand : MonoBehaviour
    {
        [SerializeField]
        private GameObject cardPrefab;

        List<Card> hand = new List<Card>();
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddCardToHand(Card card)
        {
            card.transform.SetParent(this.transform);
            card.loadCardImg();
            card.raycastTargetChange(true);
            hand.Add(card);      
        }
    }
}
