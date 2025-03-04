using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace TCGSim
{
    public class GameBoard : MonoBehaviour
    {
        [SerializeField]
        private GameObject playerBoardPrefab;

        [SerializeField]
        private GameObject enemyBoardPrefab;

        [SerializeField]
        private GameObject serverConPrefab;

        private ServerCon serverCon;

        public string gameCustomID { get; private set; } = System.Guid.NewGuid().ToString();

        // Start is called before the first frame update
        void Start()
        {
            serverCon = Instantiate(serverConPrefab, this.gameObject.transform).GetComponent<ServerCon>();
            CreateBoards();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void CreateBoards()
        {
            
            PlayerBoard enemyBoard = Instantiate(enemyBoardPrefab, this.gameObject.transform).GetComponent<PlayerBoard>();
            PlayerBoard playerBoard = Instantiate(playerBoardPrefab, this.gameObject.transform).GetComponent<PlayerBoard>();
            playerBoard.Init("PLAYERBOARD", serverCon, gameCustomID);
            enemyBoard.Init("ENEMYBOARD",serverCon, gameCustomID);
            playerBoard.gameObject.transform.Translate(0, -235, 0);
            enemyBoard.gameObject.transform.Translate(0, 235, 0);
            enemyBoard.gameObject.transform.Rotate(0, 0, 180);
        }
    }
}
