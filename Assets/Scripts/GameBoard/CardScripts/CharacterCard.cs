using System.Collections;
using System.Collections.Generic;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;

public class CharacterCard : Card
{
    public double power { get; set; }
    public double counter { get; set; }
    public string trigger { get; set; }
    public CharacterType characterType { get; set; }
    public Attributes attribute { get; set; }

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
}
