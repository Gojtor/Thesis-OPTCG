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
        private GameObject handPrefab;

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
            PlayerBoard board1 = Instantiate(playerBoardPrefab, this.gameObject.transform).GetComponent<PlayerBoard>();
            PlayerBoard board2 = Instantiate(playerBoardPrefab, this.gameObject.transform).GetComponent<PlayerBoard>();
            board1.gameObject.transform.Translate(0, -270, 0);
            board2.gameObject.transform.Translate(0, 270,0);
            board2.gameObject.transform.Rotate(0, 0, 180);
            Hand p1Hand = Instantiate(handPrefab, this.gameObject.transform).GetComponent<Hand>();
            p1Hand.DrawCard();
        }
    }
}
