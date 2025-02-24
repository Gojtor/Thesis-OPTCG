using System.Collections;
using System.Collections.Generic;
using TCGSim.CardResources;
using UnityEngine;

namespace TCGSim.CardResources
{
    [System.Serializable]
    public class CardData
    {
        public string cardID { get; set; }
        public string cardName { get; set; }
        public string effect { get; set; }
        public double cost { get; set; }
        public double power { get; set; }
        public double counter { get; set; }
        public string trigger { get; set; }
        public CardType cardType { get; set; }
        public CharacterType characterType { get; set; }
        public Attributes attribute { get; set; }
        public Colors color { get; set; }
        public int id { get; set; }
    }
}
