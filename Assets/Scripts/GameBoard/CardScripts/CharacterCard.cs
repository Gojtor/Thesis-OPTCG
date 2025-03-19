using System.Collections;
using System.Collections.Generic;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;

public class CharacterCard : Card
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

    public override void LoadDataFromCardData(CardData cardData)
    {
        this.cardData = cardData;
    }
}
