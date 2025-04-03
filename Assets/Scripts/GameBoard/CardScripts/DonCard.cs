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

        }

        public void RestDon()
        {
            if (this.cardData.active)
            {
                this.cardData.active = false;
                this.draggable = false;
                this.transform.Rotate(0, 0, 90);
            }           
        }

        public void RestandDon()
        {
            if (!this.cardData.active)
            {
                this.cardData.active = true;
                this.draggable = true;
                this.transform.Rotate(0, 0, -90);
            }          
        }

    }
}
