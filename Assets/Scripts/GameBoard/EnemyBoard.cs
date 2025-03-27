using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCGSim;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;

public class EnemyBoard : Board
{
    private List<Card> cards;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(boardName + "- In hand:" + handObject.transform.childCount + ", in deck: " + deckObject.transform.childCount + ", in life: " + lifeObject.transform.childCount);
    }

    public async override void Init(string boardName, ServerCon serverCon, string gameCustomID)
    {
        base.Init(boardName, serverCon, gameCustomID);
        if (serverCon == null)
        {
            Debug.LogError("ServerCon prefab NULL after Init!", this);
        }
        Debug.Log(boardName);
        cards = await CreateCardsFromDeck();
    }

    public async Task<List<Card>> CreateCardsFromDeck()
    {
        List<Card> deck = new List<Card>();
        List<CardData> cardsFromDB = await serverCon.GetAllCardByGameID("TEST");
        foreach (CardData cardData in cardsFromDB)
        {
            GameObject cardObj;
            Card card;
            switch (cardData.cardType)
            {
                case CardType.CHARACTER:
                    cardObj = Instantiate(cardPrefab, deckObject.transform);
                    cardObj.AddComponent<CharacterCard>();
                    card = cardObj.GetComponent<CharacterCard>();
                    break;
                case CardType.STAGE:
                    cardObj = Instantiate(cardPrefab, deckObject.transform);
                    cardObj.AddComponent<StageCard>();
                    card = cardObj.GetComponent<StageCard>();
                    break;
                case CardType.EVENT:
                    cardObj = Instantiate(cardPrefab, deckObject.transform);
                    cardObj.AddComponent<EventCard>();
                    card = cardObj.GetComponent<EventCard>();
                    break;
                default:
                    cardObj = Instantiate(cardPrefab, deckObject.transform);
                    cardObj.AddComponent<Card>();
                    card = cardObj.GetComponent<Card>();
                    break;
            }
            card.LoadDataFromCardData(cardData);
            card.Init(this);
            this.SetCardParentByNameString(cardData.currentParent,card);
            card.raycastTargetChange(false);
            deck.Add(card);
        }
        return deck;
    }
}
