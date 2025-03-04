using System.Collections;
using System.Collections.Generic;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;

public class EventCard : Card
{
    public string trigger { get; set; } = "#";
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckCardVisibility();
    }

    public override CardData TurnCardToCardData()
    {
        CardData cardData = base.TurnCardToCardData();
        cardData.power = 0;
        cardData.counter = 0;
        cardData.trigger = this.trigger;
        cardData.cardType = this.cardType;
        cardData.characterType = CharacterType.NONE;
        cardData.attribute = Attributes.NONE;
        return cardData;
    }
}
