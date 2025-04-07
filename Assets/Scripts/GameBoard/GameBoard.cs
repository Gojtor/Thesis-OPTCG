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
        #endregion

        private GameObject waitingForOpp;
        private GameObject connecting;
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
            playerName = GameOptions.playerName;
            gameCustomID = GameOptions.gameID;
            Debug.Log(playerName + " " + gameCustomID);
            ServerCon.Instance.Init(gameCustomID, playerName);
            connecting = Instantiate(connectingPrefab, this.gameObject.transform);
            await ServerCon.Instance.ConnectToServer();
            GameObject chatView = Instantiate(chatViewPrefab, this.gameObject.transform);
            ChatManager chatManager = Instantiate(chatManagerPrefab, this.gameObject.transform).GetComponent<ChatManager>();
            chatManager.SetChatContent(chatView.transform.GetChild(0).GetChild(0).gameObject.GetComponent<CanvasGroup>());
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
                    Debug.Log("Waiting For Opponent!");
                    await ServerCon.Instance.WaitForEnemyToConnect();
                    break;
                default:
                    break;
            }

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void BuildUpBoards()
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                CreateBoards();
                connecting.SetActive(false);
                if (waitingForOpp != null)
                {
                    waitingForOpp.SetActive(false);
                }
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
                cardPrefab, lifePrefab, keepBtnPrefab, mulliganBtnPrefab, donDeckPrefab, donPrefab, endOfTurnBtnPrefab, noBlockBtnPrefab, noMoreCounterBtnPrefab);
            enemyBoard.InitPrefabs(handPrefab, characterAreaPrefab, costAreaPrefab, stageAreaPrefab, deckPrefab, leaderPrefab, trashPrefab,
                cardPrefab, lifePrefab, keepBtnPrefab, mulliganBtnPrefab, donDeckPrefab, donPrefab, endOfTurnBtnPrefab, noBlockBtnPrefab, noMoreCounterBtnPrefab);
            playerBoard.Init("PLAYERBOARD", gameCustomID, playerName);
            enemyBoard.Init("ENEMYBOARD", gameCustomID,enemyName);
            playerBoard.gameObject.transform.Translate(0, -255, 0);
            enemyBoard.gameObject.transform.Translate(0, 255, 0);
            enemyBoard.gameObject.transform.Rotate(0, 0, 180);
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
                GameObject alreadyExist = Instantiate(twoPlayerInGamePrefab, this.gameObject.transform);
                Button backToMainMenu = alreadyExist.transform.GetChild(0).GetComponent<Button>();
                backToMainMenu.onClick.AddListener(BackToMainMenu);
            });
        }
        public void GameWithIDDoesntExist()
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                GameObject alreadyExist = Instantiate(gameDoesntExistPrefab, this.gameObject.transform);
                Button backToMainMenu = alreadyExist.transform.GetChild(0).GetComponent<Button>();
                backToMainMenu.onClick.AddListener(BackToMainMenu);
            });
        }

        public void GameWon()
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                GameObject alreadyExist = Instantiate(matchWonPrefab, this.gameObject.transform);
                Button backToMainMenu = alreadyExist.transform.GetChild(0).GetComponent<Button>();
                backToMainMenu.onClick.AddListener(BackToMainMenu);
            });
        }

        public void GameLost()
        {
            UnityMainThreadDispatcher.Enqueue(() =>
            {
                GameObject alreadyExist = Instantiate(matchLostPrefab, this.gameObject.transform);
                Button backToMainMenu = alreadyExist.transform.GetChild(0).GetComponent<Button>();
                backToMainMenu.onClick.AddListener(BackToMainMenu);
            });
        }

        public void BackToMainMenu()
        {
            SceneManager.LoadScene("Menu");
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
    }
}
