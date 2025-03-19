using System.Collections;
using System.Collections.Generic;
using TCGSim.CardResources;
using UnityEngine;

namespace TCGSim.CardResources
{
    [System.Serializable]
    public class CardData
    {
        public string cardID { get; set; } = "#";
        public string cardName { get; set; } = "#";
        public string effect { get; set; } = "#";
        public int cost { get; set; } = -1;
        public int power { get; set; } = -1;
        public int counter { get; set; } = -1;
        public string trigger { get; set; } = "#";
        public CardType cardType { get; set; } = CardType.DON;
        public CharacterType characterType { get; set; } = CharacterType.NONE;
        public Attributes attribute { get; set; } = Attributes.NONE;
        public Colors color { get; set; } = Colors.Red;
        public bool active { get; set; } = false;
        public string customCardID { get; set; } = "#";
        public string playerName { get; set; } = "#";
        public string gameCustomID { get; set; } = "#";
        public int gameID { get; set; } = -1;

        public string currentParent { get; set; } = "#";
    }
}
