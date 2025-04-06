using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCGSim.CardScripts;

namespace TCGSim
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState currentState { get; private set; } = GameState.MAINMENU;
        public PlayerTurnPhases currentPlayerTurnPhase { get; private set; }
        public BattlePhases currentBattlePhase { get; private set; } = BattlePhases.NOBATTLE;

        public static event Action<GameState> OnGameStateChange;
        public static event Action<PlayerTurnPhases> OnPlayerTurnPhaseChange;
        public static event Action<BattlePhases, Card, Card> OnBattlePhaseChange;
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

        public void ChangePlayerTurnPhase(PlayerTurnPhases turnPhase)
        {
            currentPlayerTurnPhase = turnPhase;

            switch (turnPhase)
            {
                case PlayerTurnPhases.REFRESHPHASE:
                    break;
                case PlayerTurnPhases.DRAWPHASE:
                    break;
                case PlayerTurnPhases.DONPHASE:
                    break;
                case PlayerTurnPhases.MAINPHASE:
                    break;
                case PlayerTurnPhases.ENDPHASE:
                    break;
            }

            OnPlayerTurnPhaseChange?.Invoke(turnPhase);
        }

        public void ChangeBattlePhase(BattlePhases battlePhase, Card attacker = null,Card attacked = null)
        {
            currentBattlePhase = battlePhase;
            OnBattlePhaseChange?.Invoke(battlePhase,attacker,attacked);
        }

    }
}
