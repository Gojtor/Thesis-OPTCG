using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
namespace TCGSim
{
    public class GameBoard : MonoBehaviour
    {
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
        #endregion

        private GameObject waitingForOpp;
        private GameObject connecting;
        private string playerName;
        private string enemyName;

        public string gameCustomID { get; private set; } = System.Guid.NewGuid().ToString();

        // Start is called before the first frame update
        async void Start()
        {
            playerName = GameOptions.playerName;
            gameCustomID = GameOptions.gameID;
            Debug.Log(playerName+" "+gameCustomID);
            ServerCon.Instance.Init(gameCustomID, playerName);
            connecting = Instantiate(connectingPrefab, this.gameObject.transform);
            await ServerCon.Instance.ConnectToServer();
            CreateBoards();
            switch (GameManager.Instance.currentState)
            {
                case GameState.CONNECTING:
                    await ServerCon.Instance.AddPlayerToGroupInSocket(gameCustomID,playerName);
                    Debug.Log("Connecting!");
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
            connecting.SetActive(false);
            if (waitingForOpp != null)
            {
                waitingForOpp.SetActive(false);
            }
            GameManager.Instance.ChangeGameState(GameState.STARTINGPHASE);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void CreateBoards()
        {
            GameObject enemyBoardObj = Instantiate(boardPrefab, this.gameObject.transform);
            enemyBoardObj.AddComponent<EnemyBoard>();
            GameObject playerBoardObj = Instantiate(boardPrefab, this.gameObject.transform);
            playerBoardObj.AddComponent<PlayerBoard>();
            EnemyBoard enemyBoard = enemyBoardObj.GetComponent<EnemyBoard>();
            PlayerBoard playerBoard = playerBoardObj.GetComponent<PlayerBoard>();
            playerBoard.InitPrefabs(handPrefab,characterAreaPrefab,costAreaPrefab,stageAreaPrefab,deckPrefab,leaderPrefab,trashPrefab,
                cardPrefab,lifePrefab,keepBtnPrefab,mulliganBtnPrefab,donDeckPrefab,donPrefab);
            enemyBoard.InitPrefabs(handPrefab, characterAreaPrefab, costAreaPrefab, stageAreaPrefab, deckPrefab, leaderPrefab, trashPrefab,
                cardPrefab, lifePrefab, keepBtnPrefab, mulliganBtnPrefab, donDeckPrefab, donPrefab);
            playerBoard.Init("PLAYERBOARD", gameCustomID, playerName);
            enemyBoard.Init("ENEMYBOARD", gameCustomID);
            playerBoard.gameObject.transform.Translate(0, -235, 0);
            enemyBoard.gameObject.transform.Translate(0, 235, 0);
            enemyBoard.gameObject.transform.Rotate(0, 0, 180);
        }
    }
}
