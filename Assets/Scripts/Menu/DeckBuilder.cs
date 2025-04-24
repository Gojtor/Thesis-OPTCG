using Assets.Scripts.ServerCon;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    public string serverUrl { get; private set; }

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
    [SerializeField]
    public GameObject deleteDeckBtnPrefab;
    [SerializeField]
    public GameObject backToMenuBtnPrefab;

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
    private Button deleteDeckBtn;
    private Button backToMenuBtn;
    private TMP_InputField deckNameInput;
    private string newDeckName;

    public static event Action DeckBuilderSetActive;


    private void Awake()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "server_config.json");
        string configText = System.IO.File.ReadAllText(path);
        if (configText != null)
        {
            ServerSettings config = JsonUtility.FromJson<ServerSettings>(configText);
            serverUrl = config.serverUrl;
        }
        else
        {
            serverUrl = "https://gabcsika.picidolgok.hu";
        }
    }
    async void Start()
    {
        DeckBuilderSetActive += DeckBuilder_DeckBuilderSetActive;
        cardMagnifier = Instantiate(cardMagnifierPrefab, this.gameObject.transform);
        cardMagnifier.SetActive(false);

        availableCardsView = Instantiate(availableCardsViewPrefab, this.gameObject.transform);
        ScrollRect availabelScroll = availableCardsView.GetComponent<ScrollRect>();
        availabelScroll.horizontalNormalizedPosition = 0f;
        GameObject availableViewport = availableCardsView.transform.Find("Viewport").gameObject;
        availableViewport.AddComponent<HorizontalLayoutGroup>();
        availableCardsContent = availableViewport.transform.Find("AvailableContent").gameObject;

        selectedCardsView = Instantiate(selectedCardsViewPrefab, this.gameObject.transform);
        ScrollRect selectedScroll = selectedCardsView.GetComponent<ScrollRect>();
        selectedScroll.horizontalNormalizedPosition = 0f;
        GameObject selectedViewport = selectedCardsView.transform.Find("Viewport").gameObject;
        selectedViewport.AddComponent<HorizontalLayoutGroup>();
        selectedCardsContent = selectedViewport.transform.Find("SelectedContent").gameObject;

        cardsInSelectedAreaText = Instantiate(cardsInSelectedAreaTextPrefab, this.gameObject.transform).GetComponent<TextMeshProUGUI>();
        cardsInSelectedAreaText.text = selectedCards.Count + "/51";

        createDeckBtn = Instantiate(createDeckBtnPrefab, this.gameObject.transform).GetComponent<Button>();
        createDeckBtn.onClick.AddListener(NewDeckCreationWrapper);
        saveDeckBtn = Instantiate(saveDeckBtnPrefab, this.gameObject.transform).GetComponent<Button>();
        saveDeckBtn.onClick.AddListener(SaveDeckWrapper);
        deckNameInput = Instantiate(deckNameInputPrefab, this.gameObject.transform).GetComponent<TMP_InputField>();
        deckNameInput.onEndEdit.AddListener(UpdateDeckName);
        deleteDeckBtn = Instantiate(deleteDeckBtnPrefab, this.gameObject.transform).GetComponent<Button>();
        deleteDeckBtn.onClick.AddListener(DeleteDeck);
        backToMenuBtn = Instantiate(backToMenuBtnPrefab, this.gameObject.transform).GetComponent<Button>();
        backToMenuBtn.onClick.AddListener(BackToMenu);

        HorizontalLayoutGroup layoutGroupAvailable = availableCardsContent.AddComponent<HorizontalLayoutGroup>();
        layoutGroupAvailable.childForceExpandHeight = false;
        layoutGroupAvailable.childForceExpandWidth = false;
        layoutGroupAvailable.childControlHeight = false;
        layoutGroupAvailable.childControlWidth = false;
        layoutGroupAvailable.spacing = 10;
        layoutGroupAvailable.childAlignment = TextAnchor.MiddleCenter;
        RectOffset tempPaddingAvailable = layoutGroupAvailable.padding;
        tempPaddingAvailable.left += 20;
        tempPaddingAvailable.right += 20;

        HorizontalLayoutGroup layoutGroupSelected = selectedCardsContent.AddComponent<HorizontalLayoutGroup>();
        layoutGroupSelected.childForceExpandHeight = false;
        layoutGroupSelected.childForceExpandWidth = false;
        layoutGroupSelected.childControlHeight = false;
        layoutGroupSelected.childControlWidth = false;
        layoutGroupSelected.spacing = 10;
        layoutGroupSelected.childAlignment = TextAnchor.MiddleCenter;
        RectOffset tempPaddingSelected = layoutGroupSelected.padding;
        tempPaddingSelected.left += 20;
        tempPaddingSelected.right += 20;

        selectCardsText = Instantiate(selectCardsPrefab, this.gameObject.transform).GetComponent<TextMeshProUGUI>();
        selectCardsText.gameObject.SetActive(false);
        selectLeaderText = Instantiate(selectLeaderPrefab, this.gameObject.transform).GetComponent<TextMeshProUGUI>();

        leaderSprites = Resources.LoadAll<Sprite>("Cards/Leaders");
        cardSprites = Resources.LoadAll<Sprite>("Cards/Cards");
        userDecksDropDown = Instantiate(userDeckDropDownPrefab, this.gameObject.transform).GetComponent<TMP_Dropdown>();
        await HandleUserDecks();
        await PopulateScrollViewWithLeaders();
    }

    private void OnDestroy()
    {
        DeckBuilderSetActive -= DeckBuilder_DeckBuilderSetActive;
    }

    private void DeckBuilder_DeckBuilderSetActive()
    {
        int defaultIndex = userDecksDropDown.options.FindIndex(x => x.text == "Default-NoCards");
        if (defaultIndex != -1)
        {
            userDecksDropDown.value = defaultIndex;
            userDecksDropDown.RefreshShownValue();
        }
    }

    public static void InvokeSetActive()
    {
        DeckBuilderSetActive?.Invoke();
    }

    private void BackToMenu()
    {
        this.gameObject.SetActive(false);
        Menu.InvokeBackToMenuFromDeckBuilder();
    }

    private async void DeleteDeck()
    {
        int selectedIndex = userDecksDropDown.value;
        string currentlySelectedDeck = userDecksDropDown.options[selectedIndex].text;
        if (currentlySelectedDeck != "Default-NoCards" && currentlySelectedDeck != "ST01-DefaultDeck")
        {
            var optionToRemove = userDecksDropDown.options.FirstOrDefault(x => x.text == currentlySelectedDeck);

            if (optionToRemove != null)
            {
                cardDataDecks.Remove(currentlySelectedDeck);
                userDecksDropDown.options.Remove(optionToRemove);
                userDecksDropDown.RefreshShownValue();
                string deckToRemove = GameOptions.decksJson.FirstOrDefault(x => x.Split(',')[0] == currentlySelectedDeck);
                GameOptions.decksJson.Remove(deckToRemove);
                if (!GameOptions.userName.Contains("Guest"))
                {
                    await LoginManager.Instance.RemoveFromUserDeck(GameOptions.userName, currentlySelectedDeck);
                }
                int defaultIndex = userDecksDropDown.options.FindIndex(x => x.text == "Default-NoCards");
                if (defaultIndex != -1)
                {
                    userDecksDropDown.value = defaultIndex;
                    userDecksDropDown.RefreshShownValue();
                }
            }
        }
    }

    public async void SaveDeckWrapper()
    {
        await SaveDeck();
    }

    public async Task SaveDeck()
    {
        if (newDeckName != null)
        {
            if (selectedCards.Count == 51)
            {
                int selectedIndex = userDecksDropDown.value;
                string currentlySelectedDeck = userDecksDropDown.options[selectedIndex].text;
                if (cardDataDecks.ContainsKey(newDeckName))
                {
                    if (currentlySelectedDeck == "New deck")
                    {
                        newDeckName = newDeckName + cardDataDecks.Count(x => x.Key.Contains(newDeckName));
                        cardDataDecks.Add(newDeckName, selectedCards);
                        TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(newDeckName);
                        userDecksDropDown.options[selectedIndex].text = newDeckName;
                        userDecksDropDown.RefreshShownValue();
                    }
                    else
                    {
                        newDeckName = newDeckName + cardDataDecks.Count(x => x.Key.Contains(newDeckName));
                        cardDataDecks.Add(newDeckName, selectedCards);
                        TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(newDeckName);
                        userDecksDropDown.options.Insert(0, newOption);
                        userDecksDropDown.value = 0;
                        userDecksDropDown.RefreshShownValue();
                    }
                }
                else
                {
                    cardDataDecks.Add(newDeckName, selectedCards);
                    TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(newDeckName);
                    userDecksDropDown.options.Insert(0, newOption);
                    userDecksDropDown.value = 0;
                    userDecksDropDown.RefreshShownValue();
                }
                string newDeck = newDeckName + CardDataDeckToStringDeck(selectedCards);
                GameOptions.decksJson.Add(newDeck);
                if (!GameOptions.userName.Contains("Guest"))
                {
                    await LoginManager.Instance.AddToUserDecks(GameOptions.userName, newDeck);
                }
            }
            else
            {
                Debug.Log("You must have 50 cards + a leader in your deck");
            }
        }
        else
        {
            if (selectedCards.Count == 51)
            {
                int selectedIndex = userDecksDropDown.value;
                string currentlySelectedDeck = userDecksDropDown.options[selectedIndex].text;
                if (cardDataDecks.ContainsKey(currentlySelectedDeck))
                {
                    newDeckName = currentlySelectedDeck + cardDataDecks.Count(x => x.Key.Contains(currentlySelectedDeck));
                    cardDataDecks.Add(newDeckName, selectedCards);
                    TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData(newDeckName);
                    userDecksDropDown.options.Insert(0, newOption);
                    userDecksDropDown.value = 0;
                    userDecksDropDown.RefreshShownValue();
                    string newDeck = newDeckName + CardDataDeckToStringDeck(selectedCards);
                    GameOptions.decksJson.Add(newDeck);
                    if (!GameOptions.userName.Contains("Guest"))
                    {
                        await LoginManager.Instance.AddToUserDecks(GameOptions.userName, newDeck);
                    }
                }
            }
        }
        newDeckName = null;
    }

    public async void NewDeckCreationWrapper()
    {
        await NewDeckCreation();
    }

    public async Task NewDeckCreation()
    {
        selectLeaderText.gameObject.SetActive(true);
        selectCardsText.gameObject.SetActive(false);
        UnloadCardsFromAvailableArea();
        UnloadCardsFromSelectedArea();
        await PopulateScrollViewWithLeaders();
        TMP_Dropdown.OptionData newOption = new TMP_Dropdown.OptionData("New deck");
        userDecksDropDown.options.Insert(0, newOption);
        userDecksDropDown.RefreshShownValue();
        userDecksDropDown.value = 0;
    }

    private void PopulateUserDecksDropDown()
    {
        List<string> deckNames = new List<string>();
        deckNames.Add("Default-NoCards");
        foreach (string deck in cardDataDecks.Keys)
        {
            deckNames.Add(deck.Split(',')[0]);
        }
        userDecksDropDown.ClearOptions();
        userDecksDropDown.AddOptions(deckNames);
        userDecksDropDown.onValueChanged.AddListener(OnDropdownChangedWrapper);
    }

    private void UpdateDeckName(string s)
    {
        newDeckName = s;
    }

    public async Task HandleUserDecks()
    {
        cardDataDecks = await StringDeckToCardDataDeck();
        PopulateUserDecksDropDown();
    }

    public async void OnDropdownChangedWrapper(int index)
    {
        await OnDropdownChanged(index);
    }
    public async Task OnDropdownChanged(int index)
    {
        string selectedValue = userDecksDropDown.options[index].text;
        selectedCards = new List<CardData>();
        UnloadCardsFromSelectedArea();
        UnloadCardsFromAvailableArea();
        if (cardDataDecks.ContainsKey(selectedValue) && cardDataDecks[selectedValue].Count == 51)
        {
            selectCardsText.gameObject.SetActive(true);
            selectLeaderText.gameObject.SetActive(false);
            await LoadDeckToSelectedCards(cardDataDecks[selectedValue]);
        }
        else if (!cardDataDecks.ContainsKey(selectedValue) || cardDataDecks[selectedValue].Count == 0)
        {
            selectCardsText.gameObject.SetActive(false);
            selectLeaderText.gameObject.SetActive(true);
            await PopulateScrollViewWithLeaders();
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
            string formatted = $",{item.Value}x{item.Key}";
            deckInString = deckInString + formatted;
        }
        return deckInString;
    }

    private async Task LoadDeckToSelectedCards(List<CardData> cardDataDeck)
    {
        selectedCards = new List<CardData>();
        List<CardData> givenDeck = cardDataDeck.OrderBy(x => x.cardID).Reverse().ToList();
        foreach (CardData cardData in givenDeck)
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
        await PopulateScrollViewWithCardsWithSameColor(leaderCard);
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
        if(availableCardsContent.gameObject==null || this == null) { return; }
        if (availableCardsContent.gameObject != null || this.gameObject != null)
        {
            foreach (Transform child in availableCardsContent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public async Task PopulateScrollViewWithLeaders()
    {
        if (this == null || availableCardsContent == null) { return; }
        UnloadCardsFromAvailableArea();
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
            btn.onClick.AddListener(() => OnLeaderClickedWrapper(btn));
        }
        await Task.CompletedTask;
    }

    public async void OnLeaderClickedWrapper(Button btn)
    {
        await OnLeaderClicked(btn);
    }

    public async Task OnLeaderClicked(Button btn)
    {
        Debug.Log("Kép kattintva: " + btn.name);
        GameObject newButton = Instantiate(imageButtonPrefab, selectedCardsContent.transform);
        Image img = newButton.GetComponentInChildren<Image>();
        img.sprite = btn.gameObject.GetComponent<Image>().sprite;
        selectedCards.Add(await GetCardByCardID(img.sprite.name));
        Button newBtn = newButton.GetComponent<Button>();
        newBtn.gameObject.name = img.sprite.name;
        btn.onClick.RemoveAllListeners();
        selectLeaderText.gameObject.SetActive(false);
        UnloadCardsFromAvailableArea();
        await PopulateScrollViewWithCardsWithSameColor(selectedCards[0]);
        cardsInSelectedAreaText.text = selectedCards.Count + "/51";
    }

    public async Task PopulateScrollViewWithCardsWithSameColor(CardData leaderData)
    {
        selectCardsText.gameObject.SetActive(true);
        UnloadCardsFromAvailableArea();
        foreach (Sprite sprite in cardSprites)
        {
            CardData cardData = await GetCardByCardID(sprite.name);
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                string leaderColor = leaderData.color.ToString();
                string cardColor = cardData.color.ToString();
                if (leaderColor.Contains(cardColor))
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
            });
        }
    }
    public void OnCardsClicked(Button btn, CardData cardData)
    {
        if (selectedCards.Count > 50)
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
        selectedCards.Add(cardData);
        cardsInSelectedAreaText.text = selectedCards.Count + "/51";
        Debug.Log("Kép kattintva: " + btn.name);
        GameObject newButton = Instantiate(imageButtonPrefab, selectedCardsContent.transform);
        newButton.transform.SetAsFirstSibling();
        Image img = newButton.GetComponentInChildren<Image>();
        img.sprite = btn.gameObject.GetComponent<Image>().sprite;
        Button newBtn = newButton.GetComponent<Button>();
        newBtn.gameObject.name = img.sprite.name;
        newBtn.onClick.AddListener(() => OnSelectedCardsClicked(newBtn, cardData));
    }

    public void OnSelectedCardsClicked(Button btn, CardData cardData)
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
                    Debug.Log(cardID);
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

    public CardData GetSelectedCardsCardDataByID(string id)
    {
        return selectedCards.Where(x => x.cardID == id).First();
    }
}
