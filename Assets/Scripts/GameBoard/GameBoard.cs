using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace TCGSim
{
    public class GameBoard : MonoBehaviour
    {
        public static GameBoard Instance { get; private set; }

        [SerializeField]
        private GameObject boardPrefab;

        /// <summary>
        ///  Prefabs for the player board
        /// </summary>
        #region Prefabs
        [SerializeField]
        public GameObject handPrefab;

        [SerializeField]
        public GameObject characterAreaPrefab;

        [SerializeField]
        public GameObject costAreaPrefab;

        [SerializeField]
        public GameObject stageAreaPrefab;

        [SerializeField]
        public GameObject deckPrefab;

        [SerializeField]
        public GameObject leaderPrefab;

        [SerializeField]
        public GameObject trashPrefab;

        [SerializeField]
        public GameObject cardPrefab;

        [SerializeField]
        public GameObject lifePrefab;

        [SerializeField]
        public GameObject keepBtnPrefab;

        [SerializeField]
        public GameObject mulliganBtnPrefab;

        [SerializeField]
        public GameObject donDeckPrefab;

        [SerializeField]
        public GameObject donPrefab;

        [SerializeField]
        public GameObject waitingForOpponentPrefab;

        [SerializeField]
        public GameObject connectingPrefab;

        [SerializeField]
        public GameObject endOfTurnBtnPrefab;

        [SerializeField]
        public GameObject noBlockBtnPrefab;

        [SerializeField]
        public GameObject noMoreCounterBtnPrefab;

        [SerializeField]
        public GameObject cancelBtnPrefab;

        [SerializeField]
        public GameObject chatManagerPrefab;

        [SerializeField]
        public GameObject chatViewPrefab;

        [SerializeField]
        public GameObject gameAlreadyExistPrefab;

        [SerializeField]
        public GameObject playerAlreadyInGamePrefab;

        [SerializeField]
        public GameObject twoPlayerInGamePrefab;

        [SerializeField]
        public GameObject gameDoesntExistPrefab;

        [SerializeField]
        public GameObject matchWonPrefab;

        [SerializeField]
        public GameObject matchLostPrefab;

        [SerializeField]
        public GameObject cardMagnifierPrefab;

        [SerializeField]
        public GameObject menuBtnPrefab;

        [SerializeField]
        public GameObject menuPanelPrefab;

        [SerializeField]
        public GameObject concedePrefab;

        [SerializeField]
        public GameObject backToMainInMenuPrefab;

        [SerializeField]
        public GameObject resumeGamePrefab;

        [SerializeField]
        public GameObject backToMainBtnPrefab;
        #endregion

        private GameObject waitingForOpp;
        private GameObject connecting;
        private Button menuBtn;
        private GameObject menuPanel;
        private Button concedeBtn;
        private Button backToMainBtn;
        private Button resumeBtn;
        private GameState beforeMenuState;
        public Image cardMagnifier { get; private set; }
        public string playerName { get; private set; }
        public string enemyName { get; private set; }

        public string gameCustomID { get; private set; } = System.Guid.NewGuid().ToString();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        // Start is called before the first frame update
        async void Start()
        {
            if (GameManager.Instance.currentState == GameState.TESTING)
            {
                CreateBoardsForTesting();
                ServerCon.Instance.Init(gameCustomID, playerName);
                await ServerCon.Instance.ConnectToServer();
                return;
            }
            playerName = GameOptions.playerName;
            gameCustomID = GameOptions.gameID;
            Debug.Log(playerName + " " + gameCustomID);
            ServerCon.Instance.Init(gameCustomID, playerName);
            connecting = Instantiate(connectingPrefab, this.gameObject.transform);
            await ServerCon.Instance.ConnectToServer();
            GameObject chatView = Instantiate(chatViewPrefab, this.gameObject.transform);
            ChatManager chatManager = Instantiate(chatManagerPrefab, this.gameObject.transform).GetComponent<ChatManager>();
            chatManager.SetChatContent(chatView.transform.GetChild(0).GetChild(0).gameObject.GetComponent<CanvasGroup>());
            CreateMenu();
            switch (GameManager.Instance.currentState)
            {
                case GameState.CONNECTING:
                    await ServerCon.Instance.AddPlayerToGroupInSocket(gameCustomID, playerName);
                    Debug.Log(playerName + "is connecting!");
                    await ServerCon.Instance.WaitForConnection();
                    break;
                case GameState.WAITINGFOROPPONENT:
                    await ServerCon.Instance.CreateGroupInSocket(gameCustomID, playerName);
                    connecting.SetActive(false);
                    waitingForOpp = Instantiate(waitingForOpponentPrefab, this.gameObject.transform);
                    ChatManager.Instance.AddMessage("Game created with the following ID: " + gameCustomID + " . Waiting for an opponent to join!");
                    Debug.Log("Waiting For Opponent!");
                    await ServerCon.Instance.WaitForEnemyToConnect();
                    break;
                default:
                    break;
            }

        }
        public async void BuildUpBoards()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                menuBtn.interactable = false;
                concedeBtn.interactable = false;
                CreateBoards();
                connecting.SetActive(false);
                if (waitingForOpp != null)
                {
                    waitingForOpp.SetActive(false);
                }
                cardMagnifier = Instantiate(cardMagnifierPrefab, this.gameObject.transform).GetComponent<Image>();
                cardMagnifier.gameObject.SetActive(false);
                GameManager.Instance.ChangeGameState(GameState.STARTINGPHASE);
            });
        }

        private void CreateBoards()
        {
            GameObject enemyBoardObj = Instantiate(boardPrefab, this.gameObject.transform);
            enemyBoardObj.AddComponent<EnemyBoard>();
            GameObject playerBoardObj = Instantiate(boardPrefab, this.gameObject.transform);
            playerBoardObj.AddComponent<PlayerBoard>();
            EnemyBoard enemyBoard = enemyBoardObj.GetComponent<EnemyBoard>();
            PlayerBoard playerBoard = playerBoardObj.GetComponent<PlayerBoard>();
            playerBoard.InitPrefabs(handPrefab, characterAreaPrefab, costAreaPrefab, stageAreaPrefab, deckPrefab, leaderPrefab, trashPrefab,
                cardPrefab, lifePrefab, keepBtnPrefab, mulliganBtnPrefab, donDeckPrefab, donPrefab, endOfTurnBtnPrefab, noBlockBtnPrefab, cancelBtnPrefab, noMoreCounterBtnPrefab);
            enemyBoard.InitPrefabs(handPrefab, characterAreaPrefab, costAreaPrefab, stageAreaPrefab, deckPrefab, leaderPrefab, trashPrefab,
                cardPrefab, lifePrefab, keepBtnPrefab, mulliganBtnPrefab, donDeckPrefab, donPrefab, endOfTurnBtnPrefab, noBlockBtnPrefab, cancelBtnPrefab, noMoreCounterBtnPrefab);
            playerBoard.Init("PLAYERBOARD", GameOptions.gameID, playerName);
            enemyBoard.Init("ENEMYBOARD", GameOptions.gameID, enemyName);
            playerBoard.gameObject.transform.Translate(0, -255, 0);
            enemyBoard.gameObject.transform.Translate(0, 255, 0);
            enemyBoard.gameObject.transform.Rotate(0, 0, 180);
        }
        private void CreateBoardsForTesting()
        {
            GameObject chatView = Instantiate(chatViewPrefab, this.gameObject.transform);
            ChatManager chatManager = Instantiate(chatManagerPrefab, this.gameObject.transform).GetComponent<ChatManager>();
            chatManager.SetChatContent(chatView.transform.GetChild(0).GetChild(0).gameObject.GetComponent<CanvasGroup>());
            cardMagnifier = Instantiate(cardMagnifierPrefab, this.gameObject.transform).GetComponent<Image>();
            cardMagnifier.gameObject.SetActive(false);
            GameObject enemyBoardObj = Instantiate(boardPrefab, this.gameObject.transform);
            enemyBoardObj.AddComponent<EnemyBoard>();
            GameObject playerBoardObj = Instantiate(boardPrefab, this.gameObject.transform);
            playerBoardObj.AddComponent<PlayerBoard>();
            EnemyBoard enemyBoard = enemyBoardObj.GetComponent<EnemyBoard>();
            enemyBoard.gameObject.name = "EnemyBoard";
            PlayerBoard playerBoard = playerBoardObj.GetComponent<PlayerBoard>();
            playerBoard.gameObject.name = "PlayerBoard";
            playerBoard.InitPrefabs(handPrefab, characterAreaPrefab, costAreaPrefab, stageAreaPrefab, deckPrefab, leaderPrefab, trashPrefab,
                cardPrefab, lifePrefab, keepBtnPrefab, mulliganBtnPrefab, donDeckPrefab, donPrefab, endOfTurnBtnPrefab, noBlockBtnPrefab, cancelBtnPrefab, noMoreCounterBtnPrefab);
            enemyBoard.InitPrefabs(handPrefab, characterAreaPrefab, costAreaPrefab, stageAreaPrefab, deckPrefab, leaderPrefab, trashPrefab,
                cardPrefab, lifePrefab, keepBtnPrefab, mulliganBtnPrefab, donDeckPrefab, donPrefab, endOfTurnBtnPrefab, noBlockBtnPrefab, cancelBtnPrefab, noMoreCounterBtnPrefab);
            playerBoard.gameObject.transform.Translate(0, -255, 0);
            enemyBoard.gameObject.transform.Translate(0, 255, 0);
            enemyBoard.gameObject.transform.Rotate(0, 0, 180);
        }

        private async void CreateMenu()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                menuBtn = Instantiate(menuBtnPrefab, this.gameObject.transform).GetComponent<Button>();
                menuBtn.onClick.AddListener(MenuBtnClick);
                menuBtn.gameObject.AddComponent<GraphicRaycaster>();
                Canvas menuBtnCanvas = menuBtn.GetComponent<Canvas>();
                menuBtnCanvas.overrideSorting = true;
                menuBtnCanvas.sortingOrder = 6;
                menuPanel = Instantiate(menuPanelPrefab, this.gameObject.transform);
                Canvas menuPanelCanvas = menuPanel.GetComponent<Canvas>();
                menuPanelCanvas.overrideSorting = true;
                menuPanelCanvas.sortingOrder = 6;
                concedeBtn = Instantiate(concedePrefab, menuPanel.gameObject.transform).GetComponent<Button>();
                backToMainBtn = Instantiate(backToMainInMenuPrefab, menuPanel.gameObject.transform).GetComponent<Button>();
                resumeBtn = Instantiate(resumeGamePrefab, menuPanel.gameObject.transform).GetComponent<Button>();
                concedeBtn.onClick.AddListener(Concede);
                backToMainBtn.onClick.AddListener(BackToMainMenu);
                resumeBtn.onClick.AddListener(ResumeGame);
                menuPanel.SetActive(false);
                concedeBtn.interactable = false;
            });
        }
        public void GameWithIDAlreadyExist()
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                GameObject alreadyExist = Instantiate(gameAlreadyExistPrefab, this.gameObject.transform);
                Button backToMainMenu = alreadyExist.transform.GetChild(0).GetComponent<Button>();
                backToMainMenu.onClick.AddListener(BackToMainMenu);
            });
        }
        public void PlayerWithThisNameAlreadyIsInTheGame()
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                GameObject alreadyExist = Instantiate(playerAlreadyInGamePrefab, this.gameObject.transform);
                Button backToMainMenu = alreadyExist.transform.GetChild(0).GetComponent<Button>();
                backToMainMenu.onClick.AddListener(BackToMainMenu);
            });
        }
        public void TwoPlayerAlreadyInGame()
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                GameObject alreadyInGame = Instantiate(twoPlayerInGamePrefab, this.gameObject.transform);
                Button backToMainMenu = alreadyInGame.transform.GetChild(0).GetComponent<Button>();
                backToMainMenu.onClick.AddListener(BackToMainMenu);
            });
        }
        public void GameWithIDDoesntExist()
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                GameObject doesntExist = Instantiate(gameDoesntExistPrefab, this.gameObject.transform);
                Button backToMainMenu = doesntExist.transform.GetChild(0).GetComponent<Button>();
                backToMainMenu.onClick.AddListener(BackToMainMenu);
            });
        }

        public async void GameWon()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                GameObject gameWonPanel = Instantiate(matchWonPrefab, this.gameObject.transform);
                Button backToMainMenu = Instantiate(backToMainBtnPrefab, gameWonPanel.transform).GetComponent<Button>();
                Canvas gameWonPanelCanvas = gameWonPanel.GetComponent<Canvas>();
                gameWonPanelCanvas.overrideSorting = true;
                gameWonPanelCanvas.sortingOrder = 6;
                backToMainMenu.onClick.AddListener(BackToMainMenu);
            });
        }

        public async void GameLost()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(() =>
            {
                GameObject gameLostPanel = Instantiate(matchLostPrefab, this.gameObject.transform);
                Button backToMainMenu = Instantiate(backToMainBtnPrefab, gameLostPanel.transform).GetComponent<Button>();
                Canvas gameLostPanelCanvas = gameLostPanel.GetComponent<Canvas>();
                gameLostPanelCanvas.overrideSorting = true;
                gameLostPanelCanvas.sortingOrder = 6;
                backToMainMenu.onClick.AddListener(BackToMainMenu);
            });
        }

        public async void BackToMainMenu()
        {
            await UnityMainThreadDispatcher.RunOnMainThread(async () =>
            {
                await ServerCon.Instance.EnemyWon();
                SceneManager.LoadScene("Menu");
            });
        }
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void SetPlayerName(string name)
        {
            this.playerName = name;
        }

        public void SetEnemyName(string name)
        {
            this.enemyName = name;
        }

        public void SetCardMagnifierActiveWithImage(Sprite sprite)
        {
            Image magnifiedImg = cardMagnifier.transform.GetChild(0).gameObject.GetComponent<Image>();
            cardMagnifier.gameObject.SetActive(true);
            magnifiedImg.sprite = sprite;
        }

        public void HideCardMagnifier()
        {
            cardMagnifier.gameObject.SetActive(false);
        }

        public void MenuBtnClick()
        {
            menuPanel.SetActive(true);
            menuPanel.transform.SetAsLastSibling();
            menuBtn.gameObject.SetActive(false);
            beforeMenuState = GameManager.Instance.currentState;
        }

        public async void Concede()
        {
            await ServerCon.Instance.EnemyWon();
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                menuPanel.SetActive(false);
                this.GameLost();
            });
        }

        public void ResumeGame()
        {
            menuPanel.SetActive(false);
            menuBtn.gameObject.SetActive(true);
        }

        public void SetMenuActive(bool active)
        {
            menuBtn.interactable = active;
        }

        public void SetConcedeActive(bool active)
        {
            concedeBtn.interactable = active;
        }
    }
}
