using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TCGSim
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState currentState { get; private set; } = GameState.MAINMENU;

        public static event Action<GameState> OnGameStateChange;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        public void ChangeGameState(GameState gameState)
        {
            currentState = gameState;

            switch (gameState)
            {
                case GameState.CONNECTING:
                    break;
                case GameState.WAITINGFOROPPONENT:
                    break;
                case GameState.STARTINGPHASE:
                    break;
                case GameState.PLAYERPHASE:
                    break;
                case GameState.ENEMYPHASE:
                    break;
                default:
                    break;
            }

            OnGameStateChange?.Invoke(gameState);
        }

    }
}
