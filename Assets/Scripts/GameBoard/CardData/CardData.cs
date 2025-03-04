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
        public int cost { get; set; }
        public int power { get; set; }
        public int counter { get; set; }
        public string trigger { get; set; }
        public CardType cardType { get; set; }
        public CharacterType characterType { get; set; }
        public Attributes attribute { get; set; }
        public Colors color { get; set; }
        public int id { get; set; }
        public bool active { get; set; } = false;
        public string customCardID { get; set; }
        public string playerName { get; set; }
        public string gameCustomID { get; set; }
        public int gameID { get; set; }

        public string currentParent { get; set; }
    }
}
