using Microsoft.AspNetCore.SignalR.Client;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCGSim;
using TCGSim.CardResources;
using TCGSim.CardScripts;
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
}
