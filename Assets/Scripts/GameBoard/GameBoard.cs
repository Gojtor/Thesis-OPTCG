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
        private GameObject cardPrefab;

        List<PlayerBoard> playerBoards = new List<PlayerBoard>();
        Card testCard;
    // Start is called before the first frame update
        void Start()
        {
            CreateBoard();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void CreateBoard()
        {
            PlayerBoard board1 = Instantiate(playerBoardPrefab, this.gameObject.transform).GetComponent<PlayerBoard>();
            PlayerBoard board2 = Instantiate(playerBoardPrefab, this.gameObject.transform).GetComponent<PlayerBoard>();
            board1.gameObject.transform.Translate(0, -270, 0);
            board2.gameObject.transform.Translate(0, 270,0);
            board2.gameObject.transform.Rotate(0, 0, 180);
            cardPrefab = Instantiate(cardPrefab, this.gameObject.transform);
        }
    }
}
