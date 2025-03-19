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

        [SerializeField]
        private GameObject enemyBoardPrefab;

        [SerializeField]
        private GameObject serverConPrefab;

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
        #endregion

        private ServerCon serverCon;
        private string playerName;
        private string enemyName;

        public string gameCustomID { get; private set; } = System.Guid.NewGuid().ToString();

        // Start is called before the first frame update
        void Start()
        {
            serverCon = Instantiate(serverConPrefab, this.gameObject.transform).GetComponent<ServerCon>();
            playerName = GameOptions.playerName;
            Debug.Log(playerName);
            CreateBoards();
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
            playerBoard.Init("PLAYERBOARD", serverCon, gameCustomID);
            enemyBoard.Init("ENEMYBOARD",serverCon, gameCustomID);
            playerBoard.gameObject.transform.Translate(0, -235, 0);
            enemyBoard.gameObject.transform.Translate(0, 235, 0);
            enemyBoard.gameObject.transform.Rotate(0, 0, 180);
        }
    }
}
