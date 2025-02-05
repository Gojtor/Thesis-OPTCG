using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu { 
    public class MenuScript : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }
        public void PlayGame()
        {
            SceneManager.LoadSceneAsync("GameBoard");
        }
    }
}
