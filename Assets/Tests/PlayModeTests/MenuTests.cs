using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.ServerCon;
using NUnit.Framework;
using TCGSim;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.TestTools;
using UnityEngine.UI;
using static UnityEditorInternal.ReorderableList;

public class MenuTests
{
    Menu menu;
    GameManager gameManager;
    LoginManager loginManager;

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

    public IEnumerator LoadMenuScene()
    {

        SceneManager.LoadScene("Menu", LoadSceneMode.Single);

        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Menu");

        GameObject menuObj = GameObject.Find("MainMenu");
        Assert.NotNull(menuObj, "MainMenu object is loaded after the scene is loaded");
        menu = menuObj.GetComponent<Menu>();
        Assert.NotNull(menu, "Menu script component can be found in MainMenu object!");

        GameObject gameManagerObj = GameObject.Find("GameManager");
        Assert.NotNull(gameManagerObj, "GameManager object is loaded after the scene is loaded");
        gameManager = gameManagerObj.GetComponent<GameManager>();
        Assert.NotNull(gameManager, "GameManager script component can be found in GameManager object!");

        GameObject loginManagerObj = GameObject.Find("LoginManager");
        Assert.NotNull(loginManagerObj, "LoginManager object is loaded after the scene is loaded");
        loginManager = loginManagerObj.GetComponent<LoginManager>();
        Assert.NotNull(loginManager, "LoginManager script component can be found in LoginManager object!");

        yield return null;
    }

    [UnityTest]
    public IEnumerator RegisterTest()
    {
        yield return new WaitForSeconds(3);
        yield return LoadMenuScene();

        GameObject loginRegisterPanel = GameObject.Find("LoginRegisterPanel(Clone)");
        GameObject registerBtnObj = loginRegisterPanel.transform.Find("RegisterBtn").gameObject;
        Assert.NotNull(registerBtnObj, "Register button object is loaded after the scene is loaded");
        Button registerBtn = registerBtnObj.GetComponent<Button>();
        Assert.NotNull(registerBtn, "Register functions as a button");
        Assert.True(registerBtnObj.transform.parent == loginRegisterPanel.transform, "Register button can be found in LoginRegisterPanel object!");

        GameObject registerPanel = GameObject.Find("RegisterPanel(Clone)");
        Assert.Null(registerPanel, "Can't be seen and found because its not active until register button is clicked");
        registerBtn.onClick.Invoke();
        registerPanel = GameObject.Find("RegisterPanel(Clone)");
        Assert.True(registerPanel.activeInHierarchy, "Register panel is visible after register button is clicked");

        TMP_InputField nameInput = GameObject.Find("NameInput").GetComponent<TMP_InputField>();
        TMP_InputField passwordInput = GameObject.Find("PasswordInput").GetComponent<TMP_InputField>();
        Button registerBtnInRegisterPanel = registerPanel.transform.Find("RegisterBtn").gameObject.GetComponent<Button>();
        Button backBtn = GameObject.Find("BackBtn").GetComponent<Button>();

        Assert.NotNull(nameInput, "Name input shows after clicking on register");
        Assert.NotNull(passwordInput, "Password input shows after clicking on register");
        Assert.NotNull(registerBtnInRegisterPanel, "Register button shows after clicking on register");
        Assert.NotNull(backBtn, "Back button shows after clicking on register");

        Assert.IsTrue(nameInput.text == string.Empty, "Nothing is writteng at name input at first");
        Assert.IsTrue(passwordInput.text == string.Empty, "Nothing is writteng at password input at first");

        GameObject registerFailed = GameObject.Find("RegisterFailed(Clone)");
        Assert.Null(registerFailed, "Register failed panel is not active until register button is clicked without data in inputs or with wrong datas");
        registerBtnInRegisterPanel.onClick.Invoke();
        yield return new WaitUntil(() =>
        {
            registerFailed = GameObject.Find("RegisterFailed(Clone)");
            return registerFailed != null && registerFailed.activeInHierarchy;
        });
        Assert.NotNull(registerFailed, "Register failed panel is active after the register button is clicked without data");
        Button registerFailBackBtn = registerFailed.transform.Find("Back").GetComponent<Button>();
        Assert.NotNull(registerFailBackBtn, "Back button can be seen on register failed panel");
        registerFailBackBtn.onClick.Invoke();
        registerFailed = GameObject.Find("RegisterFailed(Clone)");
        Assert.Null(registerFailed, "After clicking on back button register failed panel is not active");

        nameInput.text = "Test";
        passwordInput.text = "Test";
        nameInput.onEndEdit.Invoke("Test");
        passwordInput.onEndEdit.Invoke("Test");
        registerBtnInRegisterPanel.onClick.Invoke();
        yield return new WaitUntil(() =>
        {
            registerFailed = GameObject.Find("RegisterFailed(Clone)");
            return registerFailed != null && registerFailed.activeInHierarchy;
        });
        Assert.NotNull(registerFailed, "Register failed panel is active after the register button is clicked with wrong datas");
        registerFailBackBtn.onClick.Invoke();
        registerFailed = GameObject.Find("RegisterFailed(Clone)");
        Assert.Null(registerFailed, "After clicking on back button register failed panel is not active");

        System.Random random = new System.Random();
        int randomNumb = random.Next(10000, 100000);
        nameInput.text = "Test" + randomNumb;
        passwordInput.text = "Test-" + randomNumb;
        nameInput.onEndEdit.Invoke("Test" + randomNumb);
        passwordInput.onEndEdit.Invoke("Test-" + randomNumb);
        GameObject defaultMenu = GameObject.Find("DefaultPanel(Clone)");
        Assert.Null(defaultMenu, "Default menu is not shown until correct register happens");
        registerBtnInRegisterPanel.onClick.Invoke();
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        yield return AwaitTask(menu.Menu_succesFullyRegistered());
        Assert.True(defaultMenu.activeInHierarchy, "After clicking register with valid username and password default menu panel shows");
        TextMeshProUGUI loggedInText = GameObject.Find("LoggedInAs").GetComponent<TextMeshProUGUI>();
        Button createGameBtn = GameObject.Find("CreateGameBtn").GetComponent<Button>();
        Button connectGameBtn = GameObject.Find("ConnectGameBtn").GetComponent<Button>();
        Button quitGameBtn = GameObject.Find("QuitGameBtn").GetComponent<Button>();
        Button friendsBtn = GameObject.Find("FriendsBtn(Clone)").GetComponent<Button>();

        Assert.True(createGameBtn.gameObject.activeInHierarchy, "Default menu button create game can be seen after register");
        Assert.True(connectGameBtn.gameObject.activeInHierarchy, "Default menu button connect game can be seen after register");
        Assert.True(quitGameBtn.gameObject.activeInHierarchy, "Default menu button quit game can be seen after register");
        Assert.True(friendsBtn.gameObject.activeInHierarchy, "Default menu button friends can be seen after register");
        yield return new WaitUntil(() =>
        {
            loggedInText = GameObject.Find("LoggedInAs").GetComponent<TextMeshProUGUI>();
            return loggedInText.text.Contains("Test" + randomNumb);
        });
        Assert.True(loggedInText.text.Contains("Test" + randomNumb), "Logged in as text is the same as the username that was used");

        yield return null;
    }


    [UnityTest]
    public IEnumerator LoginTest()
    {
        yield return LoadMenuScene();

        GameObject loginRegisterPanel = GameObject.Find("LoginRegisterPanel(Clone)");
        GameObject loginBtnObj = GameObject.Find("LoginBtn");
        Assert.NotNull(loginBtnObj, "loginBtnObj object is loaded after the scene is loaded");
        Button loginBtn = loginBtnObj.GetComponent<Button>();
        Assert.NotNull(loginBtn, "Login functions as a button");
        Assert.True(loginBtnObj.transform.parent == loginRegisterPanel.transform, "Login button can be found in LoginRegisterPanel object!");

        GameObject loginPanel = GameObject.Find("LoginPanel(Clone)");
        Assert.Null(loginPanel, "Can't be seen and found becaus its not active until login button is clicked");
        loginBtn.onClick.Invoke();
        loginPanel = GameObject.Find("LoginPanel(Clone)");
        Assert.True(loginPanel.activeInHierarchy, "Login panel is visible after login button is clicked");

        TMP_InputField nameInput = GameObject.Find("NameInput").GetComponent<TMP_InputField>();
        TMP_InputField passwordInput = GameObject.Find("PasswordInput").GetComponent<TMP_InputField>();
        Button loginBtnInLoginPanel = loginPanel.transform.Find("LoginBtn").gameObject.GetComponent<Button>();
        Button backBtn = GameObject.Find("BackBtn").GetComponent<Button>();

        Assert.NotNull(nameInput, "Name input shows after clicking on login");
        Assert.NotNull(passwordInput, "Password input shows after clicking on login");
        Assert.NotNull(loginBtnInLoginPanel, "Login button shows after clicking on login");
        Assert.NotNull(backBtn, "Back button shows after clicking on login");

        Assert.IsTrue(nameInput.text == string.Empty, "Nothing is writteng at name input at first");
        Assert.IsTrue(passwordInput.text == string.Empty, "Nothing is writteng at password input at first");

        GameObject loginFailed = GameObject.Find("LoginFailed(Clone)");
        Assert.Null(loginFailed, "Login failed panel is not active until login button is clicked without data in inputs or with wrong datas");
        loginBtnInLoginPanel.onClick.Invoke();
        yield return new WaitUntil(() =>
        {
            loginFailed = GameObject.Find("LoginFailed(Clone)");
            return loginFailed != null && loginFailed.activeInHierarchy;
        });
        Assert.NotNull(loginFailed, "Login failed panel is active after the login button is clicked without data");
        Button loginFailBackBtn = loginFailed.transform.Find("Back").GetComponent<Button>();
        Assert.NotNull(loginFailBackBtn, "Back button can be seen on login failed panel");
        loginFailBackBtn.onClick.Invoke();
        loginFailed = GameObject.Find("LoginFailed(Clone)");
        Assert.Null(loginFailed, "After clicking on back button login failed panel is not active");

        nameInput.text = "Test";
        passwordInput.text = "Test";
        nameInput.onEndEdit.Invoke("Test");
        passwordInput.onEndEdit.Invoke("Test");
        loginBtnInLoginPanel.onClick.Invoke();
        yield return new WaitUntil(() =>
        {
            loginFailed = GameObject.Find("LoginFailed(Clone)");
            return loginFailed != null && loginFailed.activeInHierarchy;
        });
        Assert.NotNull(loginFailed, "Login failed panel is active after the login button is clicked with wrong datas");
        loginFailBackBtn.onClick.Invoke();
        loginFailed = GameObject.Find("LoginFailed(Clone)");
        Assert.Null(loginFailed, "After clicking on back button login failed panel is not active");

        nameInput.text = "Test123";
        passwordInput.text = "Test-123";
        nameInput.onEndEdit.Invoke("Test123");
        passwordInput.onEndEdit.Invoke("Test-123");
        GameObject defaultMenu = GameObject.Find("DefaultPanel(Clone)");
        Assert.Null(defaultMenu, "Default menu is not shown until correct login happens");
        loginBtnInLoginPanel.onClick.Invoke();
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        yield return AwaitTask(menu.Menu_succesFullyLoggedIn());
        Assert.True(defaultMenu.activeInHierarchy, "After clicking login with valid username and password default menu panel shows");
        TextMeshProUGUI loggedInText = GameObject.Find("LoggedInAs").GetComponent<TextMeshProUGUI>();
        Button createGameBtn = GameObject.Find("CreateGameBtn").GetComponent<Button>();
        Button connectGameBtn = GameObject.Find("ConnectGameBtn").GetComponent<Button>();
        Button quitGameBtn = GameObject.Find("QuitGameBtn").GetComponent<Button>();
        Button friendsBtn = GameObject.Find("FriendsBtn(Clone)").GetComponent<Button>();

        Assert.True(createGameBtn.gameObject.activeInHierarchy, "Default menu button create game can be seen after login");
        Assert.True(connectGameBtn.gameObject.activeInHierarchy, "Default menu button connect game can be seen after login");
        Assert.True(quitGameBtn.gameObject.activeInHierarchy, "Default menu button quit game can be seen after login");
        Assert.True(friendsBtn.gameObject.activeInHierarchy, "Default menu button friends can be seen after login");
        yield return new WaitUntil(() =>
        {
            loggedInText = GameObject.Find("LoggedInAs").GetComponent<TextMeshProUGUI>();
            return loggedInText.text.Contains("Test123");
        });
        Assert.True(loggedInText.text.Contains("Test123"), "Logged in as text is the same as the username that was used");

        yield return null;
    }

    [UnityTest]
    public IEnumerator LoginAsGuestTest()
    {
        yield return LoadMenuScene();

        GameObject loginRegisterPanel = GameObject.Find("LoginRegisterPanel(Clone)");
        Button loginGuestBtn = loginRegisterPanel.transform.Find("LoginGuestBtn").GetComponent<Button>();
        Assert.True(loginGuestBtn.gameObject.activeInHierarchy, "Login as guest button is visible and active after scene loads");
        GameObject defaultMenu = GameObject.Find("DefaultPanel(Clone)");
        Assert.Null(defaultMenu, "Default menu is not shown until login as guest button is clicked");
        loginGuestBtn.onClick.Invoke();
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        Assert.True(defaultMenu.activeInHierarchy, "After clicking on login as guest the default menu is shown");
        yield return null;
    }

    public IEnumerator LoginUser(string userName, string password)
    {
        yield return LoadMenuScene();

        GameObject loginRegisterPanel = GameObject.Find("LoginRegisterPanel(Clone)");
        Button loginBtn = loginRegisterPanel.transform.Find("LoginBtn").gameObject.GetComponent<Button>();
        Assert.NotNull(loginBtn, "Register functions as a button");

        loginBtn.onClick.Invoke();
        GameObject loginPanel = GameObject.Find("LoginPanel(Clone)");
        Assert.True(loginPanel.activeInHierarchy, "Login panel is visible after login button is clicked");

        TMP_InputField nameInput = GameObject.Find("NameInput").GetComponent<TMP_InputField>();
        TMP_InputField passwordInput = GameObject.Find("PasswordInput").GetComponent<TMP_InputField>();
        Button loginBtnInLoginPanel = loginPanel.transform.Find("LoginBtn").gameObject.GetComponent<Button>();

        nameInput.text = userName;
        passwordInput.text = password;
        nameInput.onEndEdit.Invoke(userName);
        passwordInput.onEndEdit.Invoke(password);
        GameObject defaultMenu = GameObject.Find("DefaultPanel(Clone)");
        Assert.Null(defaultMenu, "Default menu is not shown until correct login happens");
        loginBtnInLoginPanel.onClick.Invoke();
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        yield return AwaitTask(menu.Menu_succesFullyLoggedIn());
    }

    public IEnumerator RegisterUser(string userName, string password)
    {
        yield return LoadMenuScene();

        GameObject loginRegisterPanel = GameObject.Find("LoginRegisterPanel(Clone)");
        Button registerBtn = loginRegisterPanel.transform.Find("RegisterBtn").gameObject.GetComponent<Button>();
        Assert.NotNull(registerBtn, "Register functions as a button");

        registerBtn.onClick.Invoke();
        GameObject registerPanel = GameObject.Find("RegisterPanel(Clone)");
        Assert.True(registerPanel.activeInHierarchy, "Register panel is visible after register button is clicked");

        TMP_InputField nameInput = GameObject.Find("NameInput").GetComponent<TMP_InputField>();
        TMP_InputField passwordInput = GameObject.Find("PasswordInput").GetComponent<TMP_InputField>();
        Button registerBtnInRegisterPanel = registerPanel.transform.Find("RegisterBtn").gameObject.GetComponent<Button>();

        nameInput.text = userName;
        passwordInput.text = password;
        nameInput.onEndEdit.Invoke(userName);
        passwordInput.onEndEdit.Invoke(password);
        GameObject defaultMenu = GameObject.Find("DefaultPanel(Clone)");
        Assert.Null(defaultMenu, "Default menu is not shown until correct register happens");
        registerBtnInRegisterPanel.onClick.Invoke();
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        yield return AwaitTask(menu.Menu_succesFullyRegistered());
    }


    [UnityTest]
    public IEnumerator FriendRequestTests()
    {

        System.Random random = new System.Random();
        int firstUserID = random.Next(10000, 100000);
        int secondUserID = random.Next(10000, 100000);
        yield return RegisterUser("Test" + secondUserID, "Test-" + secondUserID);
        yield return AwaitTask(menu.Menu_succesFullyRegistered());
        GameObject defaultMenu = GameObject.Find("DefaultPanel(Clone)");
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });

        yield return RegisterUser("Test" + firstUserID, "Test-" + firstUserID);
        yield return AwaitTask(menu.Menu_succesFullyRegistered());
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        Button friendsBtn = GameObject.Find("FriendsBtn(Clone)").GetComponent<Button>();
        GameObject friendsPanel = GameObject.Find("FriendsPanel(Clone)");
        Assert.Null(friendsPanel, "Friends panel is not visible until friends button is clicked");
        yield return AwaitTask(menu.FriendsButtonClicked());
        yield return new WaitUntil(() =>
        {
            friendsPanel = GameObject.Find("FriendsPanel(Clone)");
            return friendsPanel != null && friendsPanel.activeInHierarchy;
        });
        Assert.True(friendsPanel.activeInHierarchy, "Friends panel is visible after friends button is clicked");

        GameObject friendListPanel = GameObject.Find("FriendListPanel");
        GameObject requestListPanel = GameObject.Find("FriendRequestsPanel");
        GameObject sendRequestPanel = GameObject.Find("SendRequestPanel");
        Assert.True(friendListPanel.activeInHierarchy, "Friend list is visible in friends panel after clicking on friends button");
        Assert.True(requestListPanel.activeInHierarchy, "Friend request list is visible in friends panel after clicking on friends button");
        Assert.True(sendRequestPanel.activeInHierarchy, "Send friend request is visible in friends panel after clicking on friends button");

        GameObject friendListContent = friendListPanel.transform.Find("FriendList").Find("Viewport").Find("Content").gameObject;
        GameObject friendRequestsContent = requestListPanel.transform.Find("FriendRequest").Find("Viewport").Find("Content").gameObject;
        TMP_InputField sendRequestInput = sendRequestPanel.transform.Find("SendNameInput").GetComponent<TMP_InputField>();
        Button sendReqBtn = sendRequestPanel.transform.Find("Send").GetComponent<Button>();

        Assert.True(friendListContent.activeInHierarchy, "Friend list content is visible");
        Assert.AreEqual(0, friendListContent.transform.childCount);
        Assert.True(friendRequestsContent.activeInHierarchy, "Friend request list content is visible");
        Assert.AreEqual(0, friendRequestsContent.transform.childCount);
        Assert.True(sendRequestInput.gameObject.activeInHierarchy, "Friend list content is visible");
        Assert.True(sendReqBtn.gameObject.activeInHierarchy, "Friend list content is visible");

        sendRequestInput.text = "Test" + secondUserID;
        sendRequestInput.onValueChanged.Invoke("Test" + secondUserID);
        yield return AwaitTask(menu.SendFriendRequest());
        yield return AwaitTask(menu.FriendsButtonClicked());
        friendsPanel = GameObject.Find("FriendsPanel(Clone)");
        Assert.Null(friendsPanel, "Friends panel is not visible after friends button is clicked");

        yield return AwaitTask(menu.FriendsButtonClicked());
        yield return new WaitUntil(() =>
        {
            friendsPanel = GameObject.Find("FriendsPanel(Clone)");
            return friendsPanel != null && friendsPanel.activeInHierarchy;
        });
        friendsPanel = GameObject.Find("FriendsPanel(Clone)");
        Assert.True(friendsPanel.activeInHierarchy, "Friends panel is visible after friends button is clicked");
        friendListContent = friendListPanel.transform.Find("FriendList").Find("Viewport").Find("Content").gameObject;
        friendRequestsContent = requestListPanel.transform.Find("FriendRequest").Find("Viewport").Find("Content").gameObject;
        sendRequestInput = sendRequestPanel.transform.Find("SendNameInput").GetComponent<TMP_InputField>();
        sendReqBtn = sendRequestPanel.transform.Find("Send").GetComponent<Button>();
        Assert.True(friendListContent.activeInHierarchy, "Friend list content is visible");
        Assert.AreEqual(0, friendListContent.transform.childCount);
        Assert.True(friendRequestsContent.activeInHierarchy, "Friend request list content is visible");
        Assert.AreEqual(0, friendRequestsContent.transform.childCount);
        Assert.True(sendRequestInput.gameObject.activeInHierarchy, "Friend list content is visible");
        Assert.True(sendReqBtn.gameObject.activeInHierarchy, "Friend list content is visible");

        yield return LoginUser("Test" + secondUserID, "Test-" + secondUserID);
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        yield return AwaitTask(menu.FriendsButtonClicked());
        friendListPanel = GameObject.Find("FriendListPanel");
        requestListPanel = GameObject.Find("FriendRequestsPanel");
        sendRequestPanel = GameObject.Find("SendRequestPanel");
        Assert.True(friendListPanel.activeInHierarchy, "Friend list is visible in friends panel after clicking on friends button");
        Assert.True(requestListPanel.activeInHierarchy, "Friend request list is visible in friends panel after clicking on friends button");
        Assert.True(sendRequestPanel.activeInHierarchy, "Send friend request is visible in friends panel after clicking on friends button");
        friendListContent = friendListPanel.transform.Find("FriendList").Find("Viewport").Find("Content").gameObject;
        friendRequestsContent = requestListPanel.transform.Find("FriendRequest").Find("Viewport").Find("Content").gameObject;
        sendRequestInput = sendRequestPanel.transform.Find("SendNameInput").GetComponent<TMP_InputField>();
        sendReqBtn = sendRequestPanel.transform.Find("Send").GetComponent<Button>();
        Assert.True(friendListContent.activeInHierarchy, "Friend list content is visible");
        Assert.AreEqual(0, friendListContent.transform.childCount);
        Assert.True(friendRequestsContent.activeInHierarchy, "Friend request list content is visible");
        Assert.AreEqual(1, friendRequestsContent.transform.childCount);
        Assert.True(sendRequestInput.gameObject.activeInHierarchy, "Friend list content is visible");
        Assert.True(sendReqBtn.gameObject.activeInHierarchy, "Friend list content is visible");
        GameObject requestWindow = friendRequestsContent.transform.Find("Request(Clone)").gameObject;
        TextMeshProUGUI reqName = requestWindow.transform.Find("ReqName").GetComponent<TextMeshProUGUI>();
        Button accept = requestWindow.transform.Find("Accept").GetComponent<Button>();
        Button decline = requestWindow.transform.Find("Decline").GetComponent<Button>();
        Assert.True(reqName.gameObject.activeInHierarchy && reqName.text == "Test" + firstUserID);
        Assert.True(accept.gameObject.activeInHierarchy);
        Assert.True(decline.gameObject.activeInHierarchy);
        yield return AwaitTask(menu.DeclineFriendRequest("Test" + firstUserID, requestWindow));
        Assert.AreEqual(0, friendRequestsContent.transform.childCount, "After declining the request the request dissapears");

        yield return LoginUser("Test" + firstUserID, "Test-" + firstUserID);
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        yield return AwaitTask(menu.FriendsButtonClicked());
        sendRequestPanel = GameObject.Find("SendRequestPanel");
        sendRequestInput = sendRequestPanel.transform.Find("SendNameInput").GetComponent<TMP_InputField>();
        sendRequestInput.text = "Test" + secondUserID;
        sendRequestInput.onValueChanged.Invoke("Test" + secondUserID);
        yield return AwaitTask(menu.SendFriendRequest());

        yield return LoginUser("Test" + secondUserID, "Test-" + secondUserID);
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        yield return AwaitTask(menu.FriendsButtonClicked());
        requestListPanel = GameObject.Find("FriendRequestsPanel");
        friendListPanel = GameObject.Find("FriendListPanel");
        friendListContent = friendListPanel.transform.Find("FriendList").Find("Viewport").Find("Content").gameObject;
        friendRequestsContent = requestListPanel.transform.Find("FriendRequest").Find("Viewport").Find("Content").gameObject;
        Assert.AreEqual(1, friendRequestsContent.transform.childCount);
        requestWindow = friendRequestsContent.transform.Find("Request(Clone)").gameObject;
        yield return AwaitTask(menu.AcceptFriendRequest("Test" + firstUserID, requestWindow));
        Assert.AreEqual(0, friendRequestsContent.transform.childCount, "After accepting the request the request dissapears");
        Assert.AreEqual(0, friendListContent.transform.childCount, "After accepting the friend request the friend is cannot be seen until the friend window is refreshed");
        yield return AwaitTask(menu.FriendsButtonClicked());
        yield return AwaitTask(menu.FriendsButtonClicked());
        Assert.AreEqual(1, friendListContent.transform.childCount);
        Assert.AreEqual("Test" + firstUserID, friendListContent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);

        yield return LoginUser("Test" + firstUserID, "Test-" + firstUserID);
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        yield return AwaitTask(menu.FriendsButtonClicked());
        friendListPanel = GameObject.Find("FriendListPanel");
        friendListContent = friendListPanel.transform.Find("FriendList").Find("Viewport").Find("Content").gameObject;
        Assert.AreEqual(1, friendListContent.transform.childCount);
        Assert.AreEqual("Test" + secondUserID, friendListContent.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text);
    }

    [UnityTest]
    public IEnumerator ConnectGamePanelTest()
    {
        System.Random random = new System.Random();
        int randomUserID = random.Next(10000, 100000);
        yield return RegisterUser("Test" + randomUserID, "Test-" + randomUserID);
        yield return AwaitTask(menu.Menu_succesFullyRegistered());
        GameObject defaultMenu = GameObject.Find("DefaultPanel(Clone)");
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        TextMeshProUGUI loggedInText = GameObject.Find("LoggedInAs").GetComponent<TextMeshProUGUI>();
        Button createGameBtn = GameObject.Find("CreateGameBtn").GetComponent<Button>();
        Button connectGameBtn = GameObject.Find("ConnectGameBtn").GetComponent<Button>();
        Button quitGameBtn = GameObject.Find("QuitGameBtn").GetComponent<Button>();
        Button friendsBtn = GameObject.Find("FriendsBtn(Clone)").GetComponent<Button>();
        GameObject connectGamePanel = GameObject.Find("CreateGamePanel(Clone)");

        Assert.True(createGameBtn.gameObject.activeInHierarchy, "Default menu button create game can be seen after login");
        Assert.True(connectGameBtn.gameObject.activeInHierarchy, "Default menu button connect game can be seen after login");
        Assert.True(quitGameBtn.gameObject.activeInHierarchy, "Default menu button quit game can be seen after login");
        Assert.True(friendsBtn.gameObject.activeInHierarchy, "Default menu button friends can be seen after login");
        Assert.Null(connectGamePanel);

        connectGameBtn.onClick.Invoke();
        connectGamePanel = GameObject.Find("CreateGamePanel(Clone)");
        Assert.True(connectGamePanel.activeInHierarchy);
        GameObject playerNameText = connectGamePanel.transform.Find("NamePanel").Find("PlayerNameText(Clone)").gameObject;
        GameObject gameIDText = connectGamePanel.transform.Find("GameIDPanel").Find("GameIDInput").gameObject;
        GameObject deckSelector = connectGamePanel.transform.Find("DeckSelector").gameObject;
        Button connectToGameBtn = connectGamePanel.transform.Find("ConnectToGameBtn").GetComponent<Button>();
        Button backBtn = GameObject.Find("BackBtn").GetComponent<Button>();

        Assert.True(playerNameText.activeInHierarchy);
        Assert.True(gameIDText.activeInHierarchy);
        Assert.True(deckSelector.activeInHierarchy);
        Assert.True(connectToGameBtn.gameObject.activeInHierarchy);
        Assert.True(backBtn.gameObject.activeInHierarchy);
        Assert.AreEqual("Test" + randomUserID, playerNameText.GetComponent<TextMeshProUGUI>().text);
        Assert.AreEqual(1, deckSelector.GetComponent<TMP_Dropdown>().options.Count);
        Assert.AreEqual("ST01-DefaultDeck", deckSelector.GetComponent<TMP_Dropdown>().transform.Find("Label").GetComponent<TextMeshProUGUI>().text);

        backBtn.onClick.Invoke();
        Assert.True(!connectGamePanel.activeInHierarchy);
        Assert.True(defaultMenu.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator CreateGamePanelTest()
    {
        System.Random random = new System.Random();
        int randomUserID = random.Next(10000, 100000);
        yield return RegisterUser("Test" + randomUserID, "Test-" + randomUserID);
        yield return AwaitTask(menu.Menu_succesFullyRegistered());
        GameObject defaultMenu = GameObject.Find("DefaultPanel(Clone)");
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        TextMeshProUGUI loggedInText = GameObject.Find("LoggedInAs").GetComponent<TextMeshProUGUI>();
        Button createGameBtn = GameObject.Find("CreateGameBtn").GetComponent<Button>();
        Button quitGameBtn = GameObject.Find("QuitGameBtn").GetComponent<Button>();
        Button friendsBtn = GameObject.Find("FriendsBtn(Clone)").GetComponent<Button>();
        GameObject createGamePanel = GameObject.Find("CreateGamePanel(Clone)");

        Assert.True(createGameBtn.gameObject.activeInHierarchy, "Default menu button create game can be seen after login");
        Assert.True(createGameBtn.gameObject.activeInHierarchy, "Default menu button connect game can be seen after login");
        Assert.True(quitGameBtn.gameObject.activeInHierarchy, "Default menu button quit game can be seen after login");
        Assert.True(friendsBtn.gameObject.activeInHierarchy, "Default menu button friends can be seen after login");
        Assert.Null(createGamePanel);

        createGameBtn.onClick.Invoke();
        createGamePanel = GameObject.Find("CreateGamePanel(Clone)");
        Assert.True(createGamePanel.activeInHierarchy);
        GameObject playerNameText = createGamePanel.transform.Find("NamePanel").Find("PlayerNameText(Clone)").gameObject;
        GameObject gameIDText = createGamePanel.transform.Find("GameIDPanel").Find("GameIDInput").gameObject;
        GameObject deckSelector = createGamePanel.transform.Find("DeckSelector").gameObject;
        Button startGameBtn = createGamePanel.transform.Find("StartGameBtn").GetComponent<Button>();
        Button backBtn = GameObject.Find("BackBtn").GetComponent<Button>();

        Assert.True(playerNameText.activeInHierarchy);
        Assert.True(gameIDText.activeInHierarchy);
        Assert.True(deckSelector.activeInHierarchy);
        Assert.True(startGameBtn.gameObject.activeInHierarchy);
        Assert.True(backBtn.gameObject.activeInHierarchy);
        Assert.AreEqual("Test" + randomUserID, playerNameText.GetComponent<TextMeshProUGUI>().text);
        Assert.AreEqual(1, deckSelector.GetComponent<TMP_Dropdown>().options.Count);
        Assert.AreEqual("ST01-DefaultDeck", deckSelector.GetComponent<TMP_Dropdown>().transform.Find("Label").GetComponent<TextMeshProUGUI>().text);

        backBtn.onClick.Invoke();
        Assert.True(!createGamePanel.activeInHierarchy);
        Assert.True(defaultMenu.activeInHierarchy);
    }
    public IEnumerator SkipToDeckBuilderOpenWithRegister(int randomUserID)
    {
        yield return RegisterUser("Test" + randomUserID, "Test-" + randomUserID);
        GameObject defaultMenu = GameObject.Find("DefaultPanel(Clone)");
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        GameObject deckBuilderBtn = GameObject.Find("DeckBuilderBtn(Clone)");
        Assert.True(deckBuilderBtn.gameObject.activeInHierarchy, "Deck builder button is active after register");
        deckBuilderBtn.GetComponent<Button>().onClick.Invoke();
        DeckBuilder deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
        deckBuilderBtn = GameObject.Find("DeckBuilderBtn(Clone)");
        Assert.Null(deckBuilderBtn, "Deck builder button is inactive after clicking on it");
        Assert.NotNull(deckBuilder, "Deck builder opens after clicking on builder button");
    }
    public IEnumerator SkipToDeckBuilderOpenWithLogin(int randomUserID)
    {
        yield return LoginUser("Test" + randomUserID, "Test-" + randomUserID);
        GameObject defaultMenu = GameObject.Find("DefaultPanel(Clone)");
        yield return new WaitUntil(() =>
        {
            defaultMenu = GameObject.Find("DefaultPanel(Clone)");
            return defaultMenu != null && defaultMenu.activeInHierarchy;
        });
        GameObject deckBuilderBtn = GameObject.Find("DeckBuilderBtn(Clone)");
        Assert.True(deckBuilderBtn.gameObject.activeInHierarchy, "Deck builder button is active after register");
        deckBuilderBtn.GetComponent<Button>().onClick.Invoke();
        DeckBuilder deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
        deckBuilderBtn = GameObject.Find("DeckBuilderBtn(Clone)");
        Assert.Null(deckBuilderBtn, "Deck builder button is inactive after clicking on it");
        Assert.NotNull(deckBuilder, "Deck builder opens after clicking on builder button");
    }

    [UnityTest]
    public IEnumerator DeckBuilderDefaultTest()
    {
        System.Random random = new System.Random();
        int randomUserID = random.Next(10000, 100000);
        yield return SkipToDeckBuilderOpenWithRegister(randomUserID);
        DeckBuilder deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
        yield return new WaitUntil(() =>
        {
            deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
            return deckBuilder != null && deckBuilder.gameObject.activeInHierarchy;
        });
        yield return AwaitTask(deckBuilder.PopulateScrollViewWithLeaders());
        GameObject availableCards = deckBuilder.transform.Find("AvailableCards(Clone)").gameObject;
        GameObject selectedCards = deckBuilder.transform.Find("SelectedCards(Clone)").gameObject;
        GameObject selectedCounter = deckBuilder.transform.Find("CardInSelectedArea(Clone)").gameObject;
        GameObject saveDeckBtn = deckBuilder.transform.Find("SaveDeckBtn(Clone)").gameObject;
        GameObject decknameInput = deckBuilder.transform.Find("DeckNameInput(Clone)").gameObject;
        GameObject deleteDeckBtn = deckBuilder.transform.Find("DeleteDeckBtn(Clone)").gameObject;
        GameObject createNewDeckBtn = deckBuilder.transform.Find("CreateNewDeckBtn(Clone)").gameObject;
        GameObject userDeckDrop = deckBuilder.transform.Find("UserDecksDropDown(Clone)").gameObject;
        GameObject backToMenuBtn = deckBuilder.transform.Find("BackToMenu(Clone)").gameObject;
        GameObject selectLeaderText = deckBuilder.transform.Find("SelectLeaderText(Clone)").gameObject;
        GameObject selectCardsText = deckBuilder.transform.Find("SelectCardsText(Clone)").gameObject;
        Assert.True(availableCards.activeInHierarchy);
        Assert.True(selectedCards.activeInHierarchy);
        Assert.True(selectedCounter.activeInHierarchy);
        Assert.True(saveDeckBtn.activeInHierarchy);
        Assert.True(decknameInput.activeInHierarchy);
        Assert.True(deleteDeckBtn.activeInHierarchy);
        Assert.True(createNewDeckBtn.activeInHierarchy);
        Assert.True(userDeckDrop.activeInHierarchy);
        Assert.True(backToMenuBtn.activeInHierarchy);
        Assert.True(selectLeaderText.activeInHierarchy && selectLeaderText.GetComponent<TextMeshProUGUI>().text == "SELECT A LEADER");
        Assert.True(!selectCardsText.activeInHierarchy);
        yield return AwaitTask(deckBuilder.HandleUserDecks());
        Assert.AreEqual("0/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);
        Assert.AreEqual("Default-NoCards", userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text);
        Assert.AreEqual("​", decknameInput.transform.Find("Text Area").Find("Text").GetComponent<TextMeshProUGUI>().text);
        Assert.AreEqual("Enter text...", decknameInput.transform.Find("Text Area").Find("Placeholder").GetComponent<TextMeshProUGUI>().text);
        backToMenuBtn.GetComponent<Button>().onClick.Invoke();
        GameObject deckBuilderBtn = GameObject.Find("DeckBuilderBtn(Clone)");
        Assert.NotNull(deckBuilderBtn, "Deck builder button is active after clicking on bact to menu button");
        Assert.Null(GameObject.Find("DeckBuilder(Clone)"), "Deck builder closes after clicking on back to menu button");
        deckBuilderBtn.GetComponent<Button>().onClick.Invoke();
        yield return new WaitUntil(() =>
        {
            deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
            return deckBuilder != null && deckBuilder.gameObject.activeInHierarchy;
        });
        deckBuilderBtn = GameObject.Find("DeckBuilderBtn(Clone)");
        Assert.Null(deckBuilderBtn, "Deck builder button is inactive after clicking on it");
        Assert.NotNull(deckBuilder, "Deck builder opens after clicking on builder button");
    }

    [UnityTest]
    public IEnumerator DeckBuilderDropDownTest()
    {
        System.Random random = new System.Random();
        int randomUserID = random.Next(10000, 100000);
        yield return SkipToDeckBuilderOpenWithRegister(randomUserID);
        DeckBuilder deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
        yield return new WaitUntil(() =>
        {
            deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
            return deckBuilder != null && deckBuilder.gameObject.activeInHierarchy;
        });
        yield return AwaitTask(deckBuilder.HandleUserDecks());
        yield return AwaitTask(deckBuilder.PopulateScrollViewWithLeaders());
        GameObject availableCards = deckBuilder.transform.Find("AvailableCards(Clone)").gameObject;
        GameObject selectedCards = deckBuilder.transform.Find("SelectedCards(Clone)").gameObject;
        GameObject selectedCounter = deckBuilder.transform.Find("CardInSelectedArea(Clone)").gameObject;
        GameObject createNewDeckBtn = deckBuilder.transform.Find("CreateNewDeckBtn(Clone)").gameObject;
        GameObject backToMenuBtn = deckBuilder.transform.Find("BackToMenu(Clone)").gameObject;
        GameObject availableContent = availableCards.transform.Find("Viewport").Find("AvailableContent").gameObject;
        GameObject selectedContent = selectedCards.transform.Find("Viewport").Find("SelectedContent").gameObject;
        GameObject selectLeaderText = deckBuilder.transform.Find("SelectLeaderText(Clone)").gameObject;
        GameObject selectCardsText = deckBuilder.transform.Find("SelectCardsText(Clone)").gameObject;
        GameObject userDeckDrop = deckBuilder.transform.Find("UserDecksDropDown(Clone)").gameObject;
        Assert.AreEqual("Default-NoCards", userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text);
        Assert.True(userDeckDrop.activeInHierarchy);
        Assert.True(availableCards.activeInHierarchy);
        Assert.True(selectedCards.activeInHierarchy);
        Assert.True(createNewDeckBtn.activeInHierarchy);
        Assert.True(selectedCounter.activeInHierarchy);
        Assert.True(backToMenuBtn.activeInHierarchy);
        Assert.True(selectLeaderText.activeInHierarchy && selectLeaderText.GetComponent<TextMeshProUGUI>().text == "SELECT A LEADER");
        Assert.True(!selectCardsText.activeInHierarchy);
        Assert.AreEqual("0/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);
        Assert.True(availableContent.transform.childCount > 1, "At least default luffy and kid is shown in available cards");
        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(userDeckDrop.GetComponent<TMP_Dropdown>().gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => userDeckDrop.transform.Find("Dropdown List").gameObject != null);
        GameObject dropDownList = userDeckDrop.transform.Find("Dropdown List").gameObject;
        Assert.True(dropDownList.activeInHierarchy);
        GameObject dropDownListContent = dropDownList.transform.Find("Viewport").Find("Content").gameObject;
        Assert.AreEqual(3, dropDownListContent.transform.childCount);
        Assert.True(dropDownListContent.transform.GetChild(1).name.Contains("Default-NoCards"));
        Assert.True(dropDownListContent.transform.GetChild(2).name.Contains("ST01-DefaultDeck"));

        userDeckDrop.GetComponent<TMP_Dropdown>().value = 1;
        userDeckDrop.GetComponent<TMP_Dropdown>().RefreshShownValue();
        yield return null;
        yield return AwaitTask(deckBuilder.OnDropdownChanged(1));
        yield return null;
        yield return new WaitUntil(() => userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text == "ST01-DefaultDeck");
        yield return new WaitUntil(() => availableContent.transform.childCount>1);
        Assert.AreEqual("ST01-DefaultDeck", userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text);
        Assert.True(!selectLeaderText.activeInHierarchy);
        Assert.True(selectCardsText.activeInHierarchy);
        Assert.True(availableContent.transform.childCount > 5);
        Assert.AreEqual(51, selectedContent.transform.childCount);
        Assert.AreEqual("51/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);
        Assert.AreEqual("ST01-001", selectedContent.transform.GetChild(50).name);
        userDeckDrop.GetComponent<TMP_Dropdown>().value = 0;
        userDeckDrop.GetComponent<TMP_Dropdown>().RefreshShownValue();
        yield return null;
        yield return AwaitTask(deckBuilder.OnDropdownChanged(0));
        yield return null;
        yield return new WaitUntil(() => userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text == "Default-NoCards");
        yield return new WaitUntil(() => availableContent.transform.childCount > 1);
        Assert.AreEqual("Default-NoCards", userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text);
        Assert.True(selectLeaderText.activeInHierarchy);
        Assert.True(!selectCardsText.activeInHierarchy);
        Assert.True(availableContent.transform.childCount >= 2);
        Assert.AreEqual(0, selectedContent.transform.childCount);
        Assert.True(selectLeaderText.activeInHierarchy && selectLeaderText.GetComponent<TextMeshProUGUI>().text == "SELECT A LEADER");
        Assert.True(!selectCardsText.activeInHierarchy);
        Assert.AreEqual("0/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);
        Assert.True(availableContent.transform.childCount > 1, "At least default luffy and kid is shown in available cards");

    }

    [UnityTest]
    public IEnumerator DeckBuilderNewDeckTest()
    {
        System.Random random = new System.Random();
        int randomUserID = random.Next(10000, 100000);
        yield return SkipToDeckBuilderOpenWithRegister(randomUserID);
        DeckBuilder deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
        yield return new WaitUntil(() =>
        {
            deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
            return deckBuilder != null && deckBuilder.gameObject.activeInHierarchy;
        });
        yield return AwaitTask(deckBuilder.HandleUserDecks());
        yield return AwaitTask(deckBuilder.PopulateScrollViewWithLeaders());
        GameObject availableCards = deckBuilder.transform.Find("AvailableCards(Clone)").gameObject;
        GameObject selectedCards = deckBuilder.transform.Find("SelectedCards(Clone)").gameObject;
        GameObject selectedCounter = deckBuilder.transform.Find("CardInSelectedArea(Clone)").gameObject;
        GameObject backToMenuBtn = deckBuilder.transform.Find("BackToMenu(Clone)").gameObject;
        GameObject availableContent = availableCards.transform.Find("Viewport").Find("AvailableContent").gameObject;
        GameObject selectedContent = selectedCards.transform.Find("Viewport").Find("SelectedContent").gameObject;
        GameObject selectLeaderText = deckBuilder.transform.Find("SelectLeaderText(Clone)").gameObject;
        GameObject selectCardsText = deckBuilder.transform.Find("SelectCardsText(Clone)").gameObject;
        GameObject userDeckDrop = deckBuilder.transform.Find("UserDecksDropDown(Clone)").gameObject;
        GameObject createNewDeckBtn = deckBuilder.transform.Find("CreateNewDeckBtn(Clone)").gameObject;
        Assert.True(userDeckDrop.activeInHierarchy);
        Assert.True(createNewDeckBtn.activeInHierarchy);
        Assert.True(availableCards.activeInHierarchy);
        Assert.True(selectedCards.activeInHierarchy);
        Assert.True(selectedCounter.activeInHierarchy);
        Assert.True(backToMenuBtn.activeInHierarchy);
        Assert.True(selectLeaderText.activeInHierarchy && selectLeaderText.GetComponent<TextMeshProUGUI>().text == "SELECT A LEADER");
        Assert.True(!selectCardsText.activeInHierarchy);
        Assert.AreEqual("0/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);
        Assert.True(availableContent.transform.childCount > 1, "At least default luffy and kid is shown in available cards");
        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(userDeckDrop.GetComponent<TMP_Dropdown>().gameObject, pointer, ExecuteEvents.pointerClickHandler);
        yield return new WaitUntil(() => userDeckDrop.transform.Find("Dropdown List").gameObject != null);
        GameObject dropDownList = userDeckDrop.transform.Find("Dropdown List").gameObject;
        Assert.True(dropDownList.activeInHierarchy);
        GameObject dropDownListContent = dropDownList.transform.Find("Viewport").Find("Content").gameObject;
        Assert.AreEqual(3, dropDownListContent.transform.childCount);
        Assert.True(dropDownListContent.transform.GetChild(1).name.Contains("Default-NoCards"));
        Assert.True(dropDownListContent.transform.GetChild(2).name.Contains("ST01-DefaultDeck"));

        yield return new WaitUntil(() => userDeckDrop.GetComponent<TMP_Dropdown>().options.Count == 2);
        userDeckDrop.GetComponent<TMP_Dropdown>().value = 1;
        userDeckDrop.GetComponent<TMP_Dropdown>().RefreshShownValue();
        yield return null;
        yield return AwaitTask(deckBuilder.OnDropdownChanged(1));
        yield return null;
        yield return new WaitUntil(() => userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text == "ST01-DefaultDeck");
        Assert.AreEqual("ST01-DefaultDeck", userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text);
        Assert.True(!selectLeaderText.activeInHierarchy);
        Assert.True(selectCardsText.activeInHierarchy);
        Assert.True(availableContent.transform.childCount > 5);
        Assert.AreEqual(51, selectedContent.transform.childCount);
        Assert.AreEqual("ST01-001", selectedContent.transform.GetChild(50).name);

        yield return AwaitTask(deckBuilder.NewDeckCreation());
        Assert.AreEqual("New deck", userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text);
        Assert.True(selectLeaderText.activeInHierarchy);
        Assert.True(!selectCardsText.activeInHierarchy);
        Assert.True(availableContent.transform.childCount > 2);
        Assert.AreEqual(0, selectedContent.transform.childCount);
    }

    [UnityTest]
    public IEnumerator DeckBuilderSaveDeckTest()
    {
        System.Random random = new System.Random();
        int randomUserID = random.Next(10000, 100000);
        yield return SkipToDeckBuilderOpenWithRegister(randomUserID);
        DeckBuilder deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
        yield return new WaitUntil(() =>
        {
            deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
            return deckBuilder != null && deckBuilder.gameObject.activeInHierarchy;
        });
        yield return AwaitTask(deckBuilder.HandleUserDecks());
        yield return AwaitTask(deckBuilder.PopulateScrollViewWithLeaders());
        GameObject availableCards = deckBuilder.transform.Find("AvailableCards(Clone)").gameObject;
        GameObject selectedCards = deckBuilder.transform.Find("SelectedCards(Clone)").gameObject;
        GameObject selectedCounter = deckBuilder.transform.Find("CardInSelectedArea(Clone)").gameObject;
        GameObject backToMenuBtn = deckBuilder.transform.Find("BackToMenu(Clone)").gameObject;
        GameObject availableContent = availableCards.transform.Find("Viewport").Find("AvailableContent").gameObject;
        GameObject selectedContent = selectedCards.transform.Find("Viewport").Find("SelectedContent").gameObject;
        GameObject selectLeaderText = deckBuilder.transform.Find("SelectLeaderText(Clone)").gameObject;
        GameObject selectCardsText = deckBuilder.transform.Find("SelectCardsText(Clone)").gameObject;
        GameObject userDeckDrop = deckBuilder.transform.Find("UserDecksDropDown(Clone)").gameObject;
        GameObject saveDeckBtn = deckBuilder.transform.Find("SaveDeckBtn(Clone)").gameObject;
        GameObject createNewDeckBtn = deckBuilder.transform.Find("CreateNewDeckBtn(Clone)").gameObject;
        GameObject decknameInput = deckBuilder.transform.Find("DeckNameInput(Clone)").gameObject;
        Assert.True(userDeckDrop.activeInHierarchy);
        Assert.True(decknameInput.activeInHierarchy);
        Assert.True(createNewDeckBtn.activeInHierarchy);
        Assert.True(saveDeckBtn.activeInHierarchy);
        Assert.True(availableCards.activeInHierarchy);
        Assert.True(selectedCards.activeInHierarchy);
        Assert.True(selectedCounter.activeInHierarchy);
        Assert.True(backToMenuBtn.activeInHierarchy);
        Assert.True(selectLeaderText.activeInHierarchy && selectLeaderText.GetComponent<TextMeshProUGUI>().text == "SELECT A LEADER");
        Assert.True(!selectCardsText.activeInHierarchy);
        Assert.AreEqual("0/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);
        Assert.True(availableContent.transform.childCount > 1, "At least default luffy and kid is shown in available cards");
        Assert.AreEqual(2, userDeckDrop.GetComponent<TMP_Dropdown>().options.Count);
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[0].text.Contains("Default-NoCards"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[1].text.Contains("ST01-DefaultDeck"));

        yield return new WaitUntil(() => userDeckDrop.GetComponent<TMP_Dropdown>().options.Count == 2);
        userDeckDrop.GetComponent<TMP_Dropdown>().value = 1;
        userDeckDrop.GetComponent<TMP_Dropdown>().RefreshShownValue();
        yield return null;
        yield return AwaitTask(deckBuilder.OnDropdownChanged(1));
        yield return null;
        yield return new WaitUntil(() => userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text == "ST01-DefaultDeck");
        Assert.AreEqual("ST01-DefaultDeck", userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text);
        Assert.True(!selectLeaderText.activeInHierarchy);
        Assert.True(selectCardsText.activeInHierarchy);
        Assert.True(availableContent.transform.childCount > 5);
        Assert.AreEqual("51/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);
        Assert.AreEqual(51, selectedContent.transform.childCount);
        Assert.AreEqual("ST01-001", selectedContent.transform.GetChild(50).name);

        string cardName = selectedContent.transform.GetChild(0).name;
        CardData cardData = deckBuilder.GetSelectedCardsCardDataByID(cardName);
        deckBuilder.OnSelectedCardsClicked(selectedContent.transform.GetChild(0).GetComponent<Button>(), cardData);
        yield return new WaitUntil(() => selectedCounter.GetComponent<TextMeshProUGUI>().text == "50/51");
        Assert.AreEqual(50, selectedContent.transform.childCount);

        yield return AwaitTask(deckBuilder.SaveDeck());
        Assert.AreEqual(2, userDeckDrop.GetComponent<TMP_Dropdown>().options.Count);
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[0].text.Contains("Default-NoCards"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[1].text.Contains("ST01-DefaultDeck"));

        cardName = availableContent.transform.GetChild(0).name;
        yield return AwaitTaskWithReturn(deckBuilder.GetCardByCardID(cardName), result => cardData = result);
        deckBuilder.OnCardsClicked(availableContent.transform.GetChild(0).GetComponent<Button>(), cardData);
        yield return new WaitUntil(() => selectedCounter.GetComponent<TextMeshProUGUI>().text == "51/51");
        Assert.AreEqual(51, selectedContent.transform.childCount);

        yield return AwaitTask(deckBuilder.SaveDeck());
        yield return new WaitUntil(() => userDeckDrop.GetComponent<TMP_Dropdown>().options.Count == 3);
        Assert.AreEqual(3, userDeckDrop.GetComponent<TMP_Dropdown>().options.Count);
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[0].text.Contains("ST01-DefaultDeck1"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[1].text.Contains("Default-NoCards"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[2].text.Contains("ST01-DefaultDeck"));

        userDeckDrop.GetComponent<TMP_Dropdown>().value = 1;
        userDeckDrop.GetComponent<TMP_Dropdown>().RefreshShownValue();
        yield return AwaitTask(deckBuilder.OnDropdownChanged(1));
        Assert.AreEqual("Default-NoCards", userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text);
        Assert.True(selectLeaderText.activeInHierarchy);
        Assert.True(!selectCardsText.activeInHierarchy);
        Assert.True(availableContent.transform.childCount > 2);
        Assert.AreEqual(0, selectedContent.transform.childCount);
        yield return AwaitTask(deckBuilder.OnLeaderClicked(availableContent.transform.GetChild(0).GetComponent<Button>()));
        yield return AwaitTaskWithReturn(deckBuilder.GetCardByCardID(selectedContent.transform.GetChild(0).name), result => cardData = result);
        yield return AwaitTask(deckBuilder.PopulateScrollViewWithCardsWithSameColor(cardData));
        Assert.True(availableContent.transform.childCount > 0, "After selecting a leader the other leaders dissappear and cards with same color appears");
        Assert.AreEqual(1, selectedContent.transform.childCount, "There is a card in selected cards area after clicking on one");
        Assert.True(!selectLeaderText.activeInHierarchy, "After a leader card is clicked the select leader text should not be visible");
        Assert.True(selectedCards.activeInHierarchy, "After a leader card is clicke the select cards text should be visible");
        Assert.AreEqual("1/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);

        for (int i = 1; i < 13; i++)
        {
            cardName = availableContent.transform.GetChild(i).name;
            yield return AwaitTaskWithReturn(deckBuilder.GetCardByCardID(cardName), result => cardData = result);
            for (int j = 0; j < 4; j++)
            {
                deckBuilder.OnCardsClicked(availableContent.transform.GetChild(i).GetComponent<Button>(), cardData);
            }
            int cardCount = selectedContent.transform.childCount;
            deckBuilder.OnCardsClicked(availableContent.transform.GetChild(i).GetComponent<Button>(), cardData);
            Assert.AreEqual(cardCount, selectedContent.transform.childCount);
        }
        cardName = availableContent.transform.GetChild(0).name;
        yield return AwaitTaskWithReturn(deckBuilder.GetCardByCardID(cardName), result => cardData = result);
        deckBuilder.OnCardsClicked(availableContent.transform.GetChild(0).GetComponent<Button>(), cardData);
        deckBuilder.OnCardsClicked(availableContent.transform.GetChild(0).GetComponent<Button>(), cardData);

        Assert.AreEqual(51, selectedContent.transform.childCount);
        Assert.AreEqual("51/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);

        yield return AwaitTask(deckBuilder.SaveDeck());
        yield return new WaitUntil(() => userDeckDrop.GetComponent<TMP_Dropdown>().options.Count == 3);
        Assert.AreEqual(3, userDeckDrop.GetComponent<TMP_Dropdown>().options.Count);
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[0].text.Contains("ST01-DefaultDeck1"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[1].text.Contains("Default-NoCards"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[2].text.Contains("ST01-DefaultDeck"));

        decknameInput.GetComponent<TMP_InputField>().onEndEdit.Invoke("TestDeck");
        yield return AwaitTask(deckBuilder.SaveDeck());
        yield return new WaitUntil(() => userDeckDrop.GetComponent<TMP_Dropdown>().options.Count == 4);
        Assert.AreEqual(4, userDeckDrop.GetComponent<TMP_Dropdown>().options.Count);
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[0].text.Contains("TestDeck"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[1].text.Contains("ST01-DefaultDeck1"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[2].text.Contains("Default-NoCards"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[3].text.Contains("ST01-DefaultDeck"));

        yield return AwaitTask(deckBuilder.NewDeckCreation());
        Assert.AreEqual("New deck", userDeckDrop.transform.Find("Label").GetComponent<TextMeshProUGUI>().text);
        Assert.True(selectLeaderText.activeInHierarchy);
        Assert.True(!selectCardsText.activeInHierarchy);
        Assert.True(availableContent.transform.childCount > 2);
        Assert.AreEqual(0, selectedContent.transform.childCount);

        yield return AwaitTask(deckBuilder.OnLeaderClicked(availableContent.transform.GetChild(0).GetComponent<Button>()));
        yield return AwaitTaskWithReturn(deckBuilder.GetCardByCardID(selectedContent.transform.GetChild(0).name), result => cardData = result);
        yield return AwaitTask(deckBuilder.PopulateScrollViewWithCardsWithSameColor(cardData));
        Assert.True(availableContent.transform.childCount > 0, "After selecting a leader the other leaders dissappear and cards with same color appears");
        Assert.AreEqual(1, selectedContent.transform.childCount, "There is a card in selected cards area after clicking on one");
        Assert.True(!selectLeaderText.activeInHierarchy, "After a leader card is clicked the select leader text should not be visible");
        Assert.True(selectedCards.activeInHierarchy, "After a leader card is clicke the select cards text should be visible");
        Assert.AreEqual("1/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);

        for (int i = 1; i < 13; i++)
        {
            cardName = availableContent.transform.GetChild(i).name;
            yield return AwaitTaskWithReturn(deckBuilder.GetCardByCardID(cardName), result => cardData = result);
            for (int j = 0; j < 4; j++)
            {
                deckBuilder.OnCardsClicked(availableContent.transform.GetChild(i).GetComponent<Button>(), cardData);
            }
            int cardCount = selectedContent.transform.childCount;
            deckBuilder.OnCardsClicked(availableContent.transform.GetChild(i).GetComponent<Button>(), cardData);
            Assert.AreEqual(cardCount, selectedContent.transform.childCount);
        }
        cardName = availableContent.transform.GetChild(0).name;
        yield return AwaitTaskWithReturn(deckBuilder.GetCardByCardID(cardName), result => cardData = result);
        deckBuilder.OnCardsClicked(availableContent.transform.GetChild(0).GetComponent<Button>(), cardData);
        deckBuilder.OnCardsClicked(availableContent.transform.GetChild(0).GetComponent<Button>(), cardData);

        Assert.AreEqual(51, selectedContent.transform.childCount);
        Assert.AreEqual("51/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);

        decknameInput.GetComponent<TMP_InputField>().onEndEdit.Invoke("TestDeck");
        yield return AwaitTask(deckBuilder.SaveDeck());
        yield return new WaitUntil(() => userDeckDrop.GetComponent<TMP_Dropdown>().options.Count == 5);
        Assert.AreEqual(5, userDeckDrop.GetComponent<TMP_Dropdown>().options.Count);
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[0].text.Contains("TestDeck1"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[1].text.Contains("TestDeck"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[2].text.Contains("ST01-DefaultDeck1"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[3].text.Contains("Default-NoCards"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[4].text.Contains("ST01-DefaultDeck"));

        yield return SkipToDeckBuilderOpenWithLogin(randomUserID);
        deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
        yield return new WaitUntil(() =>
        {
            deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
            return deckBuilder != null && deckBuilder.gameObject.activeInHierarchy;
        });
        userDeckDrop = deckBuilder.transform.Find("UserDecksDropDown(Clone)").gameObject;
        yield return new WaitUntil(() => userDeckDrop.GetComponent<TMP_Dropdown>().options.Count == 5);
        Assert.AreEqual(5, userDeckDrop.GetComponent<TMP_Dropdown>().options.Count);
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[4].text.Contains("TestDeck1"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[3].text.Contains("TestDeck"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[2].text.Contains("ST01-DefaultDeck1"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[1].text.Contains("ST01-DefaultDeck"));
        Assert.True(userDeckDrop.GetComponent<TMP_Dropdown>().options[0].text.Contains("Default-NoCards"));
    }

    [UnityTest]
    public IEnumerator DeckBuilderLeaderSelectTest()
    {
        System.Random random = new System.Random();
        int randomUserID = random.Next(10000, 100000);
        yield return SkipToDeckBuilderOpenWithRegister(randomUserID);
        DeckBuilder deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
        yield return new WaitUntil(() =>
        {
            deckBuilder = GameObject.Find("DeckBuilder(Clone)").GetComponent<DeckBuilder>();
            return deckBuilder != null && deckBuilder.gameObject.activeInHierarchy;
        });
        yield return AwaitTask(deckBuilder.PopulateScrollViewWithLeaders());
        GameObject availableCards = deckBuilder.transform.Find("AvailableCards(Clone)").gameObject;
        GameObject selectedCards = deckBuilder.transform.Find("SelectedCards(Clone)").gameObject;
        GameObject selectedCounter = deckBuilder.transform.Find("CardInSelectedArea(Clone)").gameObject;
        GameObject backToMenuBtn = deckBuilder.transform.Find("BackToMenu(Clone)").gameObject;
        GameObject availableContent = availableCards.transform.Find("Viewport").Find("AvailableContent").gameObject;
        GameObject selectedContent = selectedCards.transform.Find("Viewport").Find("SelectedContent").gameObject;
        GameObject selectLeaderText = deckBuilder.transform.Find("SelectLeaderText(Clone)").gameObject;
        GameObject selectCardsText = deckBuilder.transform.Find("SelectCardsText(Clone)").gameObject;
        Assert.True(availableCards.activeInHierarchy);
        Assert.True(selectedCards.activeInHierarchy);
        Assert.True(selectedCounter.activeInHierarchy);
        Assert.True(backToMenuBtn.activeInHierarchy);
        Assert.True(selectLeaderText.activeInHierarchy && selectLeaderText.GetComponent<TextMeshProUGUI>().text == "SELECT A LEADER");
        Assert.True(!selectCardsText.activeInHierarchy);
        Assert.AreEqual("0/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);
        Assert.True(availableContent.transform.childCount > 1, "At least default luffy and kid is shown in available cards");
        Assert.AreEqual(0, selectedContent.transform.childCount, "There is no selected cards at first");
        yield return AwaitTask(deckBuilder.OnLeaderClicked(availableContent.transform.GetChild(0).GetComponent<Button>()));
        Assert.True(availableContent.transform.childCount > 0, "After selecting a leader the other leaders dissappear and cards with same color appears");
        Assert.AreEqual(1, selectedContent.transform.childCount, "There is a card in selected cards area after clicking on one");
        Assert.True(!selectLeaderText.activeInHierarchy, "After a leader card is clicked the select leader text should not be visible");
        Assert.True(selectedCards.activeInHierarchy, "After a leader card is clicke the select cards text should be visible");
        Assert.AreEqual("1/51", selectedCounter.GetComponent<TextMeshProUGUI>().text);
    }
}
