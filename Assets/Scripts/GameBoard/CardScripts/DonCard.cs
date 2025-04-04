using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace TCGSim.CardScripts
{

    public class DonCard : Card
    {
        private bool rested = false;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            CheckCardVisibility();
        }

        public void RestDon()
        {
            if (!rested)
            {
                this.cardData.active = false;
                this.draggable = false;
                this.transform.Rotate(0, 0, 90);
                rested = true;
            }
        }

        public void RestandDon()
        {
            if (rested)
            {
                this.cardData.active = true;
                this.draggable = true;
                this.transform.Rotate(0, 0, -90);
            } 
        }

    }
}
