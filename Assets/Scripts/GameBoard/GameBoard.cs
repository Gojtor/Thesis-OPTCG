using System.Collections;
using System.Collections.Generic;
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

        List<PlayerBoard> playerBoards = new List<PlayerBoard>();

        List<string> deck = new List<string>();

    // Start is called before the first frame update
        void Start()
        {
            if (serverConPrefab == null)
            {
                Debug.LogError("Prefab NINCS BEÁLLÍTVA!", this);
                return;
            }
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
            if (serverCon == null)
            {
                Debug.LogError("ServerCon NULL before Init!", this);
            }
            playerBoard.Init("Player Board",serverCon);
            enemyBoard.Init("Enemy Board",serverCon);
            playerBoard.gameObject.transform.Translate(0, -235, 0);
            enemyBoard.gameObject.transform.Translate(0, 235, 0);
            enemyBoard.gameObject.transform.Rotate(0, 0, 180);
            playerBoard.CreateDeck();
            playerBoard.CreateHand();
            playerBoard.CreateLife();
            enemyBoard.CreateDeck();
            enemyBoard.CreateHand();
            enemyBoard.CreateLife();
        }
    }
}
