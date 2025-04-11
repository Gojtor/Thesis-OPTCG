using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TCGSim
{
    public class Menu : MonoBehaviour
    {
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

        private GameObject defaultPaneObject;
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

        private string playerName="Default";
        private string gameID="Default";


        private LineRenderer lineRenderer;
        private Vector2 mousePos;
        private Vector2 startMousePos;

        // Start is called before the first frame update
        void Start()
        {
            defaultPaneObject = Instantiate(defaultPanelPrefab, this.gameObject.transform);
            createGameBtn = defaultPaneObject.transform.Find("CreateGameBtn").gameObject.GetComponent<Button>();
            connectGameBtn = defaultPaneObject.transform.Find("ConnectGameBtn").gameObject.GetComponent<Button>();
            quitGameBtn = defaultPaneObject.transform.Find("QuitGameBtn").gameObject.GetComponent<Button>();
            createGameBtn.onClick.AddListener(CreateGame);
            connectGameBtn.onClick.AddListener(ConnectToGame);
            quitGameBtn.onClick.AddListener(QuitGame);

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
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            if (Input.GetMouseButton(0))
            {
                mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                lineRenderer.SetPosition(0, new Vector3(startMousePos.x, startMousePos.y, 1f));
                lineRenderer.SetPosition(1, new Vector3(mousePos.x, mousePos.y, 1f));
            }
        }

        private void Awake()
        {
        }

        public void CreateGame()
        {
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

        public void QuitGame()
        {
            Application.Quit();
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
    }
}