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
    private List<Card> cards = new List<Card>();
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

    public override void GameManagerOnGameStateChange(GameState state)
    {
        if (state == GameState.STARTINGPHASE)
        {
            LoadBoardElements();
            CreateStartingPhaseBoard();
        }
    }

    public void CreateStartingPhaseBoard()
    {
        GameObject deckCardObj = Instantiate(cardPrefab, deckObject.transform);
        deckCardObj.AddComponent<CharacterCard>();
        Card deckCard = deckCardObj.GetComponent<CharacterCard>();
        cards.Add(deckCard);
        for (int i = 0; i < 5; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, handObject.transform);
            cardObj.AddComponent<CharacterCard>();
            Card card = cardObj.GetComponent<CharacterCard>();
            cards.Add(card);
        }
    }

    public async void CreateAfterStartingPhase()
    {
        foreach (Card card in cards)
        {
            Destroy(card.gameObject);
        }
        cards.Clear();
        //cards = await GetEnemyCardsFromGameDB();
    }

    public async Task<List<Card>> GetEnemyCardsFromGameDB()
    {
        List<Card> deck = new List<Card>();
        List<CardData> cardsFromDB = await ServerCon.Instance.GetAllCardByGameIDAndPlayerName(this.gameCustomID,this.playerName);
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
