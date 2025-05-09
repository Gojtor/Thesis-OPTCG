using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace TCGSim.CardScripts
{

    public class DonCard : Card
    {

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
                this.transform.rotation = Quaternion.Euler(0, 0, 90);
                this.rested = true;
            }
        }

        public void RestandDon()
        {
            if (rested)
            {
                this.cardData.active = true;
                this.draggable = true;
                this.transform.rotation = Quaternion.Euler(0, 0, 0);
                this.rested = false;
            } 
        }

        public void EnemyRestDon()
        {
            if (!rested)
            {
                this.cardData.active = false;
                this.draggable = false;
                this.transform.rotation = Quaternion.Euler(0, 0, 90);
                this.rested = true;
            }
        }

        public void EnemyRestandDon()
        {
            if (rested)
            {
                this.cardData.active = true;
                this.draggable = true;
                this.transform.rotation = Quaternion.Euler(0, 0, 180);
                this.rested = false;
            }
        }

    }
}
