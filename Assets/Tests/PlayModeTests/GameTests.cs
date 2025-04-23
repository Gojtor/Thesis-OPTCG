using Microsoft.AspNetCore.SignalR.Client;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCGSim;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.TestTools;

public class GameTests : MonoBehaviour
{
    public static IEnumerator AwaitTask(Task task)
    {
        while (!task.IsCompleted)
            yield return null;
    }

    public static IEnumerator AwaitTaskWithReturn<T>(Task<T> task, Action<T> onComplete)
    {
        while (!task.IsCompleted)
            yield return null;

        if (task.IsCompletedSuccessfully)
        {
            onComplete?.Invoke(task.Result);
        }
    }

    private IEnumerator CreateGameManager()
    {
        if (GameManager.Instance != null)
        {
            UnityEngine.Object.Destroy(GameManager.Instance.gameObject);
            yield return null;
        }

        GameObject gameManagerObject = new GameObject("GameManager");
        GameManager gameManager = gameManagerObject.AddComponent<GameManager>();
        GameManager.Instance = gameManager;
        yield return null;
    }

    private IEnumerator DestroyGameManager()
    {
        if (GameManager.Instance != null)
        {
            UnityEngine.Object.Destroy(GameManager.Instance.gameObject);
            yield return null;
        }
        if (ChatManager.Instance != null)
        {
            UnityEngine.Object.Destroy(ChatManager.Instance.gameObject);
            yield return new WaitUntil(() => ChatManager.Instance == null);
        }
    }
    public static IEnumerator EnsureDispatcherExists()
    {
        if (UnityMainThreadDispatcher.Instance == null)
        {
            var go = new GameObject("Dispatcher");
            go.AddComponent<UnityMainThreadDispatcher>();
            yield return null;
        }
    }

    private IEnumerator LoadSceneAndInitBoards()
    {
        yield return EnsureDispatcherExists();
        Assert.IsTrue(UnityMainThreadDispatcher.Instance.enabled);
        yield return CreateGameManager();
        System.Random random = new System.Random();
        GameOptions.playerName = "TestPlayer1";
        GameOptions.selectedDeckForGame = "ST01-DefaultDeck,1xST01-001,4xST01-002,4xST01-003,4xST01-004,4xST01-005,4xST01-006,4xST01-007,4xST01-008,4xST01-009,4xST01-010,2xST01-011,2xST01-012,2xST01-013,2xST01-014,2xST01-015,2xST01-016,2xST01-017";
        GameOptions.gameID = "GameID" + random.Next(10000, 100000);
        GameManager.Instance.ChangeGameState(GameState.TESTING);
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
        SceneManager.LoadScene("GameBoard");
        yield return new WaitUntil(() => PlayerBoard.Instance != null);
        PlayerBoard.Instance.Init("PLAYERBOARD", GameOptions.gameID, "TestPlayer1");
        EnemyBoard.Instance.Init("ENEMYBOARD", GameOptions.gameID, "TestPlayer2");
        PlayerBoard.Instance.LoadBoardElements();
        EnemyBoard.Instance.LoadBoardElements();
        PlayerBoard.Instance.LoadDeckCardsForTesting(PlayerBoard.Instance.CreateTestCards());
        PlayerBoard.Instance.SetEnemyFinishedStartingHand(true);
        PlayerBoard.Instance.CreateGameButtons();
        PlayerBoard.Instance.CreateMockLeaderForTesting();
        EnemyBoard.Instance.CreateMockLeaderForTesting();
        yield return new WaitUntil(() => ServerCon.Instance.connection?.State == HubConnectionState.Connected);
    }

    [UnityTest]
    public IEnumerator BoardCreationTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.True(GameBoard.Instance.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.lifeObject.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.characterAreaObject.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.costAreaObject.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.trashObject.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.deckObject.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.stageObject.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.leaderObject.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.donDeckObject.gameObject.activeInHierarchy);
        Assert.True(EnemyBoard.Instance.gameObject.activeInHierarchy);
        Assert.True(EnemyBoard.Instance.lifeObject.gameObject.activeInHierarchy);
        Assert.True(EnemyBoard.Instance.characterAreaObject.gameObject.activeInHierarchy);
        Assert.True(EnemyBoard.Instance.costAreaObject.gameObject.activeInHierarchy);
        Assert.True(EnemyBoard.Instance.trashObject.gameObject.activeInHierarchy);
        Assert.True(EnemyBoard.Instance.deckObject.gameObject.activeInHierarchy);
        Assert.True(EnemyBoard.Instance.stageObject.gameObject.activeInHierarchy);
        Assert.True(EnemyBoard.Instance.leaderObject.gameObject.activeInHierarchy);
        Assert.True(EnemyBoard.Instance.donDeckObject.gameObject.activeInHierarchy);
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator StartingHandTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.deckCards.Count > 0, "There is no cards in deck");
        PlayerBoard.Instance.CreateStartingHand();
        Assert.True(PlayerBoard.Instance.handObject.hand.Count == 5, "Starting hand doesn't have exactly 5 cards");
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator CharacterCardMovingTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.deckCards.Count > 0, "There is no cards in deck");

        Card topDeckCard = PlayerBoard.Instance.deckCards.First();

        PlayerBoard.Instance.AddCardToHandFromDeck(topDeckCard, true, false);
        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent, "Moved cards parent is not correct");

        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(topDeckCard);
        Assert.AreEqual(PlayerBoard.Instance.characterAreaObject.transform, topDeckCard.transform.parent, "Moved cards parent is not correct");

        PlayerBoard.Instance.MoveCardToTrash(topDeckCard);
        Assert.AreEqual(PlayerBoard.Instance.trashObject.transform, topDeckCard.transform.parent, "Moved cards parent is not correct");

        PlayerBoard.Instance.MoveStageFromHandToStageArea(topDeckCard);
        Assert.AreEqual(PlayerBoard.Instance.stageObject.transform, topDeckCard.transform.parent, "Moved cards parent is not correct");

        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator DonMovingTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.deckCards.Count > 0, "There are no cards in deck");
        Assert.IsTrue(PlayerBoard.Instance.donCardsInDeck.Count > 0, "There are no dons in deck");

        DonCard topDon = PlayerBoard.Instance.donCardsInDeck.First().GetComponent<DonCard>();
        PlayerBoard.Instance.MoveDonFromDeckToCostArea(topDon);
        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent, "Moved don card parent is not correct");

        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator CharacterCardDraggingTestToCharacterArea()
    {
        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.deckCards.Count > 0, "There is no cards in deck");

        Card topDeckCard = PlayerBoard.Instance.deckCards.First();
        Assert.NotNull(topDeckCard);

        PlayerBoard.Instance.AddCardToHandFromDeck(topDeckCard, true, false);
        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent, "Moved cards parent is not correct");
        topDeckCard.ChangeDraggable(true);

        DonCard topDon = PlayerBoard.Instance.donCardsInDeck.First().GetComponent<DonCard>();
        PlayerBoard.Instance.MoveDonFromDeckToCostArea(topDon);
        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent, "Moved don card parent is not correct");

        yield return new WaitUntil(() => PlayerBoard.Instance.activeDon == 1);
        Vector2 startPos = Camera.main.WorldToScreenPoint(topDeckCard.transform.position);
        Vector2 endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.costAreaObject.transform.position);

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };
        //Test for costAreaObj drop
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.costAreaObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for donDeckObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.donDeckObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.donDeckObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for deckObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.deckObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.deckObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for stageObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.stageObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.stageObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for leaderAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.leaderObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.leaderObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for lifeObject drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.lifeObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.lifeObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for trashObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.trashObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.trashObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy costAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.costAreaObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.costAreaObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy donDeckObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.donDeckObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.donDeckObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy deckObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.deckObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.deckObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy stageObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.stageObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.stageObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy leaderAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.leaderObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.leaderObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy lifeObject drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.lifeObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.lifeObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy trashObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.trashObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.trashObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for null object drop
        endPos = Camera.main.WorldToScreenPoint(new Vector3(0, 0, 0));
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = null;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent);
        Assert.True(!topDon.rested);

        //Actual dragging to the character area
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.characterAreaObject.transform.position);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.characterAreaObject.gameObject;
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.characterAreaObject.transform, topDeckCard.transform.parent);
        Assert.True(topDon.rested);
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator StageCardDraggingTestToCharacterArea()
    {
        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.deckCards.Count > 0, "There is no cards in deck");

        Card stageCard = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardType == CardType.STAGE).First();
        Assert.NotNull(stageCard);

        PlayerBoard.Instance.AddCardToHandFromDeck(stageCard, true, false);
        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent, "Moved cards parent is not correct");
        stageCard.ChangeDraggable(true);

        DonCard topDon = PlayerBoard.Instance.donCardsInDeck.First().GetComponent<DonCard>();
        PlayerBoard.Instance.MoveDonFromDeckToCostArea(topDon);
        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent, "Moved don card parent is not correct");

        yield return new WaitUntil(() => PlayerBoard.Instance.activeDon == 1);
        Vector2 startPos = Camera.main.WorldToScreenPoint(stageCard.transform.position);
        Vector2 endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.costAreaObject.transform.position);

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };
        //Test for costAreaObj drop
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.costAreaObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for donDeckObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.donDeckObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.donDeckObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for deckObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.deckObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.deckObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for characterAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.characterAreaObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.characterAreaObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for leaderAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.leaderObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.leaderObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for lifeObject drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.lifeObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.lifeObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for trashObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.trashObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.trashObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy costAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.costAreaObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.costAreaObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy donDeckObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.donDeckObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.donDeckObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy deckObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.deckObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.deckObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy stageObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.stageObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.stageObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy leaderAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.leaderObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.leaderObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy lifeObject drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.lifeObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.lifeObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy trashObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.trashObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.trashObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for null object drop
        endPos = Camera.main.WorldToScreenPoint(new Vector3(0, 0, 0));
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = null;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, stageCard.transform.parent);
        Assert.True(!topDon.rested);

        //Actual dragging to the stage area
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.stageObject.transform.position);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.stageObject.gameObject;
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(stageCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.stageObject.transform, stageCard.transform.parent);
        Assert.True(topDon.rested);
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator EventCardDraggingTestToCharacterArea()
    {
        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.deckCards.Count > 0, "There is no cards in deck");

        Card eventCard = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardType == CardType.EVENT).First();
        Assert.NotNull(eventCard);

        PlayerBoard.Instance.AddCardToHandFromDeck(eventCard, true, false);
        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent, "Moved cards parent is not correct");
        eventCard.ChangeDraggable(true);

        DonCard topDon = PlayerBoard.Instance.donCardsInDeck.First().GetComponent<DonCard>();
        PlayerBoard.Instance.MoveDonFromDeckToCostArea(topDon);
        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent, "Moved don card parent is not correct");

        yield return new WaitUntil(() => PlayerBoard.Instance.activeDon == 1);
        Vector2 startPos = Camera.main.WorldToScreenPoint(eventCard.transform.position);
        Vector2 endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.costAreaObject.transform.position);

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };
        //Test for costAreaObj drop
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.costAreaObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for donDeckObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.donDeckObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.donDeckObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for deckObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.deckObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.deckObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for stageObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.stageObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.stageObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for leaderAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.leaderObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.leaderObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for lifeObject drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.lifeObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.lifeObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for trashObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.trashObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.trashObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy costAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.costAreaObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.costAreaObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy donDeckObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.donDeckObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.donDeckObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy deckObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.deckObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.deckObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy stageObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.stageObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.stageObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy leaderAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.leaderObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.leaderObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy lifeObject drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.lifeObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.lifeObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy trashObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.trashObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.trashObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Test for null object drop
        endPos = Camera.main.WorldToScreenPoint(new Vector3(0, 0, 0));
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = null;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.handObject.transform, eventCard.transform.parent);
        Assert.True(!topDon.rested);

        //Actual dragging to the character area
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.characterAreaObject.transform.position);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.characterAreaObject.gameObject;
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(eventCard.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.trashObject.transform, eventCard.transform.parent);
        Assert.True(topDon.rested);
        yield return DestroyGameManager();
    }
    [UnityTest]
    public IEnumerator DonDragTest()
    {

        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.donCardsInDeck.Count > 0, "There is no cards in deck");
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.DONPHASE);

        Card topDon = PlayerBoard.Instance.donCardsInDeck.First();
        Assert.NotNull(topDon);

        Vector2 startPos = Camera.main.WorldToScreenPoint(topDon.transform.position);
        Vector2 endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.characterAreaObject.transform.position);

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };

        //Test for characterAreaObj drop
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.characterAreaObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for donDeckObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.donDeckObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.donDeckObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for deckObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.deckObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.deckObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for stageObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.stageObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.stageObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for leaderAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.leaderObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.leaderObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for lifeObject drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.lifeObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.lifeObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for trashObj drop
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.trashObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.trashObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy costAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.costAreaObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.costAreaObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy donDeckObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.donDeckObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.donDeckObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy deckObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.deckObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.deckObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy stageObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.stageObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.stageObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy leaderAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.leaderObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.leaderObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy lifeObject drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.lifeObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.lifeObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for enemy trashObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.trashObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.trashObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for null object drop
        endPos = Camera.main.WorldToScreenPoint(new Vector3(0, 0, 0));
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = null;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);

        //Test for costAreaObj drop
        endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.trashObject.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.costAreaObject.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        yield return new WaitUntil(() => PlayerBoard.Instance.activeDon == 1);
        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);
        Assert.AreEqual(9, PlayerBoard.Instance.donCardsInDeck.Count);
        Assert.AreEqual(1, PlayerBoard.Instance.activeDon);
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator CardAttackTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.donCardsInDeck.Count > 0, "There is no cards in deck");

        Assert.AreEqual(0, EnemyBoard.Instance.cards.Count);
        EnemyBoard.Instance.CreateMockCard();
        Assert.AreEqual(1, EnemyBoard.Instance.cards.Count);

        Card enemyMockCard = EnemyBoard.Instance.cards[0];
        Assert.NotNull(enemyMockCard);
        enemyMockCard.Rest();

        CharacterCard topCard = PlayerBoard.Instance.deckCards.First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToHandFromDeck(topCard, true, false);
        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(topCard);
        topCard.CardCanAttack();
        topCard.SetCardActive();
        yield return new WaitUntil(() => topCard.lineRenderer != null);

        var pointer = new PointerEventData(EventSystem.current);
        Vector2 start = Camera.main.WorldToScreenPoint(topCard.transform.position);
        Vector2 end = Camera.main.WorldToScreenPoint(enemyMockCard.transform.position);
        pointer.position = start;
        ExecuteEvents.Execute(topCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        pointer.position = end;
        pointer.pointerEnter = enemyMockCard.gameObject;
        ExecuteEvents.Execute(topCard.gameObject, pointer, ExecuteEvents.pointerUpHandler);


        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);

        Assert.True(topCard.rested);
        Assert.True(!topCard.canAttack);
        Assert.NotNull(topCard.lineRenderer);
        Assert.AreEqual(topCard.transform.position, topCard.lineRenderer.GetPosition(0));
        Assert.AreEqual(enemyMockCard.transform.position, topCard.lineRenderer.GetPosition(1));
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator LeaderAttackTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.NotNull(PlayerBoard.Instance.leaderCard);

        Assert.AreEqual(0, EnemyBoard.Instance.cards.Count);
        EnemyBoard.Instance.CreateMockCard();
        Assert.AreEqual(1, EnemyBoard.Instance.cards.Count);

        Card enemyMockCard = EnemyBoard.Instance.cards[0];
        Assert.NotNull(enemyMockCard);
        enemyMockCard.Rest();

        LeaderCard leaderCard = PlayerBoard.Instance.leaderCard;
        leaderCard.CardCanAttack();
        leaderCard.SetCardActive();
        yield return new WaitUntil(() => leaderCard.lineRenderer != null);

        var pointer = new PointerEventData(EventSystem.current);
        Vector2 start = Camera.main.WorldToScreenPoint(leaderCard.transform.position);
        Vector2 end = Camera.main.WorldToScreenPoint(enemyMockCard.transform.position);
        pointer.position = start;
        ExecuteEvents.Execute(leaderCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        pointer.position = end;
        pointer.pointerEnter = enemyMockCard.gameObject;
        ExecuteEvents.Execute(leaderCard.gameObject, pointer, ExecuteEvents.pointerUpHandler);


        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);

        Assert.True(leaderCard.rested);
        Assert.True(!leaderCard.canAttack);
        Assert.NotNull(leaderCard.lineRenderer);
        Assert.AreEqual(leaderCard.transform.position, leaderCard.lineRenderer.GetPosition(0));
        Assert.AreEqual(enemyMockCard.transform.position, leaderCard.lineRenderer.GetPosition(1));
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator DonAttachTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.NotNull(PlayerBoard.Instance.leaderCard);
        Assert.IsTrue(PlayerBoard.Instance.donCardsInDeck.Count > 0, "There is no cards in deck");
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.DONPHASE);

        Assert.AreEqual(0, EnemyBoard.Instance.cards.Count);
        EnemyBoard.Instance.CreateMockCard();
        Assert.AreEqual(1, EnemyBoard.Instance.cards.Count);

        Card enemyMockCard = EnemyBoard.Instance.cards[0];
        Assert.NotNull(enemyMockCard);
        enemyMockCard.Rest();

        LeaderCard leaderCard = PlayerBoard.Instance.leaderCard;
        leaderCard.CardCanAttack();
        leaderCard.SetCardActive();
        yield return new WaitUntil(() => leaderCard.lineRenderer != null);

        CharacterCard topCard = PlayerBoard.Instance.deckCards.First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToHandFromDeck(topCard, true, false);
        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(topCard);
        topCard.CardCanAttack();
        topCard.SetCardActive();
        yield return new WaitUntil(() => topCard.lineRenderer != null);

        Card topDon = PlayerBoard.Instance.donCardsInDeck.First();
        Assert.NotNull(topDon);
        PlayerBoard.Instance.MoveDonFromDeckToCostArea(topDon);
        yield return new WaitUntil(() => PlayerBoard.Instance.activeDon == 1);
        Assert.AreEqual(9, PlayerBoard.Instance.donCardsInDeck.Count);
        Assert.AreEqual(1, PlayerBoard.Instance.activeDon);

        //Testing first for enemy leader
        Vector2 startPos = Camera.main.WorldToScreenPoint(topDon.transform.position);
        Vector2 endPos = Camera.main.WorldToScreenPoint(EnemyBoard.Instance.leaderObject.transform.GetChild(0).GetComponent<LeaderCard>().transform.position);

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };

        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = EnemyBoard.Instance.leaderObject.transform.GetChild(0).GetComponent<LeaderCard>().gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.AreEqual(1, PlayerBoard.Instance.activeDon);

        //Testing for enemy card
        endPos = Camera.main.WorldToScreenPoint(enemyMockCard.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = enemyMockCard.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.AreEqual(1, PlayerBoard.Instance.activeDon);

        //Testing for own leadercard
        endPos = Camera.main.WorldToScreenPoint(leaderCard.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = leaderCard.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.AreEqual(leaderCard.transform, topDon.transform.parent);
        Assert.AreEqual(1000, leaderCard.plusPower);
        Assert.True(leaderCard.IsPlusPowerTextActive());
        Assert.AreEqual("+1000", leaderCard.GetPowerText());
        Assert.AreEqual(0, PlayerBoard.Instance.activeDon);

        topDon = PlayerBoard.Instance.donCardsInDeck.First();
        Assert.NotNull(topDon);
        PlayerBoard.Instance.MoveDonFromDeckToCostArea(topDon);
        yield return new WaitUntil(() => PlayerBoard.Instance.activeDon == 1);
        Assert.AreEqual(8, PlayerBoard.Instance.donCardsInDeck.Count);
        Assert.AreEqual(1, PlayerBoard.Instance.activeDon);

        //Testing for own card
        endPos = Camera.main.WorldToScreenPoint(topCard.transform.position);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = topCard.gameObject;
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(topDon.gameObject, pointer, ExecuteEvents.endDragHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.AreEqual(topCard.transform, topDon.transform.parent);
        Assert.AreEqual(1000, topCard.plusPower);
        Assert.True(topCard.IsPlusPowerTextActive());
        Assert.AreEqual("+1000", topCard.GetPowerText());
        Assert.AreEqual(0, PlayerBoard.Instance.activeDon);
        yield return DestroyGameManager();
    }


    [UnityTest]
    public IEnumerator GettingAttackedTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.NotNull(PlayerBoard.Instance.leaderCard);
        Assert.IsTrue(PlayerBoard.Instance.donCardsInDeck.Count > 0, "There is no cards in deck");

        Assert.AreEqual(0, EnemyBoard.Instance.cards.Count);
        EnemyBoard.Instance.CreateMockCard();
        Assert.AreEqual(1, EnemyBoard.Instance.cards.Count);

        Card enemyMockCard = EnemyBoard.Instance.cards[0];
        Assert.NotNull(enemyMockCard);

        LeaderCard leaderCard = PlayerBoard.Instance.leaderCard;
        leaderCard.CardCanAttack();
        leaderCard.SetCardActive();
        yield return new WaitUntil(() => leaderCard.lineRenderer != null);

        CharacterCard topCard = PlayerBoard.Instance.deckCards.First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToLife(topCard);
        leaderCard.SetLife(1);

        topCard = PlayerBoard.Instance.deckCards.First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToHandFromDeck(topCard, true, false);

        enemyMockCard.Rest();
        yield return AwaitTask(PlayerBoard.Instance.EnemyAttacked(enemyMockCard.cardData.customCardID, leaderCard.cardData.customCardID, false));
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.True(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.AreEqual(1, leaderCard.life);
        Assert.AreEqual(1, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.NotNull(leaderCard.lineRenderer);

        PlayerBoard.Instance.noBlockBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.handObject.hand.First().HasBorder());
        Assert.AreEqual(1, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.AreEqual(1, leaderCard.life);
        Assert.NotNull(leaderCard.lineRenderer);

        PlayerBoard.Instance.noMoreCounterBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => leaderCard.life == 0);
        Assert.False(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.False(PlayerBoard.Instance.handObject.hand.First().HasBorder());
        Assert.AreEqual(0, leaderCard.life);
        Assert.AreEqual(0, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.AreEqual(2, PlayerBoard.Instance.handObject.hand.Count);
        Assert.False(leaderCard.lineRenderer.enabled);

        enemyMockCard.Restand(false, false);
        topCard = PlayerBoard.Instance.deckCards.First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToHandFromDeck(topCard, true, false);
        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(topCard);
        topCard.Rest();
        enemyMockCard.Rest();

        yield return AwaitTask(PlayerBoard.Instance.EnemyAttacked(enemyMockCard.cardData.customCardID, topCard.cardData.customCardID, false));
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.NotNull(topCard.lineRenderer);
        Assert.True(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.NotNull(topCard.lineRenderer);

        PlayerBoard.Instance.noBlockBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.handObject.hand.First().HasBorder());
        Assert.NotNull(topCard.lineRenderer);

        PlayerBoard.Instance.noMoreCounterBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => topCard.transform.parent == PlayerBoard.Instance.trashObject.transform);
        Assert.False(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.False(PlayerBoard.Instance.handObject.hand.First().HasBorder());
        Assert.AreEqual(PlayerBoard.Instance.trashObject.transform, topCard.transform.parent);
        Assert.AreEqual(0, leaderCard.life);
        Assert.AreEqual(0, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.AreEqual(2, PlayerBoard.Instance.handObject.hand.Count);
        Assert.False(topCard.lineRenderer.enabled);
        yield return DestroyGameManager();
    }


    [UnityTest]
    public IEnumerator ZeroLifeLoseConditionTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.NotNull(PlayerBoard.Instance.leaderCard);
        Assert.IsTrue(PlayerBoard.Instance.donCardsInDeck.Count > 0, "There is no cards in deck");

        Assert.AreEqual(0, EnemyBoard.Instance.cards.Count);
        EnemyBoard.Instance.CreateMockCard();
        Assert.AreEqual(1, EnemyBoard.Instance.cards.Count);

        Card enemyMockCard = EnemyBoard.Instance.cards[0];
        Assert.NotNull(enemyMockCard);

        CharacterCard topCard = PlayerBoard.Instance.deckCards.First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToHandFromDeck(topCard, true, false);

        LeaderCard leaderCard = PlayerBoard.Instance.leaderCard;
        leaderCard.CardCanAttack();
        leaderCard.SetCardActive();
        yield return new WaitUntil(() => leaderCard.lineRenderer != null);
        leaderCard.SetLife(0);

        enemyMockCard.Rest();
        yield return AwaitTask(PlayerBoard.Instance.EnemyAttacked(enemyMockCard.cardData.customCardID, leaderCard.cardData.customCardID, false));
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.True(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.AreEqual(0, leaderCard.life);
        Assert.AreEqual(0, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.NotNull(leaderCard.lineRenderer);

        PlayerBoard.Instance.noBlockBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.handObject.hand.First().HasBorder());
        Assert.AreEqual(0, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.AreEqual(0, leaderCard.life);
        Assert.NotNull(leaderCard.lineRenderer);

        PlayerBoard.Instance.noMoreCounterBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => leaderCard.life == -1);
        Assert.False(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.False(PlayerBoard.Instance.handObject.hand.First().HasBorder());
        Assert.AreEqual(-1, leaderCard.life);
        Assert.AreEqual(0, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.AreEqual(1, PlayerBoard.Instance.handObject.hand.Count);
        Assert.False(leaderCard.lineRenderer.enabled);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => GameManager.Instance.currentState == GameState.MATCHLOST);
        Assert.True(GameManager.Instance.currentState == GameState.MATCHLOST);
        yield return new WaitUntil(() => GameBoard.Instance.transform.Find("GameLost(Clone)") != null);
        Assert.True(GameBoard.Instance.transform.Find("GameLost(Clone)").gameObject.activeInHierarchy);
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator ZeroCardsInDeckLoseConditionTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.NotNull(PlayerBoard.Instance.leaderCard);
        Assert.IsTrue(PlayerBoard.Instance.donCardsInDeck.Count > 0, "There is no cards in deck");

        Assert.AreEqual(0, EnemyBoard.Instance.cards.Count);
        EnemyBoard.Instance.CreateMockCard();
        Assert.AreEqual(1, EnemyBoard.Instance.cards.Count);

        Card enemyMockCard = EnemyBoard.Instance.cards[0];
        Assert.NotNull(enemyMockCard);

        Card topCard;

        LeaderCard leaderCard = PlayerBoard.Instance.leaderCard;
        leaderCard.CardCanAttack();
        leaderCard.SetCardActive();
        yield return new WaitUntil(() => leaderCard.lineRenderer != null);
        for (int i = 0; i < 16; i++)
        {
            topCard = PlayerBoard.Instance.deckCards.First().GetComponent<Card>();
            PlayerBoard.Instance.AddCardToHandFromDeck(topCard, true, false);
            Assert.AreEqual(15 - i, PlayerBoard.Instance.deckCards.Count);
        }
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.DONPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.True(GameManager.Instance.currentState == GameState.MATCHLOST);
        yield return new WaitUntil(() => GameBoard.Instance.transform.Find("GameLost(Clone)") != null);
        Assert.True(GameBoard.Instance.transform.Find("GameLost(Clone)").gameObject.activeInHierarchy);
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator CounteringTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.NotNull(PlayerBoard.Instance.leaderCard);
        Assert.IsTrue(PlayerBoard.Instance.donCardsInDeck.Count > 0, "There is no cards in deck");

        Assert.AreEqual(0, EnemyBoard.Instance.cards.Count);
        EnemyBoard.Instance.CreateMockCard();
        Assert.AreEqual(1, EnemyBoard.Instance.cards.Count);

        Card enemyMockCard = EnemyBoard.Instance.cards[0];
        Assert.NotNull(enemyMockCard);

        LeaderCard leaderCard = PlayerBoard.Instance.leaderCard;
        leaderCard.CardCanAttack();
        leaderCard.SetCardActive();
        yield return new WaitUntil(() => leaderCard.lineRenderer != null);

        CharacterCard topCard = PlayerBoard.Instance.deckCards.First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToLife(topCard);
        leaderCard.SetLife(1);

        topCard = PlayerBoard.Instance.deckCards.First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToHandFromDeck(topCard, true, false);
        CharacterCard cardInHand = topCard;

        enemyMockCard.Rest();
        yield return AwaitTask(PlayerBoard.Instance.EnemyAttacked(enemyMockCard.cardData.customCardID, leaderCard.cardData.customCardID, false));
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.True(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.AreEqual(1, leaderCard.life);
        Assert.AreEqual(1, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.NotNull(leaderCard.lineRenderer);

        PlayerBoard.Instance.noBlockBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.True(cardInHand.HasBorder());
        Assert.AreEqual(1, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.AreEqual(1, leaderCard.life);
        Assert.NotNull(leaderCard.lineRenderer);

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = cardInHand.transform.position
        };
        ExecuteEvents.Execute(cardInHand.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.AreEqual(PlayerBoard.Instance.trashObject.transform, cardInHand.transform.parent);
        Assert.AreEqual(1000, leaderCard.plusPower);
        Assert.True(leaderCard.IsPlusPowerTextActive());

        PlayerBoard.Instance.noMoreCounterBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.False(cardInHand.HasBorder());
        Assert.AreEqual(1, leaderCard.life);
        Assert.AreEqual(1, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.AreEqual(0, PlayerBoard.Instance.handObject.hand.Count);
        Assert.False(leaderCard.lineRenderer.enabled);
        Assert.AreEqual(0, leaderCard.plusPower);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !leaderCard.IsPlusPowerTextActive());
        Assert.False(leaderCard.IsPlusPowerTextActive());
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator BlockingTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.NotNull(PlayerBoard.Instance.leaderCard);
        Assert.IsTrue(PlayerBoard.Instance.donCardsInDeck.Count > 0, "There is no cards in deck");

        Assert.AreEqual(0, EnemyBoard.Instance.cards.Count);
        EnemyBoard.Instance.CreateMockCard();
        Assert.AreEqual(1, EnemyBoard.Instance.cards.Count);

        Card enemyMockCard = EnemyBoard.Instance.cards[0];
        Assert.NotNull(enemyMockCard);

        LeaderCard leaderCard = PlayerBoard.Instance.leaderCard;
        leaderCard.CardCanAttack();
        leaderCard.SetCardActive();
        yield return new WaitUntil(() => leaderCard.lineRenderer != null);

        CharacterCard topCard = PlayerBoard.Instance.deckCards.First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToLife(topCard);
        leaderCard.SetLife(1);

        topCard = PlayerBoard.Instance.deckCards.First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToHandFromDeck(topCard, true, false);
        CharacterCard cardInHand = topCard;

        CharacterCard blocker = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-006").First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToHandFromDeck(blocker, true, false);
        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(blocker);
        blocker.SetCardActive();
        blocker.cardData.effect = "[Blocker] (After your opponent declares an attack, you may rest this card to make it the new target of the attack.)";

        enemyMockCard.Rest();
        yield return AwaitTask(PlayerBoard.Instance.EnemyAttacked(enemyMockCard.cardData.customCardID, leaderCard.cardData.customCardID, false));
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.True(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.AreEqual(1, leaderCard.life);
        Assert.AreEqual(1, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.NotNull(leaderCard.lineRenderer);
        Assert.True(blocker.HasBorder());

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = blocker.transform.position
        };
        ExecuteEvents.Execute(blocker.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !blocker.HasBorder());
        Assert.True(enemyMockCard.GetComponent<CharacterCard>().lineRenderer.GetPosition(1) == blocker.transform.position);
        Assert.False(blocker.HasBorder());

        PlayerBoard.Instance.noBlockBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.True(cardInHand.HasBorder());
        Assert.AreEqual(1, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.AreEqual(1, leaderCard.life);
        Assert.NotNull(leaderCard.lineRenderer);
        Assert.True(enemyMockCard.GetComponent<CharacterCard>().lineRenderer.GetPosition(1) == blocker.transform.position);

        PlayerBoard.Instance.noMoreCounterBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !cardInHand.HasBorder());
        Assert.False(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.False(cardInHand.HasBorder());
        Assert.AreEqual(1, leaderCard.life);
        Assert.AreEqual(1, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.AreEqual(1, PlayerBoard.Instance.handObject.hand.Count);
        Assert.False(leaderCard.lineRenderer.enabled);
        Assert.AreEqual(PlayerBoard.Instance.trashObject.transform, blocker.transform.parent);
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator CharacterAreaLimitTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.deckCards.Count > 0, "There is no cards in deck");

        DonCard topDon;
        for (int i = 0; i < 7; i++)
        {
            topDon = PlayerBoard.Instance.donCardsInDeck.First().GetComponent<DonCard>();
            PlayerBoard.Instance.MoveDonFromDeckToCostArea(topDon);
            Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent, "Moved don card parent is not correct");
        }

        yield return new WaitUntil(() => PlayerBoard.Instance.activeDon == 7);

        Card topDeckCard;

        for (int i = 0; i < 6; i++)
        {
            topDeckCard = PlayerBoard.Instance.deckCards.First();
            Assert.NotNull(topDeckCard);

            PlayerBoard.Instance.AddCardToHandFromDeck(topDeckCard, true, false);
            Assert.AreEqual(PlayerBoard.Instance.handObject.transform, topDeckCard.transform.parent, "Moved cards parent is not correct");
            topDeckCard.ChangeDraggable(true);

            Vector2 startPos = Camera.main.WorldToScreenPoint(topDeckCard.transform.position);
            Vector2 endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.characterAreaObject.transform.position);

            var pointer = new PointerEventData(EventSystem.current)
            {
                position = startPos
            };
            ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.beginDragHandler);

            pointer.position = endPos;
            pointer.pointerEnter = PlayerBoard.Instance.characterAreaObject.gameObject;
            ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(topDeckCard.gameObject, pointer, ExecuteEvents.endDragHandler);

            if (i == 5)
            {
                Assert.AreEqual(PlayerBoard.Instance.transform, topDeckCard.transform.parent);
                yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
                foreach (Card card in PlayerBoard.Instance.characterAreaCards)
                {
                    Assert.True(card.HasBorder());
                    Assert.True(card.targetForEffect);
                }
                Card cardToRemove = PlayerBoard.Instance.characterAreaCards.First();
                pointer = new PointerEventData(EventSystem.current)
                {
                    position = cardToRemove.transform.position
                };
                ExecuteEvents.Execute(cardToRemove.gameObject, pointer, ExecuteEvents.pointerClickHandler);
                yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
                Assert.AreEqual(PlayerBoard.Instance.characterAreaObject.transform, topDeckCard.transform.parent);
                Assert.AreEqual(PlayerBoard.Instance.trashObject.transform, cardToRemove.transform.parent);
                Assert.False(cardToRemove.cardData.active);
            }
            else
            {
                Assert.AreEqual(PlayerBoard.Instance.characterAreaObject.transform, topDeckCard.transform.parent);
            }
        }

    }

    [UnityTest]
    public IEnumerator CardEffectsTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.deckCards.Count > 0, "There is no cards in deck");

        Assert.AreEqual(0, EnemyBoard.Instance.cards.Count);
        EnemyBoard.Instance.CreateMockCard();
        Assert.AreEqual(1, EnemyBoard.Instance.cards.Count);

        Card enemyMockCard = EnemyBoard.Instance.cards[0];
        Assert.NotNull(enemyMockCard);
        enemyMockCard.Rest();

        DonCard topDon;
        for (int i = 0; i < 10; i++)
        {
            topDon = PlayerBoard.Instance.donCardsInDeck.First().GetComponent<DonCard>();
            PlayerBoard.Instance.MoveDonFromDeckToCostArea(topDon);
            topDon.SetCardActive();
            topDon.ChangeDraggable(true);
            Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent, "Moved don card parent is not correct");
        }

        yield return AwaitTaskWithReturn(PlayerBoard.Instance.CreateCardsFromDeck(), result => PlayerBoard.Instance.LoadDeckCardsForTesting(result));
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);

        CharacterCard usopp = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-002").First().GetComponent<CharacterCard>();
        CharacterCard sanji = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-004").First().GetComponent<CharacterCard>();
        CharacterCard jinbe = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-005").First().GetComponent<CharacterCard>();
        CharacterCard nami = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-007").First().GetComponent<CharacterCard>();
        CharacterCard brook = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-011").First().GetComponent<CharacterCard>();
        CharacterCard luffy = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-012").First().GetComponent<CharacterCard>();
        CharacterCard zoro = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-013").First().GetComponent<CharacterCard>();
        EventCard guardPoint = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-014").First().GetComponent<EventCard>();
        EventCard jetPistol = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-015").First().GetComponent<EventCard>();
        EventCard diableJambe = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-016").First().GetComponent<EventCard>();
        StageCard sunny = PlayerBoard.Instance.deckCards.Where(x => x.cardData.cardID == "ST01-017").First().GetComponent<StageCard>();

        //Testing Usopp effect activation
        PlayerBoard.Instance.AddCardToHandFromDeck(usopp, true, false);
        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(usopp);
        usopp.SetCardActive();
        usopp.CardCanAttack();
        yield return new WaitUntil(() => usopp.lineRenderer != null);
        Assert.True(usopp.HasBorder());
        Assert.True(usopp.canAttack);
        yield return new WaitUntil(() => usopp.lineRenderer != null);
        var pointer = new PointerEventData(EventSystem.current);
        Vector2 start = Camera.main.WorldToScreenPoint(usopp.transform.position);
        Vector2 end = Camera.main.WorldToScreenPoint(enemyMockCard.transform.position);
        pointer.position = start;
        ExecuteEvents.Execute(usopp.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        pointer.position = end;
        pointer.pointerEnter = enemyMockCard.gameObject;
        ExecuteEvents.Execute(usopp.gameObject, pointer, ExecuteEvents.pointerUpHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        foreach (Effects effect in usopp.effects)
        {
            if (effect.triggerType == EffectTriggerTypes.WhenAttacking)
            {
                Assert.False(WhenAttackingEnemyCantBlockOver.activated);
            }
        }
        usopp.Restand(true, true);
        usopp.lineRenderer.enabled = false;

        //Attaching don to Usopp
        Vector2 startPos;
        Vector2 endPos;
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);

        for (int i = 0; i < 2; i++)
        {
            DonCard don = PlayerBoard.Instance.donInCostArea[i].GetComponent<DonCard>();
            startPos = Camera.main.WorldToScreenPoint(don.gameObject.transform.position);
            endPos = Camera.main.WorldToScreenPoint(usopp.transform.position);
            pointer = new PointerEventData(EventSystem.current)
            {
                position = startPos
            };
            ExecuteEvents.Execute(don.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(don.gameObject, pointer, ExecuteEvents.beginDragHandler);
            pointer.position = endPos;
            pointer.pointerEnter = usopp.gameObject;
            ExecuteEvents.Execute(don.gameObject, pointer, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(don.gameObject, pointer, ExecuteEvents.endDragHandler);
            yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        }
        Assert.AreEqual(2000, usopp.plusPower);
        Assert.True(usopp.IsPlusPowerTextActive());
        usopp.SetCardActive();
        usopp.CardCanAttack();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.True(usopp.HasBorder());
        start = Camera.main.WorldToScreenPoint(usopp.transform.position);
        end = Camera.main.WorldToScreenPoint(enemyMockCard.transform.position);
        pointer.position = start;
        ExecuteEvents.Execute(usopp.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        pointer.position = end;
        pointer.pointerEnter = enemyMockCard.gameObject;
        ExecuteEvents.Execute(usopp.gameObject, pointer, ExecuteEvents.pointerUpHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        foreach (Effects effect in usopp.effects)
        {
            if (effect.triggerType == EffectTriggerTypes.WhenAttacking)
            {
                Assert.True(WhenAttackingEnemyCantBlockOver.activated);
            }
        }
        usopp.Restand(true, true);
        usopp.lineRenderer.enabled = false;
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.REFRESHPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !usopp.IsPlusPowerTextActive());
        Assert.False(usopp.HasBorder());
        Assert.False(usopp.IsPlusPowerTextActive());
        Assert.AreEqual(0, usopp.plusPower);
        PlayerBoard.Instance.MoveCardToTrash(usopp);
        Assert.AreEqual(PlayerBoard.Instance.trashObject.transform, usopp.transform.parent);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);

        //Testing Sanji effect activation
        PlayerBoard.Instance.AddCardToHandFromDeck(sanji, true, false);
        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(sanji);
        yield return new WaitUntil(() => sanji.lineRenderer != null);
        Assert.False(sanji.HasBorder());
        Assert.False(sanji.canAttack);
        DonCard donCard = PlayerBoard.Instance.donInCostArea[0].GetComponent<DonCard>();
        startPos = Camera.main.WorldToScreenPoint(donCard.gameObject.transform.position);
        endPos = Camera.main.WorldToScreenPoint(sanji.transform.position);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.beginDragHandler);
        pointer.position = endPos;
        pointer.pointerEnter = sanji.gameObject;
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.endDragHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => sanji.IsPlusPowerTextActive());
        Assert.False(sanji.HasBorder());
        Assert.False(sanji.canAttack);
        Assert.True(sanji.IsPlusPowerTextActive());
        Assert.AreEqual(1000, sanji.plusPower);
        donCard = PlayerBoard.Instance.donInCostArea[1].GetComponent<DonCard>();
        startPos = Camera.main.WorldToScreenPoint(donCard.gameObject.transform.position);
        endPos = Camera.main.WorldToScreenPoint(sanji.transform.position);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.beginDragHandler);
        pointer.position = endPos;
        pointer.pointerEnter = sanji.gameObject;
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.endDragHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => sanji.HasBorder());
        Assert.True(sanji.HasBorder());
        Assert.True(sanji.canAttack);
        Assert.True(sanji.IsPlusPowerTextActive());
        Assert.AreEqual(2000, sanji.plusPower);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.REFRESHPHASE);
        sanji.CardCannotAttack();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !sanji.IsPlusPowerTextActive());
        yield return new WaitUntil(() => !sanji.HasBorder());
        Assert.False(sanji.HasBorder());
        Assert.False(sanji.IsPlusPowerTextActive());
        Assert.AreEqual(0, sanji.plusPower);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);

        //Testing for Jinbe
        PlayerBoard.Instance.AddCardToHandFromDeck(jinbe, true, false);
        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(jinbe);
        jinbe.SetCardActive();
        jinbe.CardCanAttack();
        yield return new WaitUntil(() => jinbe.lineRenderer != null);
        Assert.True(jinbe.HasBorder());
        Assert.True(jinbe.canAttack);
        yield return new WaitUntil(() => jinbe.lineRenderer != null);
        pointer = new PointerEventData(EventSystem.current);
        start = Camera.main.WorldToScreenPoint(jinbe.transform.position);
        end = Camera.main.WorldToScreenPoint(enemyMockCard.transform.position);
        pointer.position = start;
        ExecuteEvents.Execute(jinbe.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        pointer.position = end;
        pointer.pointerEnter = enemyMockCard.gameObject;
        ExecuteEvents.Execute(jinbe.gameObject, pointer, ExecuteEvents.pointerUpHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.effectInProgress);
        Assert.False(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.False(sanji.targetForEffect);
        Assert.False(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.False(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);
        jinbe.Restand(true, true);
        jinbe.lineRenderer.enabled = false;

        //Attaching don to jinbe
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        donCard = PlayerBoard.Instance.donInCostArea[0].GetComponent<DonCard>();
        startPos = Camera.main.WorldToScreenPoint(donCard.gameObject.transform.position);
        endPos = Camera.main.WorldToScreenPoint(jinbe.transform.position);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.beginDragHandler);
        pointer.position = endPos;
        pointer.pointerEnter = jinbe.gameObject;
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.endDragHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.AreEqual(1000, jinbe.plusPower);
        Assert.True(jinbe.IsPlusPowerTextActive());
        jinbe.SetCardActive();
        jinbe.CardCanAttack();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.True(jinbe.HasBorder());
        start = Camera.main.WorldToScreenPoint(jinbe.transform.position);
        end = Camera.main.WorldToScreenPoint(enemyMockCard.transform.position);
        pointer.position = start;
        ExecuteEvents.Execute(jinbe.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        pointer.position = end;
        pointer.pointerEnter = enemyMockCard.gameObject;
        ExecuteEvents.Execute(jinbe.gameObject, pointer, ExecuteEvents.pointerUpHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.True(PlayerBoard.Instance.effectInProgress);
        Assert.False(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.True(sanji.targetForEffect);
        Assert.True(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.True(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = sanji.transform.position
        };
        ExecuteEvents.Execute(sanji.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !PlayerBoard.Instance.effectInProgress);
        Assert.False(PlayerBoard.Instance.effectInProgress);
        Assert.False(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.False(sanji.targetForEffect);
        Assert.False(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.True(sanji.IsPlusPowerTextActive());
        Assert.AreEqual(1000, sanji.plusPower);
        Assert.False(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);
        jinbe.Restand(true, true);
        jinbe.lineRenderer.enabled = false;
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.ENDPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangeGameState(GameState.TESTING);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.REFRESHPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !jinbe.IsPlusPowerTextActive());
        Assert.False(jinbe.HasBorder());
        Assert.False(jinbe.IsPlusPowerTextActive());
        Assert.AreEqual(0, jinbe.plusPower);
        Assert.False(sanji.IsPlusPowerTextActive());
        Assert.AreEqual(0, sanji.plusPower);

        //Testing for Nami
        GameManager.Instance.ChangeGameState(GameState.TESTING);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        PlayerBoard.Instance.AddCardToHandFromDeck(nami, true, false);
        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(nami);
        nami.SetCardActive();
        donCard = PlayerBoard.Instance.donInCostArea[0].GetComponent<DonCard>();
        donCard.RestDon();
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = nami.transform.position,
            button = PointerEventData.InputButton.Right
        };
        ExecuteEvents.Execute(nami.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => PlayerBoard.Instance.effectInProgress);
        Assert.True(PlayerBoard.Instance.effectInProgress);
        Assert.True(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.True(nami.targetForEffect);
        Assert.True(sanji.targetForEffect);
        Assert.True(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.True(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = sanji.transform.position,
        };
        ExecuteEvents.Execute(sanji.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => PlayerBoard.Instance.effectInProgress);
        Assert.True(PlayerBoard.Instance.effectInProgress);
        Assert.False(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.False(nami.targetForEffect);
        Assert.False(sanji.targetForEffect);
        Assert.False(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.True(donCard.targetForEffect);
        Assert.True(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);

        pointer = new PointerEventData(EventSystem.current)
        {
            position = donCard.transform.position,
        };
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.effectInProgress);
        Assert.False(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.False(nami.targetForEffect);
        Assert.False(sanji.targetForEffect);
        Assert.False(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.False(donCard.targetForEffect);
        Assert.True(sanji.IsPlusPowerTextActive());
        Assert.True(sanji.hasDonAttached);
        Assert.AreEqual(1000, sanji.plusPower);
        Assert.AreEqual(1, sanji.GetAttachedDonCount());
        Assert.False(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);

        //Testing for Brook
        GameManager.Instance.ChangeGameState(GameState.TESTING);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        PlayerBoard.Instance.AddCardToHandFromDeck(brook, true, false);
        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(nami);
        nami.SetCardActive();
        DonCard donCard1 = PlayerBoard.Instance.donInCostArea[0].GetComponent<DonCard>();
        donCard1.RestDon();
        DonCard donCard2 = PlayerBoard.Instance.donInCostArea[1].GetComponent<DonCard>();
        donCard2.RestDon();
        brook.ChangeDraggable(true);
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        startPos = Camera.main.WorldToScreenPoint(brook.transform.position);
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.characterAreaObject.transform.position);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };
        ExecuteEvents.Execute(brook.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(brook.gameObject, pointer, ExecuteEvents.beginDragHandler);
        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.characterAreaObject.gameObject;
        ExecuteEvents.Execute(brook.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(brook.gameObject, pointer, ExecuteEvents.endDragHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => PlayerBoard.Instance.effectInProgress);
        Assert.True(PlayerBoard.Instance.effectInProgress);
        Assert.True(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.True(nami.targetForEffect);
        Assert.True(sanji.targetForEffect);
        Assert.True(brook.targetForEffect);
        Assert.True(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.True(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = sanji.transform.position,
        };
        ExecuteEvents.Execute(sanji.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.True(PlayerBoard.Instance.effectInProgress);
        Assert.False(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.False(nami.targetForEffect);
        Assert.False(sanji.targetForEffect);
        Assert.False(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.True(donCard1.targetForEffect);
        Assert.True(donCard2.targetForEffect);
        Assert.True(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = donCard1.transform.position,
        };
        ExecuteEvents.Execute(donCard1.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = donCard2.transform.position,
        };
        ExecuteEvents.Execute(donCard2.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.effectInProgress);
        Assert.False(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.False(nami.targetForEffect);
        Assert.False(sanji.targetForEffect);
        Assert.False(brook.targetForEffect);
        Assert.False(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.False(donCard1.targetForEffect);
        Assert.False(donCard2.targetForEffect);
        Assert.True(sanji.IsPlusPowerTextActive());
        Assert.True(sanji.hasDonAttached);
        Assert.AreEqual(2000, sanji.plusPower);
        Assert.AreEqual(2, sanji.GetAttachedDonCount());
        Assert.False(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.REFRESHPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);

        //Testing for character Luffy
        PlayerBoard.Instance.AddCardToHandFromDeck(luffy, true, false);
        luffy.ChangeDraggable(true);
        startPos = Camera.main.WorldToScreenPoint(luffy.transform.position);
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.characterAreaObject.transform.position);

        pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };
        ExecuteEvents.Execute(luffy.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(luffy.gameObject, pointer, ExecuteEvents.beginDragHandler);
        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.characterAreaObject.gameObject;
        ExecuteEvents.Execute(luffy.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(luffy.gameObject, pointer, ExecuteEvents.endDragHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => luffy.lineRenderer != null);
        Assert.True(luffy.HasBorder());
        Assert.True(luffy.canAttack);
        yield return new WaitUntil(() => luffy.lineRenderer != null);
        pointer = new PointerEventData(EventSystem.current);
        start = Camera.main.WorldToScreenPoint(luffy.transform.position);
        end = Camera.main.WorldToScreenPoint(enemyMockCard.transform.position);
        pointer.position = start;
        ExecuteEvents.Execute(luffy.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        pointer.position = end;
        pointer.pointerEnter = enemyMockCard.gameObject;
        ExecuteEvents.Execute(luffy.gameObject, pointer, ExecuteEvents.pointerUpHandler);

        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        foreach (Effects effect in luffy.effects)
        {
            if (effect.triggerType == EffectTriggerTypes.WhenAttacking)
            {
                Assert.False(WhenAttackingEnemyCantBlockOver.activated);
            }
        }
        luffy.Restand(true, true);
        luffy.lineRenderer.enabled = false;

        //Attaching don to Luffy
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);

        for (int i = 0; i < 2; i++)
        {
            DonCard don = PlayerBoard.Instance.donInCostArea[i].GetComponent<DonCard>();
            startPos = Camera.main.WorldToScreenPoint(don.gameObject.transform.position);
            endPos = Camera.main.WorldToScreenPoint(luffy.transform.position);
            pointer = new PointerEventData(EventSystem.current)
            {
                position = startPos
            };
            ExecuteEvents.Execute(don.gameObject, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(don.gameObject, pointer, ExecuteEvents.beginDragHandler);
            pointer.position = endPos;
            pointer.pointerEnter = luffy.gameObject;
            ExecuteEvents.Execute(don.gameObject, pointer, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(don.gameObject, pointer, ExecuteEvents.endDragHandler);
            yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        }
        Assert.AreEqual(2000, luffy.plusPower);
        Assert.True(luffy.IsPlusPowerTextActive());
        luffy.SetCardActive();
        luffy.CardCanAttack();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.True(luffy.HasBorder());
        start = Camera.main.WorldToScreenPoint(luffy.transform.position);
        end = Camera.main.WorldToScreenPoint(enemyMockCard.transform.position);
        pointer.position = start;
        ExecuteEvents.Execute(usopp.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        pointer.position = end;
        pointer.pointerEnter = enemyMockCard.gameObject;
        ExecuteEvents.Execute(luffy.gameObject, pointer, ExecuteEvents.pointerUpHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        foreach (Effects effect in luffy.effects)
        {
            if (effect.triggerType == EffectTriggerTypes.WhenAttacking)
            {
                Assert.True(WhenAttackingEnemyCantBlockOver.activated);
            }
        }
        luffy.Restand(true, true);
        luffy.lineRenderer.enabled = false;
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.REFRESHPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !luffy.IsPlusPowerTextActive());
        Assert.False(luffy.HasBorder());
        Assert.False(luffy.IsPlusPowerTextActive());
        Assert.AreEqual(0, luffy.plusPower);
        PlayerBoard.Instance.MoveCardToTrash(luffy);
        Assert.AreEqual(PlayerBoard.Instance.trashObject.transform, luffy.transform.parent);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);


        //Testing for Zoro
        PlayerBoard.Instance.AddCardToHandFromDeck(zoro, true, false);
        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(zoro);
        yield return new WaitUntil(() => zoro.lineRenderer != null);
        donCard = PlayerBoard.Instance.donInCostArea[0].GetComponent<DonCard>();
        startPos = Camera.main.WorldToScreenPoint(donCard.gameObject.transform.position);
        endPos = Camera.main.WorldToScreenPoint(zoro.transform.position);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.beginDragHandler);
        pointer.position = endPos;
        pointer.pointerEnter = zoro.gameObject;
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(donCard.gameObject, pointer, ExecuteEvents.endDragHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => zoro.IsPlusPowerTextActive());
        Assert.False(zoro.HasBorder());
        Assert.False(zoro.canAttack);
        Assert.True(zoro.IsPlusPowerTextActive());
        Assert.AreEqual(2000, zoro.plusPower);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.ENDPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !zoro.HasBorder());
        yield return new WaitUntil(() => zoro.IsPlusPowerTextActive());
        Assert.False(zoro.HasBorder());
        Assert.True(zoro.IsPlusPowerTextActive());
        Assert.AreEqual(1000, zoro.plusPower);
        GameManager.Instance.ChangeGameState(GameState.TESTING);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.REFRESHPHASE);
        zoro.CardCannotAttack();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !zoro.IsPlusPowerTextActive());
        yield return new WaitUntil(() => !zoro.HasBorder());
        Assert.False(zoro.HasBorder());
        Assert.False(zoro.IsPlusPowerTextActive());
        Assert.AreEqual(0, zoro.plusPower);
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);

        //Testing effect for Sunny
        GameManager.Instance.ChangeGameState(GameState.TESTING);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        PlayerBoard.Instance.AddCardToHandFromDeck(sunny, true, false);
        PlayerBoard.Instance.MoveStageFromHandToStageArea(sunny);
        sunny.SetCardActive();
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = sunny.transform.position,
            button = PointerEventData.InputButton.Right
        };
        ExecuteEvents.Execute(sunny.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => PlayerBoard.Instance.effectInProgress);
        Assert.True(PlayerBoard.Instance.effectInProgress);
        Assert.True(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.False(luffy.targetForEffect);
        Assert.True(nami.targetForEffect);
        Assert.True(sanji.targetForEffect);
        Assert.True(zoro.targetForEffect);
        Assert.True(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.True(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);

        pointer = new PointerEventData(EventSystem.current)
        {
            position = sanji.transform.position,
        };
        ExecuteEvents.Execute(sanji.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => sanji.IsPlusPowerTextActive());
        Assert.False(PlayerBoard.Instance.effectInProgress);
        Assert.False(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.False(nami.targetForEffect);
        Assert.False(sanji.targetForEffect);
        Assert.False(zoro.targetForEffect);
        Assert.False(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.True(sanji.IsPlusPowerTextActive());
        Assert.False(sanji.hasDonAttached);
        Assert.AreEqual(1000, sanji.plusPower);
        Assert.AreEqual(0, sanji.GetAttachedDonCount());
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);

        //Testing for Diable Jambe
        PlayerBoard.Instance.AddCardToHandFromDeck(diableJambe, true, false);
        diableJambe.ChangeDraggable(true);
        startPos = Camera.main.WorldToScreenPoint(diableJambe.transform.position);
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.characterAreaObject.transform.position);

        pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };
        ExecuteEvents.Execute(diableJambe.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(diableJambe.gameObject, pointer, ExecuteEvents.beginDragHandler);

        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.characterAreaObject.gameObject;
        ExecuteEvents.Execute(diableJambe.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(diableJambe.gameObject, pointer, ExecuteEvents.endDragHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => PlayerBoard.Instance.effectInProgress);
        Assert.True(PlayerBoard.Instance.effectInProgress);
        Assert.False(usopp.targetForEffect);
        Assert.False(luffy.targetForEffect);
        Assert.True(jinbe.targetForEffect);
        Assert.True(nami.targetForEffect);
        Assert.True(sanji.targetForEffect);
        Assert.True(zoro.targetForEffect);
        Assert.True(brook.targetForEffect);
        Assert.True(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.True(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);
        Assert.AreEqual(PlayerBoard.Instance.transform, diableJambe.transform.parent);
        Assert.AreEqual(0, brook.effects.Where(x => x.triggerType == EffectTriggerTypes.WhenAttacking).Count());
        pointer = new PointerEventData(EventSystem.current)
        {
            position = brook.transform.position,
        };
        ExecuteEvents.Execute(brook.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.effectInProgress);
        Assert.False(jinbe.targetForEffect);
        Assert.False(usopp.targetForEffect);
        Assert.False(nami.targetForEffect);
        Assert.False(sanji.targetForEffect);
        Assert.False(zoro.targetForEffect);
        Assert.False(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.AreEqual(PlayerBoard.Instance.trashObject.transform, diableJambe.transform.parent);
        Assert.AreEqual(1, brook.effects.Where(x => x.triggerType == EffectTriggerTypes.WhenAttacking).Count());
        brook.CardCanAttack();
        yield return new WaitUntil(() => brook.lineRenderer != null);
        pointer = new PointerEventData(EventSystem.current);
        start = Camera.main.WorldToScreenPoint(brook.transform.position);
        end = Camera.main.WorldToScreenPoint(enemyMockCard.transform.position);
        pointer.position = start;
        ExecuteEvents.Execute(brook.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        pointer.position = end;
        pointer.pointerEnter = enemyMockCard.gameObject;
        ExecuteEvents.Execute(brook.gameObject, pointer, ExecuteEvents.pointerUpHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        foreach (Effects effect in brook.effects)
        {
            if (effect.triggerType == EffectTriggerTypes.WhenAttacking)
            {
                Assert.True(WhenAttackingEnemyCantBlockOver.activated);
            }
        }
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.ENDPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangeGameState(GameState.TESTING);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.AreEqual(0, brook.effects.Where(x => x.triggerType == EffectTriggerTypes.WhenAttacking).Count());
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);

        //Testing for Jet Pistol
        PlayerBoard.Instance.AddCardToHandFromDeck(jetPistol, true, false);
        jetPistol.ChangeDraggable(true);
        startPos = Camera.main.WorldToScreenPoint(jetPistol.transform.position);
        endPos = Camera.main.WorldToScreenPoint(PlayerBoard.Instance.characterAreaObject.transform.position);

        pointer = new PointerEventData(EventSystem.current)
        {
            position = startPos
        };
        ExecuteEvents.Execute(jetPistol.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(jetPistol.gameObject, pointer, ExecuteEvents.beginDragHandler);
        pointer.position = endPos;
        pointer.pointerEnter = PlayerBoard.Instance.characterAreaObject.gameObject;
        ExecuteEvents.Execute(jetPistol.gameObject, pointer, ExecuteEvents.dragHandler);
        ExecuteEvents.Execute(jetPistol.gameObject, pointer, ExecuteEvents.endDragHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => PlayerBoard.Instance.effectInProgress);
        Assert.True(PlayerBoard.Instance.effectInProgress);
        Assert.False(usopp.targetForEffect);
        Assert.False(luffy.targetForEffect);
        Assert.False(jinbe.targetForEffect);
        Assert.False(nami.targetForEffect);
        Assert.False(sanji.targetForEffect);
        Assert.False(zoro.targetForEffect);
        Assert.False(brook.targetForEffect);
        Assert.False(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.True(enemyMockCard.targetForEffect);
        Assert.True(PlayerBoard.Instance.cancelBtn.gameObject.activeInHierarchy);
        Assert.AreEqual(PlayerBoard.Instance.transform, jetPistol.transform.parent);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = enemyMockCard.transform.position,
        };
        ExecuteEvents.Execute(enemyMockCard.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.effectInProgress);
        Assert.False(usopp.targetForEffect);
        Assert.False(luffy.targetForEffect);
        Assert.False(jinbe.targetForEffect);
        Assert.False(nami.targetForEffect);
        Assert.False(sanji.targetForEffect);
        Assert.False(zoro.targetForEffect);
        Assert.False(brook.targetForEffect);
        Assert.False(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.False(enemyMockCard.targetForEffect);
        Assert.AreEqual(PlayerBoard.Instance.trashObject.transform, jetPistol.transform.parent);
        foreach (Effects effect in brook.effects)
        {
            if (effect.triggerType == EffectTriggerTypes.Main)
            {
                Assert.True(KoEnemyBasedOnPowerOrLess.activated);
            }
        }
        brook.lineRenderer.enabled = false;
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.MAINPHASE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        GameManager.Instance.ChangeBattlePhase(BattlePhases.NOBATTLE);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);

        //Testing for Guard Point
        PlayerBoard.Instance.AddCardToHandFromDeck(guardPoint, true, false);
        guardPoint.SetCardActive();
        LeaderCard leaderCard = PlayerBoard.Instance.leaderCard;
        leaderCard.CardCanAttack();
        leaderCard.SetCardActive();
        yield return new WaitUntil(() => leaderCard.lineRenderer != null);
        enemyMockCard.Rest();
        yield return AwaitTask(PlayerBoard.Instance.EnemyAttacked(enemyMockCard.cardData.customCardID, leaderCard.cardData.customCardID, false));
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.True(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.AreEqual(5, leaderCard.life);
        Assert.AreEqual(0, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.NotNull(leaderCard.lineRenderer);
        PlayerBoard.Instance.noBlockBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.noBlockBtn.gameObject.activeInHierarchy);
        Assert.True(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.True(guardPoint.HasBorder());
        Assert.AreEqual(0, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.AreEqual(5, leaderCard.life);
        Assert.NotNull(leaderCard.lineRenderer);
        pointer = new PointerEventData(EventSystem.current)
        {
            position = guardPoint.transform.position
        };
        ExecuteEvents.Execute(guardPoint.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !PlayerBoard.Instance.effectInProgress);
        Assert.False(PlayerBoard.Instance.effectInProgress);
        Assert.False(usopp.targetForEffect);
        Assert.False(luffy.targetForEffect);
        Assert.False(jinbe.targetForEffect);
        Assert.False(nami.targetForEffect);
        Assert.False(sanji.targetForEffect);
        Assert.False(zoro.targetForEffect);
        Assert.False(brook.targetForEffect);
        Assert.False(PlayerBoard.Instance.leaderCard.targetForEffect);
        Assert.False(enemyMockCard.targetForEffect);
        Assert.AreEqual(PlayerBoard.Instance.trashObject.transform, guardPoint.transform.parent);
        Assert.True(leaderCard.IsPlusPowerTextActive());
        Assert.AreEqual(3000, leaderCard.plusPower);
        foreach (Effects effect in guardPoint.effects)
        {
            if (effect.triggerType == EffectTriggerTypes.Counter)
            {
                Assert.True(EventGiveCounter.activated);
            }
        }
        PlayerBoard.Instance.noMoreCounterBtn.onClick.Invoke();
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        Assert.False(PlayerBoard.Instance.noMoreCounterBtn.gameObject.activeInHierarchy);
        Assert.AreEqual(5, leaderCard.life);
        Assert.AreEqual(0, PlayerBoard.Instance.lifeObject.lifeCards.Count);
        Assert.AreEqual(0, PlayerBoard.Instance.handObject.hand.Count);
        Assert.False(leaderCard.lineRenderer.enabled);
        Assert.AreEqual(0, leaderCard.plusPower);
        yield return new WaitUntil(() => !UnityMainThreadDispatcher.isProcessing);
        yield return new WaitUntil(() => !leaderCard.IsPlusPowerTextActive());
        Assert.False(leaderCard.IsPlusPowerTextActive());
        yield return DestroyGameManager();

    }
}
