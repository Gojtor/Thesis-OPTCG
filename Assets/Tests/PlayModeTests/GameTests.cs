using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCGSim;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.TestTools;

public class GameTests : MonoBehaviour
{
    GameManager gameManagerMock = new GameManager();
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
        if(ChatManager.Instance != null)
        {
            UnityEngine.Object.Destroy(ChatManager.Instance.gameObject);
            yield return new WaitUntil(() => ChatManager.Instance == null);
        }
    }

    private IEnumerator LoadSceneAndInitBoards()
    {
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
    public IEnumerator CardMovingTest()
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
    public IEnumerator CardDraggingTestToCharacterArea()
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
    public IEnumerator DonDragTest()
    {

        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.donCardsInDeck.Count > 0, "There is no cards in deck");
        GameManager.Instance.ChangePlayerTurnPhase(PlayerTurnPhases.DONPHASE);

        Card topDon = PlayerBoard.Instance.donCardsInDeck.First();
        Assert.NotNull(topDon);
        topDon.ChangeDraggable(true);

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

        Assert.AreEqual(PlayerBoard.Instance.costAreaObject.transform, topDon.transform.parent);
        Assert.True(!topDon.rested);
        yield return DestroyGameManager();
    }

    [UnityTest]
    public IEnumerator CardAttackTest()
    {
        yield return LoadSceneAndInitBoards();
        Assert.IsTrue(PlayerBoard.Instance.donCardsInDeck.Count > 0, "There is no cards in deck");

        Assert.AreEqual(0,EnemyBoard.Instance.cards.Count);
        EnemyBoard.Instance.CreateMockCard();
        Assert.AreEqual(1,EnemyBoard.Instance.cards.Count);

        Card enemyMockCard = EnemyBoard.Instance.cards[0];
        Assert.NotNull(enemyMockCard);
        enemyMockCard.Rest();

        CharacterCard topCard = PlayerBoard.Instance.deckCards.First().GetComponent<CharacterCard>();
        PlayerBoard.Instance.AddCardToHandFromDeck(topCard, true, false);
        PlayerBoard.Instance.MoveCardFromHandToCharacterArea(topCard);
        topCard.CardCanAttack();
        topCard.SetCardActive();
        yield return new WaitUntil(() =>topCard.lineRenderer!=null);

        var pointer = new PointerEventData(EventSystem.current);
        Vector2 start = Camera.main.WorldToScreenPoint(topCard.transform.position);
        Vector2 end = Camera.main.WorldToScreenPoint(enemyMockCard.transform.position);
        pointer.position = start;
        ExecuteEvents.Execute(topCard.gameObject, pointer, ExecuteEvents.pointerDownHandler);
        pointer.position = end;
        pointer.pointerEnter = enemyMockCard.gameObject;
        ExecuteEvents.Execute(topCard.gameObject, pointer, ExecuteEvents.pointerUpHandler);

        yield return new WaitForSeconds(5);
    }
}
