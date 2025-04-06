using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;
using UnityEngine.XR;

namespace TCGSim
{
    public class Life : MonoBehaviour
    {
        public Board board { get; private set; }
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

        public void Init(Board board)
        {
            this.board = board;
        }

        public void AddCardToLife(Card card)
        {
            if (currentTopLifePos == 105)
            {
                currentTopLifePos = -45;
            }
            card.transform.position = this.transform.position;
            card.gameObject.transform.Translate(0, currentTopLifePos, 0);
            currentTopLifePos += 30;
            card.transform.Rotate(0,0, 90);
            card.transform.SetParent(this.transform);
            card.SetCardVisibility(CardVisibility.NONE);
            lifeCards.Add(card);
        }

        public void TakeLife(Card card)
        {
            card.transform.Rotate(0, 0, -90);
            card.SetCardVisibility(CardVisibility.PLAYERBOARD);
            card.transform.SetParent(PlayerBoard.Instance.handObject.transform);
            card.transform.position = card.transform.parent.position;
            lifeCards.Remove(card);
        }
    }
}
