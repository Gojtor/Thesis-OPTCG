using System.Collections;
using System.Collections.Generic;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;

public class CharacterCard : Card
{
    public int power { get; set; } = 0;
    public int counter { get; set; } = 0;
    public string trigger { get; set; } = "#";
    public CharacterType characterType { get; set; } = CharacterType.NONE;
    public Attributes attribute { get; set; } = Attributes.NONE;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckCardVisibility();
    }

    public override void LoadDataFromCardData(CardData cardData)
    {
        base.LoadDataFromCardData(cardData);
        this.power = cardData.power;
        this.counter = cardData.counter;
        this.trigger = cardData.trigger;
        this.cardType = cardData.cardType;
        this.characterType = cardData.characterType;
        this.attribute = cardData.attribute;
    }

    public override CardData TurnCardToCardData()
    {
        CardData cardData= base.TurnCardToCardData();
        cardData.power = this.power;
        cardData.counter = this.counter;
        cardData.trigger = this.trigger;
        cardData.cardType = this.cardType;
        cardData.characterType = this.characterType;
        cardData.attribute = this.attribute;
        return cardData;
    }
}
