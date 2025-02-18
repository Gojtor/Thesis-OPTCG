using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu { 
    public class MenuScript : MonoBehaviour
    {
        [SerializeField]
        private GameObject startMenuPrefab;

        [SerializeField]
        private GameObject connectMenuPrefab;


        // Start is called before the first frame update
        void Start()
        {
        
        }
        public void StartGame()
        {
            //startMenuPrefab.SetActive(false);
            //connectMenuPrefab.SetActive(true);
            SceneManager.LoadSceneAsync("GameBoard");
        }
    }
}
