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


        private LineRenderer lineRenderer;
        private Vector2 mousePos;
        private Vector2 startMousePos;

        // Start is called before the first frame update
        void Start()
        {
            defaultPaneObject = Instantiate(defaultPanelPrefab, this.gameObject.transform);
            createGameBtn = Instantiate(createGameBtnPrefab, defaultPaneObject.transform).GetComponent<Button>();
            createGameBtn.onClick.AddListener(CreateGame);

            lineRenderer = this.gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Basic material
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