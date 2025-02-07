using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TCGSim
{ 
    public class PlayerBoard : MonoBehaviour
    {
        public string boardName;

        [SerializeField]
        private GameObject handPrefab;

        [SerializeField]
        private GameObject characterAreaPrefab;


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
        
        }
        public void CreateHand() 
        {
            Hand p1Hand = Instantiate(handPrefab, this.gameObject.transform).GetComponent<Hand>();
            p1Hand.DrawCard();
            p1Hand.DrawCard();
            p1Hand.DrawCard();
            p1Hand.DrawCard();
            p1Hand.DrawCard();
        }

        public void Init(string boardName)
        {
            this.name = boardName;
        }
    }
}
