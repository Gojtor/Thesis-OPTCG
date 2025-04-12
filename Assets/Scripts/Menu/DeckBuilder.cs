using Assets.Scripts.ServerCon;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCGSim;
using TCGSim.CardResources;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.U2D;
using UnityEngine.UI;

public class DeckBuilder : MonoBehaviour
{
    public string serverUrl { get; } = Environment.GetEnvironmentVariable("SERVER_ADDRESS") ?? "http://localhost:5000";

    [SerializeField]
    public GameObject availableCardsViewPrefab;
    [SerializeField]
    public GameObject selectedCardsViewPrefab;
    [SerializeField]
    public GameObject imageButtonPrefab;
    [SerializeField]
    public GameObject cardMagnifierPrefab;
    [SerializeField]
    public GameObject selectLeaderPrefab;
    [SerializeField]
    public GameObject selectCardsPrefab;
    [SerializeField]
    public GameObject userDeckDropDownPrefab;
    [SerializeField]
    public GameObject cardsInSelectedAreaTextPrefab;
    [SerializeField]
    public GameObject createDeckBtnPrefab;
    [SerializeField]
    public GameObject saveDeckBtnPrefab;
    [SerializeField]
    public GameObject deckNameInputPrefab;

    private List<CardData> selectedCards = new List<CardData>();
    private GameObject selectedCardsView;
    private GameObject availableCardsView;
    private GameObject availableCardsContent;
    private GameObject selectedCardsContent;
    public static GameObject cardMagnifier;
    private Sprite[] leaderSprites;
    private Sprite[] cardSprites;
    private TextMeshProUGUI selectLeaderText;
    private TextMeshProUGUI selectCardsText;
    private TextMeshProUGUI cardsInSelectedAreaText;
    private TMP_Dropdown userDecksDropDown;
    private Dictionary<string, List<CardData>> cardDataDecks;
    private Button createDeckBtn;
    private Button saveDeckBtn;
    private TMP_InputField deckNameInput;
    private string newDeckName;

    async void Start()
    {
        GameOptions.decksJson = await LoginManager.Instance.GetUserDecks("Test123");

        cardMagnifier = Instantiate(cardMagnifierPrefab, this.gameObject.transform);
        cardMagnifier.SetActive(false);

        availableCardsView = Instantiate(availableCardsViewPrefab, this.gameObject.transform);
        GameObject availableViewport = availableCardsView.transform.Find("Viewport").gameObject;
        availableViewport.AddComponent<HorizontalLayoutGroup>();
        availableCardsContent = availableViewport.transform.Find("AvailableContent").gameObject;

        selectedCardsView = Instantiate(selectedCardsViewPrefab, this.gameObject.transform);
        GameObject selectedViewport = selectedCardsView.transform.Find("Viewport").gameObject;
        selectedViewport.AddComponent<HorizontalLayoutGroup>();
        selectedCardsContent = selectedViewport.transform.Find("SelectedContent").gameObject;

        cardsInSelectedAreaText = Instantiate(cardsInSelectedAreaTextPrefab, this.gameObject.transform).GetComponent<TextMeshProUGUI>();
        cardsInSelectedAreaText.text = selectedCards.Count + "/51";

        createDeckBtn = Instantiate(createDeckBtnPrefab, this.gameObject.transform).GetComponent<Button>();
        createDeckBtn.onClick.AddListener(NewDeckCreation);
        saveDeckBtn = Instantiate(saveDeckBtnPrefab, this.gameObject.transform).GetComponent<Button>();
        saveDeckBtn.onClick.AddListener(SaveDeck);
        deckNameInput = Instantiate(deckNameInputPrefab, this.gameObject.transform).GetComponent<TMP_InputField>();
        deckNameInput.onEndEdit.AddListener(UpdateDeckName);

        userDecksDropDown = Instantiate(userDeckDropDownPrefab, this.gameObject.transform).GetComponent<TMP_Dropdown>();
        HandleUserDecks();

        HorizontalLayoutGroup layoutGroupAvailable = availableCardsContent.AddComponent<HorizontalLayoutGroup>();
        layoutGroupAvailable.childForceExpandHeight = false;
        layoutGroupAvailable.childForceExpandWidth = false;
        layoutGroupAvailable.childControlHeight = false;
        layoutGroupAvailable.childControlWidth = false;
        layoutGroupAvailable.spacing = 10;
        layoutGroupAvailable.childAlignment = TextAnchor.MiddleCenter;

        HorizontalLayoutGroup layoutGroupSelected = selectedCardsContent.AddComponent<HorizontalLayoutGroup>();
        layoutGroupSelected.childForceExpandHeight = false;
        layoutGroupSelected.childForceExpandWidth = false;
        layoutGroupSelected.childControlHeight = false;
        layoutGroupSelected.childControlWidth = false;
        layoutGroupSelected.spacing = 10;
        layoutGroupSelected.childAlignment = TextAnchor.MiddleCenter;

        selectCardsText = Instantiate(selectCardsPrefab, this.gameObject.transform).GetComponent<TextMeshProUGUI>();
        selectCardsText.gameObject.SetActive(false);
        selectLeaderText = Instantiate(selectLeaderPrefab, this.gameObject.transform).GetComponent<TextMeshProUGUI>();

        leaderSprites = Resources.LoadAll<Sprite>("Cards/Leaders");
        cardSprites = Resources.LoadAll<Sprite>("Cards/Cards");
        PopulateScrollViewWithLeaders();
    }

    private void SaveDeck()
    {
        if (newDeckName != null)
        {
            foreach (TMP_Dropdown.OptionData optionData in userDecksDropDown.options)
            {
                if (optionData.text == newDeckName)
                {
                    Debug.Log("Already have deck with this name!");
                    return;
                }
            }
            if (selectedCards.Count == 51)
            {
                cardDataDecks.Add(newDeckName, selectedCards);
            }
            else
            {
                Debug.Log("You must have 50 cards + a leader in your deck");
            }
        }
    }

    private void NewDeckCreation()
    {
        selectLeaderText.gameObject.SetActive(true);
        selectCardsText.gameObject.SetActive(false);
        UnloadCardsFromAvailableArea();
        UnloadCardsFromSelectedArea();
        PopulateScrollViewWithLeaders();
        PopulateUserDecksDropDown("New deck", true);
    }

    private void PopulateUserDecksDropDown(string bonusOption, bool toAdd)
    {
        List<string> deckNames = new List<string>();
        if (toAdd)
        {
            deckNames.Add(bonusOption);
        }
        deckNames.Add("Default-NoCards");
        foreach (string deck in cardDataDecks.Keys)
        {
            deckNames.Add(deck.Split(',')[0]);
        }
        userDecksDropDown.ClearOptions();
        userDecksDropDown.AddOptions(deckNames);
        userDecksDropDown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void UpdateDeckName(string s)
    {
        newDeckName = s;
    }

    public async void HandleUserDecks()
    {
        cardDataDecks = await StringDeckToCardDataDeck();
        PopulateUserDecksDropDown("", false);
    }

    private void OnDropdownChanged(int index)
    {
        string selectedValue = userDecksDropDown.options[index].text;
        selectedCards = new List<CardData>();
        UnloadCardsFromSelectedArea();
        UnloadCardsFromAvailableArea();
        if (cardDataDecks.ContainsKey(selectedValue))
        {
            LoadDeckToSelectedCards(cardDataDecks[selectedValue]);
        }
    }

    private async Task<Dictionary<string, List<CardData>>> StringDeckToCardDataDeck()
    {
        Dictionary<string, List<CardData>> cardDataDecks = new Dictionary<string, List<CardData>>();
        foreach (string deck in GameOptions.decksJson)
        {
            List<string> cards = deck.Split(',').ToList();
            List<CardData> dectToCardData = new List<CardData>();
            string deckName = cards[0];
            foreach (string samecards in cards.Skip(1))
            {
                int sameCount = int.Parse(samecards.Split('x')[0]);
                string cardID = samecards.Split('x')[1];
                CardData cardData = await GetCardByCardID(cardID);
                for (int i = 0; i < sameCount; i++)
                {
                    dectToCardData.Add(cardData);
                }
            }
            cardDataDecks.Add(deckName, dectToCardData);
        }
        return cardDataDecks;
    }

    private string CardDataDeckToStringDeck(List<CardData> cardDataDeck)
    {
        Dictionary<string, int> sameCardDict = new Dictionary<string, int>();
        string deckInString = "";
        foreach (CardData cardData in cardDataDeck)
        {
            if (sameCardDict.ContainsKey(cardData.cardID))
            {
                sameCardDict[cardData.cardID]++;
            }
            else
            {
                sameCardDict.Add(cardData.cardID, 1);
            }
        }
        List<KeyValuePair<string, int>> sortedSameCardDict = sameCardDict.OrderBy(kvp => kvp.Key).ToList();

        foreach (var item in sortedSameCardDict)
        {
            string formatted = $",{item.Key}x{item.Value}";
            deckInString = deckInString + formatted;
        }
        return deckInString;
    }

    private void LoadDeckToSelectedCards(List<CardData> cardDataDeck)
    {
        selectedCards = new List<CardData>();
        foreach (CardData cardData in cardDataDeck)
        {
            selectedCards.Add(cardData);
            GameObject newButton = Instantiate(imageButtonPrefab, selectedCardsContent.transform);
            Image img = newButton.GetComponentInChildren<Image>();
            Button btn = newButton.GetComponent<Button>();
            if (img != null)
            {
                if (cardData.cardType == CardType.LEADER)
                {
                    img.sprite = leaderSprites.Where(x => x.name == cardData.cardID).Single();
                }
                else
                {
                    img.sprite = cardSprites.Where(x => x.name == cardData.cardID).Single();
                    btn.onClick.AddListener(() => OnSelectedCardsClicked(btn, cardData));
                }

            }
            btn.gameObject.name = img.sprite.name;
        }
        cardsInSelectedAreaText.text = selectedCards.Count + "/51";

        selectLeaderText.gameObject.SetActive(false);
        selectCardsText.gameObject.SetActive(true);

        UnloadCardsFromAvailableArea();

        CardData leaderCard = selectedCards.Where(x => x.cardType == CardType.LEADER).Single();
        PopulateScrollViewWithCardsWithSameColor(leaderCard);
    }

    private void UnloadCardsFromSelectedArea()
    {
        foreach (Transform child in selectedCardsContent.transform)
        {
            Destroy(child.gameObject);
        }
        selectedCards = new List<CardData>();
        cardsInSelectedAreaText.text = selectedCards.Count + "/51";
    }

    private void UnloadCardsFromAvailableArea()
    {
        foreach (Transform child in availableCardsContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private async void PopulateScrollViewWithLeaders()
    {
        foreach (Sprite sprite in leaderSprites)
        {
            GameObject newButton = Instantiate(imageButtonPrefab, availableCardsContent.transform);
            Image img = newButton.GetComponentInChildren<Image>();
            if (img != null)
            {
                img.sprite = sprite;
            }
            Button btn = newButton.GetComponent<Button>();
            btn.gameObject.name = sprite.name;
            btn.onClick.AddListener(() => OnLeaderClicked(btn));
            selectedCards.Add(await GetCardByCardID(sprite.name));
        }
    }

    private void OnLeaderClicked(Button btn)
    {
        Debug.Log("Kép kattintva: " + btn.name);
        GameObject newButton = Instantiate(imageButtonPrefab, selectedCardsContent.transform);
        Image img = newButton.GetComponentInChildren<Image>();
        img.sprite = btn.gameObject.GetComponent<Image>().sprite;
        Button newBtn = newButton.GetComponent<Button>();
        newBtn.gameObject.name = img.sprite.name;
        btn.onClick.RemoveAllListeners();
        selectLeaderText.gameObject.SetActive(false);
        UnloadCardsFromAvailableArea();
        PopulateScrollViewWithCardsWithSameColor(selectedCards[0]);
        cardsInSelectedAreaText.text = selectedCards.Count + "/51";
    }

    private async void PopulateScrollViewWithCardsWithSameColor(CardData leaderData)
    {
        selectCardsText.gameObject.SetActive(true);
        foreach (Sprite sprite in cardSprites)
        {
            CardData cardData = await GetCardByCardID(sprite.name);
            if (cardData.color == leaderData.color)
            {
                GameObject newButton = Instantiate(imageButtonPrefab, availableCardsContent.transform);
                Image img = newButton.GetComponentInChildren<Image>();
                if (img != null)
                {
                    img.sprite = sprite;
                }
                Button btn = newButton.GetComponent<Button>();
                btn.gameObject.name = sprite.name;
                btn.onClick.AddListener(() => OnCardsClicked(btn, cardData));
            }
        }
    }

    private void OnCardsClicked(Button btn, CardData cardData)
    {
        if (selectedCards.Count > 51)
        {
            Debug.Log("Can't add more than 50 card to deck");
            return;
        }
        int sameCardCount = selectedCards.Where(x => x.cardID == btn.name).Count();
        if (sameCardCount == 4)
        {
            Debug.Log("You already have 4 of these card");
            return;
        }
        cardsInSelectedAreaText.text = selectedCards.Count + "/51";
        selectedCards.Add(cardData);
        Debug.Log("Kép kattintva: " + btn.name);
        GameObject newButton = Instantiate(imageButtonPrefab, selectedCardsContent.transform);
        Image img = newButton.GetComponentInChildren<Image>();
        img.sprite = btn.gameObject.GetComponent<Image>().sprite;
        Button newBtn = newButton.GetComponent<Button>();
        newBtn.gameObject.name = img.sprite.name;
        newBtn.onClick.AddListener(() => OnSelectedCardsClicked(newBtn, cardData));
    }

    private void OnSelectedCardsClicked(Button btn, CardData cardData)
    {
        Debug.Log("Removing from selected");
        selectedCards.Remove(cardData);
        btn.onClick.RemoveAllListeners();
        Destroy(btn.gameObject);
        cardsInSelectedAreaText.text = selectedCards.Count + "/51";
    }

    public async Task<CardData> GetCardByCardID(string cardID)
    {
        string url = serverUrl + "/api/TCG/GetCardByCardID/";
        using (UnityWebRequest request = UnityWebRequest.Get(url + cardID))
        {
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            switch (request.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(request.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(request.error);
                    break;
                case UnityWebRequest.Result.Success:
                    string jsonResponse = request.downloadHandler.text;
                    //Debug.Log("Received: " + jsonResponse);
                    return JsonConvert.DeserializeObject<CardData>(jsonResponse);
            }
            return null;
        }

    }

}
