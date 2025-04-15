using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerCon;
using NUnit.Framework;
using TCGSim;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class MenuTests
{
    Menu menu;
    GameManager gameManager;
    LoginManager loginManager;


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
        Assert.NotNull(menu, "GameManager script component can be found in GameManager object!");

        GameObject loginManagerObj = GameObject.Find("LoginManager");
        Assert.NotNull(loginManagerObj, "LoginManager object is loaded after the scene is loaded");
        loginManager = loginManagerObj.GetComponent<LoginManager>();
        Assert.NotNull(menu, "LoginManager script component can be found in LoginManager object!");
    }

    [UnityTest]
    public IEnumerator RegisterTest()
    {
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
        loginFailed = GameObject.Find("LoginFailed(Clone)");
        Assert.NotNull(loginFailed, "Login failed panel is active after the login button is clicked without data in inputs or with wrong datas");
        GameObject loginFailBackBtn = GameObject.Find("Back");
        Assert.NotNull(loginFailBackBtn, "Back button can be seen on login failed panel");


    }


    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(menu);
        Object.Destroy(gameManager);
        Object.Destroy(loginManager);
        yield return null;
    }
}
