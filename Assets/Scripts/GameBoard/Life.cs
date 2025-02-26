using System.Collections;
using System.Collections.Generic;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;
using UnityEngine.XR;

namespace TCGSim
{
    public class Life : MonoBehaviour
    {
        public PlayerBoard playerBoard { get; private set; }
        public List<Card> lifeCards { get; private set; } = new List<Card>();

        //private int bottomLifePos = 0;
        private int currentTopLifePos = -45;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Init(PlayerBoard playerBoard)
        {
            this.playerBoard = playerBoard;
        }

        public void AddCardToLife(Card card)
        {
            card.transform.position = this.transform.position;
            card.gameObject.transform.Translate(0, currentTopLifePos, 0);
            currentTopLifePos += 30;
            card.transform.Rotate(0,0, 90);
            card.transform.SetParent(this.transform);
            card.SetCardVisibility(CardVisibility.NONE);
            lifeCards.Add(card);
        }
    }
}
