using System.Collections;
using System.Collections.Generic;
using TCGSim.CardScripts;
using TCGSim;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;

public abstract class Board : MonoBehaviour
{
    public string boardName { get; protected set; }
    public string playerName { get; protected set; } = "Default";
    public string gameCustomID { get; protected set; } = "TEST";

    /// <summary>
    ///  Prefabs for the player board
    /// </summary>
    #region Prefabs
    [SerializeField]
    public GameObject handPrefab { get; protected set; }

    [SerializeField]
    public GameObject characterAreaPrefab { get; protected set; }

    [SerializeField]
    public GameObject costAreaPrefab { get; protected set; }

    [SerializeField]
    public GameObject stageAreaPrefab { get; protected set; }

    [SerializeField]
    public GameObject deckPrefab { get; protected set; }

    [SerializeField]
    public GameObject leaderPrefab { get; protected set; }

    [SerializeField]
    public GameObject trashPrefab { get; protected set; }

    [SerializeField]
    public GameObject cardPrefab { get; protected set; }

    [SerializeField]
    public GameObject lifePrefab { get; protected set; }

    [SerializeField]
    public GameObject keepBtnPrefab { get; protected set; }

    [SerializeField]
    public GameObject mulliganBtnPrefab { get; protected set; }

    [SerializeField]
    public GameObject donDeckPrefab { get; protected set; }

    [SerializeField]
    public GameObject donPrefab { get; protected set; }

    [SerializeField]
    public GameObject endOfTurnBtnPrefab { get; protected set; }

    [SerializeField]
    public GameObject noBlockBtnPrefab { get; protected set; }

    [SerializeField]
    public GameObject noMoreCounterBtnPrefab { get; protected set; }
    #endregion


    /// <summary>
    ///  Objects of the player board
    /// </summary>
    #region Objects
    public Transform playerHand { get; protected set; }
    public Hand handObject { get; protected set; }
    public GameObject deckObject { get; protected set; }
    public GameObject stageObject { get; protected set; }
    public GameObject leaderObject { get; protected set; }
    public GameObject trashObject { get; protected set; }
    public CostArea costAreaObject { get; protected set; }
    public GameObject donDeckObject { get; protected set; }
    public Life lifeObject { get; protected set; }
    public Button keepBtn { get; protected set; }
    public Button mulliganBtn { get; protected set; }
    public Button endOfTurnBtn { get; protected set; }
    public Button noBlockBtn { get; protected set; }
    public Button noMoreCounterBtn { get; protected set; }
    public Button testBtn { get; protected set; }

    public CharacterArea characterAreaObject { get; protected set; }
    #endregion

    /// <summary>
    ///  Cards of the player board (which doesn't stored in another object)
    /// </summary>
    #region Cards
    public List<string> deckString { get; protected set; }
    public List<Card> deckCards { get; protected set; } = new List<Card>();
    public List<Card> donCardsInDeck { get; protected set; } = new List<Card>();
    public List<Card> characterAreaCards { get; protected set; } = new List<Card>();

    public List<Card> donInCostArea { get; protected set; } = new List<Card>();
    #endregion

    //public ServerCon serverCon { get; protected set; }
    public int activeDon { get; protected set; } = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void Init(string boardName, string gameCustomID, string playerName)
    {
        this.boardName = boardName;
        this.gameCustomID = gameCustomID;
        this.playerName = playerName;
        Debug.Log(boardName + " called init " + playerName);
        GameManager.OnGameStateChange += GameManagerOnGameStateChange;
        GameManager.OnPlayerTurnPhaseChange += GameManagerOnPlayerTurnPhaseChange;
        GameManager.OnBattlePhaseChange += GameManagerOnBattlePhaseChange;
    }

    public virtual void Init(string boardName, string gameCustomID)
    {
        this.boardName = boardName;
        this.gameCustomID = gameCustomID;
        GameManager.OnGameStateChange += GameManagerOnGameStateChange;
        GameManager.OnPlayerTurnPhaseChange += GameManagerOnPlayerTurnPhaseChange;
        GameManager.OnBattlePhaseChange += GameManagerOnBattlePhaseChange;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChange -= GameManagerOnGameStateChange;
        GameManager.OnPlayerTurnPhaseChange -= GameManagerOnPlayerTurnPhaseChange;
        GameManager.OnBattlePhaseChange -= GameManagerOnBattlePhaseChange;
    }

    public virtual void GameManagerOnGameStateChange(GameState state)
    {

    }

    public virtual void GameManagerOnPlayerTurnPhaseChange(PlayerTurnPhases turnPhase)
    {

    }
    public virtual void GameManagerOnBattlePhaseChange(BattlePhases battlePhase, Card attacker, Card attacked)
    {

    }

    public virtual void InitPrefabs(GameObject handPrefab, GameObject characterAreaPrefab, GameObject costAreaPrefab, GameObject stageAreaPrefab,
        GameObject deckPrefab, GameObject leaderPrefab, GameObject trashPrefab, GameObject cardPrefab, GameObject lifePrefab,
        GameObject keepBtnPrefab, GameObject mulliganBtnPrefab, GameObject donDeckPrefab, GameObject donPrefab, GameObject endOfTurnBtnPrefab,
        GameObject noBlockBtnPrefab, GameObject noMoreCounterBtnPrefab)
    {
        this.handPrefab = handPrefab;
        this.characterAreaPrefab = characterAreaPrefab;
        this.costAreaPrefab = costAreaPrefab;
        this.stageAreaPrefab = stageAreaPrefab;
        this.deckPrefab = deckPrefab;
        this.leaderPrefab = leaderPrefab;
        this.trashPrefab = trashPrefab;
        this.cardPrefab = cardPrefab;
        this.lifePrefab = lifePrefab;
        this.keepBtnPrefab = keepBtnPrefab;
        this.mulliganBtnPrefab = mulliganBtnPrefab;
        this.donDeckPrefab = donDeckPrefab;
        this.donPrefab = donPrefab;
        this.endOfTurnBtnPrefab = endOfTurnBtnPrefab;
        this.noBlockBtnPrefab = noBlockBtnPrefab;
        this.noMoreCounterBtnPrefab = noMoreCounterBtnPrefab;
    }

    public virtual void LoadBoardElements()
    {
        CreateCharacterArea();
        CreateLeaderArea();
        CreateStageArea();
        CreateTrashArea();
        CreateCostArea();
        CreateDons();
        CreateDeck();
        CreateLife();
        CreateHand();
    }

    public void CreateHand()
    {
        handObject = Instantiate(handPrefab, this.gameObject.transform).GetComponent<Hand>();
        playerHand = handPrefab.transform;
        handObject.Init(this);
    }

    public void CreateDeck()
    {
        deckObject = Instantiate(deckPrefab, this.gameObject.transform);
    }

    public void CreateLife()
    {
        lifeObject = Instantiate(lifePrefab, this.gameObject.transform).GetComponent<Life>();
        lifeObject.Init(this);
    }

    public void CreateDons()
    {
        donDeckObject = Instantiate(donDeckPrefab, this.gameObject.transform);
        for (int i = 0; i < 10; i++)
        {
            DonCard donCard = Instantiate(donPrefab, this.donDeckObject.transform).GetComponent<DonCard>();
            donCard.Init(handObject, "DONCARD" + i);
            donCard.transform.SetAsFirstSibling();
            donCardsInDeck.Add(donCard);
        }
        //donDeckObject.transform.GetChild(donDeckObject.transform.childCount-1).GetComponent<Card>().ChangeDraggable(true);
    }
    public void CreateCostArea()
    {
        costAreaObject = Instantiate(costAreaPrefab, this.gameObject.transform).GetComponent<CostArea>();
    }

    public void CreateStageArea()
    {
        stageObject = Instantiate(stageAreaPrefab, this.gameObject.transform);
    }

    public void CreateLeaderArea()
    {
        if (leaderObject == null)
        {
            leaderObject = Instantiate(leaderPrefab, this.gameObject.transform);
        }
    }

    public void CreateTrashArea()
    {
        trashObject = Instantiate(trashPrefab, this.gameObject.transform);
    }

    public void CreateCharacterArea()
    {
        characterAreaObject = Instantiate(characterAreaPrefab, this.gameObject.transform).GetComponent<CharacterArea>();
    }

    public GameObject GetParentByNameString(string parentName)
    {
        switch (parentName.Replace("(Clone)", ""))
        {
            case "Deck":
                return deckObject.gameObject;
            case "Hand":
                return handObject.gameObject;
            case "Life":
                return lifeObject.gameObject;
            case "CharacterArea":
                return characterAreaObject.gameObject;
            case "StageArea":
                return stageObject.gameObject;
            case "CostArea":
                return costAreaObject.gameObject;
            case "LeaderArea":
                return leaderObject.gameObject;
            case "TrashArea":
                return trashObject.gameObject;
            default:
                return this.gameObject;

        }   
    }

    public void SetCardParentByNameString(string parentName, Card card)
    {
        switch (parentName.Replace("(Clone)", ""))
        {
            case "Deck":
                card.transform.SetParent(this.deckObject.transform);
                break;
            case "Hand":
                card.transform.SetParent(this.handObject.transform);
                break;
            case "Life":
                lifeObject.AddCardToLife(card);
                break;
            case "CharacterArea":
                card.transform.SetParent(this.characterAreaObject.transform);
                card.FlipCard();
                break;
            default:
                break;

        }
    }
    public void enableDraggingOnTopDonCard()
    {
        if (donDeckObject.transform.childCount > 0)
        {
            donDeckObject.transform.GetChild(donDeckObject.transform.childCount - 1).GetComponent<Card>().ChangeDraggable(true);
        }
    }

    public void enableDraggingOnTopTwoDonCard()
    {
        if (donDeckObject.transform.childCount > 1)
        {
            donDeckObject.transform.GetChild(donDeckObject.transform.childCount - 1).GetComponent<Card>().ChangeDraggable(true);
            donDeckObject.transform.GetChild(donDeckObject.transform.childCount - 2).GetComponent<Card>().ChangeDraggable(true);
        }
        if(donDeckObject.transform.childCount == 1)
        {
            donDeckObject.transform.GetChild(donDeckObject.transform.childCount - 1).GetComponent<Card>().ChangeDraggable(true);
        }
    }

    public void SetName(string name)
    {
        this.playerName = name;
    }

}
