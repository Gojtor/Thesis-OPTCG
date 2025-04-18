using Assets.Scripts.ServerCon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TCGSim
{
    public class Menu : MonoBehaviour
    {
        [SerializeField]
        private GameObject loginRegisterPanelPrefab;

        [SerializeField]
        private GameObject defaultPanelPrefab;

        [SerializeField]
        private GameObject createGamePanelPrefab;

        [SerializeField]
        private GameObject createGameBtnPrefab;

        [SerializeField]
        private GameObject connectGameBtnPrefab;

        [SerializeField]
        private GameObject namePanelPrefab;

        [SerializeField]
        private GameObject gameIDPanelPrefab;

        [SerializeField]
        private GameObject gameIDInputPrefab;

        [SerializeField]
        private GameObject startGameBtnPrefab;

        [SerializeField]
        private GameObject connectToGameBtnPrefab;

        [SerializeField]
        private GameObject quitGameBtnPrefab;

        [SerializeField]
        private GameObject backBtnPrefab;

        [SerializeField]
        private GameObject loginPanelPrefab;

        [SerializeField]
        private GameObject registerPanelPrefab;

        [SerializeField]
        private GameObject loginFailedPrefab;

        [SerializeField]
        private GameObject registerFailedPrefab;

        [SerializeField]
        private GameObject deckBuilderPrefab;

        [SerializeField]
        private GameObject deckBuilderBtnPrefab;

        [SerializeField]
        private GameObject playerNameTextPrefab;

        [SerializeField]
        private GameObject friendsBtnPrefab;

        [SerializeField]
        private GameObject friendsPanelPrefab;

        [SerializeField]
        private GameObject friendTxtPrefab;

        [SerializeField]
        private GameObject requestPrefab;

        private GameObject loginRegisterPanel;
        private GameObject defaultPaneObject;
        private GameObject loginPanel;
        private GameObject registerPanel;
        private GameObject loginFailed;
        private GameObject registerFailed;
        private GameObject createGamePanelObject;
        private GameObject namePanelObject;
        private GameObject gameIDPanelObject;
        private GameObject friendsPanelObject;
        private GameObject requestPanel;
        private GameObject friendListContent;
        private GameObject requestListContent;
        private Button startGameBtnObject;
        private Button connectToGameBtnObject;
        private Button quitGameBtn;
        private Button backBtn;
        private Button backBtnInLoginRegister;
        private Button loginBtnInLoginRegister;
        private Button loginGuestBtn;
        private Button registerBtnInLoginRegister;
        private Button loginBtnInLogin;
        private Button registerBtnInRegister;
        private Button failedBack;
        private Button deckBuilderBtn;
        private Button createGameBtn;
        private Button connectGameBtn;
        private Button friendsBtn;
        private Button sendFriendReqBtn;
        private TMP_InputField gameIDInputObject;
        private TMP_InputField nameInputInLogin;
        private TMP_InputField passwordInputInLogin;
        private TMP_InputField nameInputInRegister;
        private TMP_InputField passwordInputInRegister;
        private TMP_InputField sendFriendReqToInput;
        private TextMeshProUGUI loggedInAsText;
        private TextMeshProUGUI playerNameText;
        private TextMeshProUGUI friendText;

        private DeckBuilder deckBuilder;
        private TMP_Dropdown deckSelectorDropDown;

        public static event Action succesFullyRegistered;
        public static event Action succesFullyLoggedIn;
        public static event Action registerFail;
        public static event Action logInFail;
        public static event Action backToMenuFromDeckBuilder;
        public static event Action couldnSendFriendRequest;

        private string gameID="Default";

        private string userName = "Default";
        private string password = "Default";

        private string sendRequestName;


        private LineRenderer lineRenderer;
        private Vector2 mousePos;
        private Vector2 startMousePos;

        // Start is called before the first frame update
        void Start()
        {
            LoadLoginRegisterPanel();
            succesFullyLoggedIn += Menu_succesFullyLoggedInWrapper;
            succesFullyRegistered += Menu_succesFullyRegisteredWrapper;
            registerFail += Menu_registerFail;
            logInFail += Menu_logInFail;
            backToMenuFromDeckBuilder += Menu_backToMenuFromDeckBuilder;
            couldnSendFriendRequest += Menu_couldnSendFriendRequest;
            LoadDefaultPanel();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            if (Input.GetMouseButton(0) && lineRenderer!=null)
            {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                lineRenderer.SetPosition(0, new Vector3(startMousePos.x, startMousePos.y, 1f));
                lineRenderer.SetPosition(1, new Vector3(mousePos.x, mousePos.y, 1f));
            }
        }

        private void OnDestroy()
        {
            succesFullyLoggedIn -= Menu_succesFullyLoggedInWrapper;
            succesFullyRegistered -= Menu_succesFullyRegisteredWrapper;
            registerFail -= Menu_registerFail;
            logInFail -= Menu_logInFail;
            backToMenuFromDeckBuilder -= Menu_backToMenuFromDeckBuilder;
            couldnSendFriendRequest -= Menu_couldnSendFriendRequest;
        }

        public void LoadLoginRegisterPanel()
        {
            loginRegisterPanel = Instantiate(loginRegisterPanelPrefab, this.gameObject.transform);
            registerBtnInLoginRegister = loginRegisterPanel.transform.Find("RegisterBtn").gameObject.GetComponent<Button>();
            loginBtnInLoginRegister = loginRegisterPanel.transform.Find("LoginBtn").gameObject.GetComponent<Button>();
            loginGuestBtn = loginRegisterPanel.transform.Find("LoginGuestBtn").gameObject.GetComponent<Button>();
            quitGameBtn = loginRegisterPanel.transform.Find("QuitGameBtn").gameObject.GetComponent<Button>();
            quitGameBtn.onClick.AddListener(QuitGame);
            registerBtnInLoginRegister.onClick.AddListener(SetActiveRegisterPanel);
            loginBtnInLoginRegister.onClick.AddListener(SetActiveLoginPanel);
            loginGuestBtn.onClick.AddListener(LoginAsGuest);
            LoadLoginPanel();
            LoadRegisterPanel();
        }

        public void LoadDefaultPanel()
        {
            defaultPaneObject = Instantiate(defaultPanelPrefab, this.gameObject.transform);
            createGameBtn = defaultPaneObject.transform.Find("CreateGameBtn").gameObject.GetComponent<Button>();
            connectGameBtn = defaultPaneObject.transform.Find("ConnectGameBtn").gameObject.GetComponent<Button>();
            quitGameBtn = defaultPaneObject.transform.Find("QuitGameBtn").gameObject.GetComponent<Button>();
            deckBuilderBtn = Instantiate(deckBuilderBtnPrefab, defaultPaneObject.gameObject.transform).GetComponent<Button>();
            createGameBtn.onClick.AddListener(CreateGame);
            connectGameBtn.onClick.AddListener(ConnectToGame);
            quitGameBtn.onClick.AddListener(QuitGame);
            deckBuilderBtn.onClick.AddListener(OpenDeckBuilder);
            loggedInAsText = defaultPaneObject.transform.Find("LoggedInAs").gameObject.GetComponent<TextMeshProUGUI>();

            createGamePanelObject = Instantiate(createGamePanelPrefab, this.gameObject.transform);
            createGamePanelObject.SetActive(false);

            deckSelectorDropDown = createGamePanelObject.transform.Find("DeckSelector").GetComponent<TMP_Dropdown>();

            namePanelObject = createGamePanelObject.transform.Find("NamePanel").gameObject;

            playerNameText = Instantiate(playerNameTextPrefab, namePanelObject.gameObject.transform).GetComponent<TextMeshProUGUI>();
            playerNameText.text = GameOptions.userName;

            gameIDPanelObject = createGamePanelObject.transform.Find("GameIDPanel").gameObject;
            gameIDInputObject = gameIDPanelObject.transform.Find("GameIDInput").GetComponent<TMP_InputField>();
            gameIDInputObject.onEndEdit.AddListener(UpdateGameIDFromInputField);

            startGameBtnObject = createGamePanelObject.transform.Find("StartGameBtn").GetComponent<Button>();
            startGameBtnObject.onClick.AddListener(StartGame);
            backBtn = this.gameObject.transform.Find("BackBtn").GetComponent<Button>();
            backBtn.gameObject.SetActive(false);
            backBtn.transform.SetAsLastSibling();
            backBtn.onClick.AddListener(BackButtonClick);
            connectToGameBtnObject = createGamePanelObject.transform.Find("ConnectToGameBtn").GetComponent<Button>();
            connectToGameBtnObject.onClick.AddListener(Connect);

            lineRenderer = this.gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingLayerName = "Default";
            lineRenderer.sortingOrder = 100;

            friendsBtn = Instantiate(friendsBtnPrefab, defaultPaneObject.transform).GetComponent<Button>();
            friendsBtn.onClick.AddListener(FriendsButtonClickedWrapper);

            friendsPanelObject = Instantiate(friendsPanelPrefab, this.gameObject.transform);
            friendsPanelObject.gameObject.SetActive(false);
            GameObject friendListPanel = friendsPanelObject.transform.Find("FriendListPanel").gameObject;
            GameObject friendListInsidePanel = friendListPanel.transform.Find("FriendList").gameObject;
            GameObject friendListViewport = friendListInsidePanel.transform.Find("Viewport").gameObject;
            friendListContent = friendListViewport.transform.Find("Content").gameObject;

            GameObject reqListPanel = friendsPanelObject.transform.Find("FriendRequestsPanel").gameObject;
            GameObject friendReq = reqListPanel.transform.Find("FriendRequest").gameObject;
            GameObject reqListViewport = friendReq.transform.Find("Viewport").gameObject;
            requestListContent = reqListViewport.transform.Find("Content").gameObject;

            GameObject sendReqPanel = friendsPanelObject.transform.Find("SendRequestPanel").gameObject;
            sendFriendReqToInput = sendReqPanel.transform.Find("SendNameInput").gameObject.GetComponent<TMP_InputField>();
            sendFriendReqBtn = sendReqPanel.transform.Find("Send").gameObject.GetComponent<Button>();
            sendFriendReqToInput.onValueChanged.AddListener(OnSendRequestInputChange);
            sendFriendReqBtn.onClick.AddListener(SendFriendRequestWrapper);

            defaultPaneObject.SetActive(false);
        }
        public void LoadRegisterPanel()
        {
            registerPanel = Instantiate(registerPanelPrefab, this.gameObject.transform);
            nameInputInRegister = registerPanel.transform.Find("NamePanel").gameObject.transform.Find("NameInput").GetComponent<TMP_InputField>();
            nameInputInRegister.onEndEdit.AddListener(UpdateUsername);
            passwordInputInRegister = registerPanel.transform.Find("PasswordPanel").gameObject.transform.Find("PasswordInput").GetComponent<TMP_InputField>();
            passwordInputInRegister.onEndEdit.AddListener(UpdatePassword);
            registerBtnInRegister = registerPanel.transform.Find("RegisterBtn").GetComponent<Button>();
            registerBtnInRegister.onClick.AddListener(Register);
            backBtnInLoginRegister = registerPanel.transform.Find("BackBtn").GetComponent<Button>();
            backBtnInLoginRegister.onClick.AddListener(BackButtonClickRegisterOrLogin);
            registerPanel.SetActive(false);
            registerFailed = Instantiate(registerFailedPrefab, this.gameObject.transform);
            registerFailed.SetActive(false);
            failedBack = registerFailed.transform.Find("Back").GetComponent<Button>();
            failedBack.onClick.AddListener(FailedBackButton);
        }
        public void LoadLoginPanel()
        {
            loginPanel = Instantiate(loginPanelPrefab, this.gameObject.transform);
            nameInputInLogin = loginPanel.transform.Find("NamePanel").gameObject.transform.Find("NameInput").GetComponent<TMP_InputField>();
            nameInputInLogin.onEndEdit.AddListener(UpdateUsername);
            passwordInputInLogin = loginPanel.transform.Find("PasswordPanel").gameObject.transform.Find("PasswordInput").GetComponent<TMP_InputField>();
            passwordInputInLogin.onEndEdit.AddListener(UpdatePassword);
            loginBtnInLogin = loginPanel.transform.Find("LoginBtn").GetComponent<Button>();
            loginBtnInLogin.onClick.AddListener(Login);
            backBtnInLoginRegister = loginPanel.transform.Find("BackBtn").GetComponent<Button>();
            backBtnInLoginRegister.onClick.AddListener(BackButtonClickRegisterOrLogin);
            loginPanel.SetActive(false);
            loginFailed = Instantiate(loginFailedPrefab, this.gameObject.transform);
            loginFailed.SetActive(false);
            failedBack = loginFailed.transform.Find("Back").GetComponent<Button>();
            failedBack.onClick.AddListener(FailedBackButton);
        }

        public void CreateGame()
        {
            defaultPaneObject.SetActive(false);
            createGamePanelObject.SetActive(true);
            startGameBtnObject.gameObject.SetActive(true);
            connectToGameBtnObject.gameObject.SetActive(false);
            backBtn.gameObject.SetActive(true);
            UpdateDeckSelector();
        }

        public void ConnectToGame()
        {
            defaultPaneObject.SetActive(false);
            createGamePanelObject.SetActive(true);
            startGameBtnObject.gameObject.SetActive(false);
            connectToGameBtnObject.gameObject.SetActive(true);
            backBtn.gameObject.SetActive(true);
            UpdateDeckSelector();
        }

        public void BackButtonClick()
        {
            createGamePanelObject.SetActive(false);
            defaultPaneObject.SetActive(true);
            backBtn.gameObject.SetActive(false);
        }

        public void OpenDeckBuilder()
        {
            defaultPaneObject.SetActive(false);
            deckBuilder.gameObject.SetActive(true);
            DeckBuilder.InvokeSetActive();
        }

        public void BackButtonClickRegisterOrLogin()
        {
            loginRegisterPanel.SetActive(true);
            loginPanel.SetActive(false);
            registerPanel.SetActive(false);
            backBtnInLoginRegister.gameObject.SetActive(false);
        }

        public void SetActiveRegisterPanel()
        {
            loginRegisterPanel.SetActive(false);
            loginPanel.SetActive(false);
            registerPanel.SetActive(true);
            backBtnInLoginRegister.gameObject.SetActive(true);
        }

        public void SetActiveLoginPanel()
        {
            loginRegisterPanel.SetActive(false);
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
            backBtnInLoginRegister.gameObject.SetActive(true);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        public void Login()
        {
            Debug.Log("Login");
            LoginManager.Instance.LoginUser(userName, password);
        }

        public void Register()
        {
            Debug.Log("Register");
            LoginManager.Instance.RegisterUser(userName, password);
        }

        public void StartGame()
        {
            GameOptions.playerName = userName;
            GameOptions.gameID = gameID;
            int selectedIndex = deckSelectorDropDown.value;
            string currentlySelectedDeck = deckSelectorDropDown.options[selectedIndex].text;
            GameOptions.selectedDeckForGame = GameOptions.decksJson.FirstOrDefault(x => x.Split(',')[0] == currentlySelectedDeck);
            Debug.Log("Start Game");
            GameManager.Instance.ChangeGameState(GameState.WAITINGFOROPPONENT);
            SceneManager.LoadScene("GameBoard");
        }

        public void Connect()
        {
            GameOptions.playerName = userName;
            GameOptions.gameID = gameID;
            int selectedIndex = deckSelectorDropDown.value;
            string currentlySelectedDeck = deckSelectorDropDown.options[selectedIndex].text;
            GameOptions.selectedDeckForGame = GameOptions.decksJson.FirstOrDefault(x => x.Split(',')[0] == currentlySelectedDeck);
            Debug.Log("Connect to game");
            GameManager.Instance.ChangeGameState(GameState.CONNECTING);
            SceneManager.LoadScene("GameBoard");
        }
        public void UpdateGameIDFromInputField(string s)
        {
            gameID = s;
        }

        public void UpdatePassword(string s)
        {
            password = s;
        }
        public void UpdateUsername(string s)
        {
            userName = s;
        }

        public void FailedBackButton()
        {
            loginFailed.SetActive(false);
            registerFailed.SetActive(false);
        }
        private void Menu_backToMenuFromDeckBuilder()
        {
            defaultPaneObject.gameObject.SetActive(true);
        }

        public async void Menu_succesFullyRegisteredWrapper()
        {
            await Menu_succesFullyRegistered();
        }
        public async Task Menu_succesFullyRegistered()
        {
            registerPanel.SetActive(false);
            backBtn.gameObject.SetActive(false);
            defaultPaneObject.SetActive(true);
            GameOptions.userName = userName;
            GameOptions.decksJson = await LoginManager.Instance.GetUserDecks(userName);
            playerNameText.text = GameOptions.userName;
            loggedInAsText.text = "Logged in as: " + userName;
            deckBuilder = Instantiate(deckBuilderPrefab, this.gameObject.transform).GetComponent<DeckBuilder>();
            deckBuilder.gameObject.SetActive(false);
        }

        public async void Menu_succesFullyLoggedInWrapper()
        {
            await Menu_succesFullyLoggedIn();
        }

        public async Task Menu_succesFullyLoggedIn()
        {
            loginPanel.SetActive(false);
            backBtn.gameObject.SetActive(false);
            defaultPaneObject.SetActive(true);
            GameOptions.userName = userName;
            GameOptions.decksJson = await LoginManager.Instance.GetUserDecks(userName);
            playerNameText.text = GameOptions.userName;
            loggedInAsText.text = "Logged in as: " + userName;
            deckBuilder = Instantiate(deckBuilderPrefab, this.gameObject.transform).GetComponent<DeckBuilder>();
            deckBuilder.gameObject.SetActive(false);
        }


        private void Menu_couldnSendFriendRequest()
        {
            Debug.Log("Couldn't send friend request!");
        }

        private void Menu_logInFail()
        {
            loginFailed.SetActive(true);
        }

        private void Menu_registerFail()
        {
            registerFailed.SetActive(true);
        }

        public static void InvokeLoginSucces()
        {
            succesFullyLoggedIn?.Invoke();
        }

        public static void InvokeRegisterSuccess()
        {
            succesFullyRegistered?.Invoke();
        }

        public static void InvokeLoginFail()
        {
            logInFail?.Invoke();
        }

        public static void InvokeRegisterFail()
        {
            registerFail?.Invoke();
        }

        public static void InvokeBackToMenuFromDeckBuilder()
        {
            backToMenuFromDeckBuilder?.Invoke();
        }

        public static void InvokeCouldntSendFriendRequest()
        {
            couldnSendFriendRequest?.Invoke();
        }

        private void OnSendRequestInputChange(string s)
        {
            sendRequestName = s;
        }

        public void UpdateDeckSelector()
        {
            List<string> deckNames = new List<string>();
            foreach (string deck in GameOptions.decksJson)
            {
                deckNames.Add(deck.Split(',')[0]);
            }
            deckSelectorDropDown.ClearOptions();
            deckSelectorDropDown.AddOptions(deckNames);

            int defaultIndex = deckSelectorDropDown.options.FindIndex(x => x.text == "ST01-DefaultDeck");
            if (defaultIndex != -1)
            {
                deckSelectorDropDown.value = defaultIndex;
                deckSelectorDropDown.RefreshShownValue();
            }
        }
        private void LoginAsGuest()
        {
            loginRegisterPanel.SetActive(false);
            backBtn.gameObject.SetActive(false);
            defaultPaneObject.SetActive(true);
            friendsBtn.gameObject.SetActive(false);
            System.Random random = new System.Random();
            GameOptions.userName = "Guest"+random.Next(10000, 100000);
            GameOptions.decksJson.Add("ST01-DefaultDeck,1xST01-001,4xST01-002,4xST01-003,4xST01-004,4xST01-005,4xST01-006,4xST01-007,4xST01-008,4xST01-009,4xST01-010,2xST01-011,2xST01-012,2xST01-013,2xST01-014,2xST01-015,2xST01-016,2xST01-017");
            playerNameText.text = GameOptions.userName;
            loggedInAsText.text = "Logged in as: " + GameOptions.userName;
            deckBuilder = Instantiate(deckBuilderPrefab, this.gameObject.transform).GetComponent<DeckBuilder>();
            deckBuilder.gameObject.SetActive(false);
        }

        public async void FriendsButtonClickedWrapper()
        {
            await FriendsButtonClicked();
        }

        public async Task FriendsButtonClicked()
        {
            if (friendsPanelObject.activeInHierarchy)
            {
                friendsPanelObject.SetActive(false);
            }
            else
            {
                friendsPanelObject.SetActive(true);
                nameInputInLogin.text = string.Empty;
                foreach(Transform child in friendListContent.transform)
                {
                    Destroy(child.gameObject);
                }
                List<string> friends = await LoginManager.Instance.GetFriends(GameOptions.userName);
                
                foreach(string friend in friends)
                {
                    friendText = Instantiate(friendTxtPrefab, friendListContent.transform).GetComponent<TextMeshProUGUI>();
                    friendText.text = friend;
                    friendText.gameObject.name = friend;
                }

                List<string> requests = await LoginManager.Instance.GetFriendRequest(GameOptions.userName);

                foreach (Transform child in requestListContent.transform)
                {
                    Destroy(child.gameObject);
                }

                foreach (string req in requests)
                {
                    if (req != GameOptions.userName)
                    {
                        GameObject requestWindow = Instantiate(requestPrefab, requestListContent.transform);
                        TextMeshProUGUI reqNameText = requestWindow.transform.Find("ReqName").GetComponent<TextMeshProUGUI>();
                        reqNameText.text = req;
                        Button accept = requestWindow.transform.Find("Accept").GetComponent<Button>();
                        Button decline = requestWindow.transform.Find("Decline").GetComponent<Button>();
                        accept.onClick.AddListener(() => AcceptFriendRequestWrapper(req, requestWindow));
                        decline.onClick.AddListener(() => DeclineFriendRequestWrapper(req, requestWindow));
                    }
                }
            }
        }

        public async void AcceptFriendRequestWrapper(string fromUser, GameObject reqWindow)
        {
            await AcceptFriendRequest(fromUser,reqWindow);
        }
        public async void DeclineFriendRequestWrapper(string fromUser, GameObject reqWindow)
        {
            await DeclineFriendRequest(fromUser, reqWindow);
        }
        public async void SendFriendRequestWrapper()
        {
            await SendFriendRequest();
        }
        public async Task AcceptFriendRequest(string fromUser, GameObject reqWindow)
        {
            if (GameOptions.userName != null && fromUser != null)
            {
                Destroy(reqWindow);
                await LoginManager.Instance.AcceptFriendRequest(fromUser,GameOptions.userName);
            }
        }

        public async Task DeclineFriendRequest(string fromUser, GameObject reqWindow)
        {
            if (GameOptions.userName != null && fromUser != null)
            {
                Destroy(reqWindow);
                await LoginManager.Instance.DeclineFriendRequest(fromUser, GameOptions.userName);
            }
        }

        public async Task SendFriendRequest()
        {
            if (sendRequestName != null)
            {
                await LoginManager.Instance.SendFriendRequest(GameOptions.userName,sendRequestName);
            }
        }
    }
}