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
    public static EnemyBoard Instance { get; private set; }
    private List<Card> cards;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (deckObject != null)
        {
            Debug.Log(boardName + "- In hand:" + handObject.transform.childCount + ", in deck: " + deckObject.transform.childCount + ", in life: " + lifeObject.transform.childCount);
        }
    }

    public override void Init(string boardName, string gameCustomID)
    {
        base.Init(boardName, gameCustomID);
        Debug.Log(boardName);  
    }

    public override async void GameManagerOnGameStateChange(GameState state)
    {
        if (state == GameState.STARTINGPHASE)
        {
            LoadBoardElements();
            cards = await CreateCardsFromDeck();
        }
    }

    public async Task<List<Card>> CreateCardsFromDeck()
    {
        List<Card> deck = new List<Card>();
        List<CardData> cardsFromDB = await ServerCon.Instance.GetAllCardByGameID("TEST");
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
            card.Init();
            this.SetCardParentByNameString(cardData.currentParent,card);
            deck.Add(card);
        }
        return deck;
    }
}
