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

        public void CreateStartingHand(List<string> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Card card = Instantiate(cardPrefab, this.gameObject.transform).GetComponent<Card>();
                card.changeSpriteImgTo(cards[i]);
                hand.Add(card);
            }      
        }
    }
}
