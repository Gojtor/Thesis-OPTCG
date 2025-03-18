using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
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
        private GameObject namePanelPrefab;

        [SerializeField]
        private GameObject gameIDPanelPrefab;

        [SerializeField]
        private GameObject nameInputPrefab;

        [SerializeField]
        private GameObject gameIDInputPrefab;

        [SerializeField]
        private GameObject startGameBtnPrefab;

        private GameObject defaultPaneObject;
        private Button createGameBtn;
        private GameObject createGamePanelObject;
        private GameObject namePanelObject;
        private GameObject gameIDPanelObject;
        private TMP_InputField nameInputObject;
        private TMP_InputField gameIDInputObject;
        private Button startGameBtnObject;

        private string playerName="Default";
        private string gameID="Default";


        // Start is called before the first frame update
        void Start()
        {
            defaultPaneObject = Instantiate(defaultPanelPrefab, this.gameObject.transform);
            createGameBtn = Instantiate(createGameBtnPrefab, defaultPaneObject.transform).GetComponent<Button>();
            createGameBtn.onClick.AddListener(CreateGame);
        }

        // Update is called once per frame
        void Update()
        {
            Debug.Log(defaultPaneObject.name);
        }

        private void Awake()
        {
        }

        public void CreateGame()
        {
            defaultPaneObject.SetActive(false);
            createGamePanelObject = Instantiate(createGamePanelPrefab, this.gameObject.transform);
            namePanelObject = Instantiate(namePanelPrefab, createGamePanelObject.transform);
            gameIDPanelObject = Instantiate(gameIDPanelPrefab, createGamePanelObject.transform);
            nameInputObject = Instantiate(nameInputPrefab, namePanelObject.transform).GetComponent<TMP_InputField>();
            nameInputObject.onEndEdit.AddListener(UpdatePlayerNameFromInputField);
            gameIDInputObject = Instantiate(gameIDInputPrefab, gameIDPanelObject.transform).GetComponent<TMP_InputField>();
            gameIDInputObject.onEndEdit.AddListener(UpdateGameIDFromInputField);
            startGameBtnObject = Instantiate(startGameBtnPrefab, this.gameObject.transform).GetComponent<Button>();
            startGameBtnObject.onClick.AddListener(StartGame);
        }

        public void StartGame()
        {
            GameOptions.playerName = playerName;
            GameOptions.gameID = gameID;
            Debug.Log("Start Game");
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