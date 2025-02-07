using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TCGSim
{
    public class GameBoard : MonoBehaviour
    {
        [SerializeField]
        private GameObject playerBoardPrefab;

        List<PlayerBoard> playerBoards = new List<PlayerBoard>();
    // Start is called before the first frame update
        void Start()
        {
            CreateBoards();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void CreateBoards()
        {
            PlayerBoard playerBoard = Instantiate(playerBoardPrefab, this.gameObject.transform).GetComponent<PlayerBoard>();
            PlayerBoard enemyBoard = Instantiate(playerBoardPrefab, this.gameObject.transform).GetComponent<PlayerBoard>();
            playerBoard.Init("Player Board");
            enemyBoard.Init("Enemy Board");
            playerBoard.gameObject.transform.Translate(0, -270, 0);
            enemyBoard.gameObject.transform.Translate(0, 270,0);
            enemyBoard.gameObject.transform.Rotate(0, 0, 180);
            playerBoard.CreateHand();
        }
    }
}
