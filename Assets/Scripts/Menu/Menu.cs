using Assets.Scripts.ServerCon;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private GameObject nameInputPrefab;

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

        private GameObject loginRegisterPanel;
        private GameObject defaultPaneObject;
        private GameObject loginPanel;
        private GameObject registerPanel;
        private GameObject loginFailed;
        private GameObject registerFailed;
        private Button createGameBtn;
        private Button connectGameBtn;
        private GameObject createGamePanelObject;
        private GameObject namePanelObject;
        private GameObject gameIDPanelObject;
        private TMP_InputField nameInputObject;
        private TMP_InputField gameIDInputObject;
        private Button startGameBtnObject;
        private Button connectToGameBtnObject;
        private Button quitGameBtn;
        private Button backBtn;
        private Button loginBtnInLoginRegister;
        private Button registerBtnInLoginRegister;
        private Button loginBtnInLogin;
        private Button registerBtnInRegister;
        private Button failedBack;
        private TMP_InputField nameInputInLogin;
        private TMP_InputField passwordInputInLogin;
        private TMP_InputField nameInputInRegister;
        private TMP_InputField passwordInputInRegister;
        private TextMeshProUGUI loggedInAsText;

        public static event Action succesFullyRegistered;
        public static event Action succesFullyLoggedIn;
        public static event Action registerFail;
        public static event Action logInFail;

        private string playerName="Default";
        private string gameID="Default";

        private string userName = "Default";
        private string password = "Default";


        private LineRenderer lineRenderer;
        private Vector2 mousePos;
        private Vector2 startMousePos;

        // Start is called before the first frame update
        void Start()
        {
            LoadLoginRegisterPanel();
            succesFullyLoggedIn += Menu_succesFullyLoggedIn;
            succesFullyRegistered += Menu_succesFullyRegistered;
            registerFail += Menu_registerFail;
            logInFail += Menu_logInFail;
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
            succesFullyLoggedIn -= Menu_succesFullyLoggedIn;
            succesFullyRegistered -= Menu_succesFullyRegistered;
        }

        public void LoadLoginRegisterPanel()
        {
            loginRegisterPanel = Instantiate(loginRegisterPanelPrefab, this.gameObject.transform);
            registerBtnInLoginRegister = loginRegisterPanel.transform.Find("RegisterBtn").gameObject.GetComponent<Button>();
            loginBtnInLoginRegister = loginRegisterPanel.transform.Find("LoginBtn").gameObject.GetComponent<Button>();
            quitGameBtn = loginRegisterPanel.transform.Find("QuitGameBtn").gameObject.GetComponent<Button>();
            quitGameBtn.onClick.AddListener(QuitGame);
            registerBtnInLoginRegister.onClick.AddListener(SetActiveRegisterPanel);
            loginBtnInLoginRegister.onClick.AddListener(SetActiveLoginPanel);
            LoadLoginPanel();
            LoadRegisterPanel();
            loginRegisterPanel.SetActive(false);
        }

        public void LoadDefaultPanel()
        {
            defaultPaneObject = Instantiate(defaultPanelPrefab, this.gameObject.transform);
            createGameBtn = defaultPaneObject.transform.Find("CreateGameBtn").gameObject.GetComponent<Button>();
            connectGameBtn = defaultPaneObject.transform.Find("ConnectGameBtn").gameObject.GetComponent<Button>();
            quitGameBtn = defaultPaneObject.transform.Find("QuitGameBtn").gameObject.GetComponent<Button>();
            createGameBtn.onClick.AddListener(CreateGame);
            connectGameBtn.onClick.AddListener(ConnectToGame);
            quitGameBtn.onClick.AddListener(QuitGame);
            loggedInAsText = defaultPaneObject.transform.Find("LoggedInAs").gameObject.GetComponent<TextMeshProUGUI>();

            createGamePanelObject = Instantiate(createGamePanelPrefab, this.gameObject.transform);
            createGamePanelObject.SetActive(false);
            namePanelObject = createGamePanelObject.transform.Find("NamePanel").gameObject;
            gameIDPanelObject = createGamePanelObject.transform.Find("GameIDPanel").gameObject;
            nameInputObject = namePanelObject.transform.Find("NameInput").GetComponent<TMP_InputField>();
            nameInputObject.onEndEdit.AddListener(UpdatePlayerNameFromInputField);
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
            backBtn = registerPanel.transform.Find("BackBtn").GetComponent<Button>();
            backBtn.onClick.AddListener(BackButtonClickRegisterOrLogin);
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
            backBtn = loginPanel.transform.Find("BackBtn").GetComponent<Button>();
            backBtn.onClick.AddListener(BackButtonClickRegisterOrLogin);
            loginPanel.SetActive(false);
            loginFailed = Instantiate(loginFailedPrefab, this.gameObject.transform);
            loginFailed.SetActive(false);
            failedBack = loginFailed.transform.Find("Back").GetComponent<Button>();
            failedBack.onClick.AddListener(FailedBackButton);
        }

        public async void CreateGame()
        {
            GameOptions.decksJson = await LoginManager.Instance.GetUserDecks(userName);
            defaultPaneObject.SetActive(false);
            createGamePanelObject.SetActive(true);
            startGameBtnObject.gameObject.SetActive(true);
            connectToGameBtnObject.gameObject.SetActive(false);
            backBtn.gameObject.SetActive(true);
        }

        public void ConnectToGame()
        {
            defaultPaneObject.SetActive(false);
            createGamePanelObject.SetActive(true);
            startGameBtnObject.gameObject.SetActive(false);
            connectToGameBtnObject.gameObject.SetActive(true);
            backBtn.gameObject.SetActive(true);
        }

        public void BackButtonClick()
        {
            createGamePanelObject.SetActive(false);
            defaultPaneObject.SetActive(true);
            backBtn.gameObject.SetActive(false);
        }

        public void BackButtonClickRegisterOrLogin()
        {
            loginRegisterPanel.SetActive(true);
            loginPanel.SetActive(false);
            registerPanel.SetActive(false);
            backBtn.gameObject.SetActive(false);
        }

        public void SetActiveRegisterPanel()
        {
            loginRegisterPanel.SetActive(false);
            loginPanel.SetActive(false);
            registerPanel.SetActive(true);
            backBtn.gameObject.SetActive(true);
        }

        public void SetActiveLoginPanel()
        {
            loginRegisterPanel.SetActive(false);
            loginPanel.SetActive(true);
            registerPanel.SetActive(false);
            backBtn.gameObject.SetActive(true);
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
            GameOptions.playerName = playerName;
            GameOptions.gameID = gameID;
            Debug.Log("Start Game");
            GameManager.Instance.ChangeGameState(GameState.WAITINGFOROPPONENT);
            SceneManager.LoadScene("GameBoard");
        }

        public void Connect()
        {
            GameOptions.playerName = playerName;
            GameOptions.gameID = gameID;
            Debug.Log("Connect to game");
            GameManager.Instance.ChangeGameState(GameState.CONNECTING);
            SceneManager.LoadScene("GameBoard");
        }

        public void UpdatePlayerNameFromInputField(string s)
        {
            playerName = s;
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
        private void Menu_succesFullyRegistered()
        {
            registerPanel.SetActive(false);
            backBtn.gameObject.SetActive(false);
            defaultPaneObject.SetActive(true);
            GameOptions.userName = userName;
            loggedInAsText.text = "Logged in as: " + userName;
        }

        private void Menu_succesFullyLoggedIn()
        {
            loginPanel.SetActive(false);
            backBtn.gameObject.SetActive(false);
            defaultPaneObject.SetActive(true);
            GameOptions.userName = userName;
            loggedInAsText.text = "Logged in as: " + userName;
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

    }
}