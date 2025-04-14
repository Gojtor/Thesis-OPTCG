using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.ServerCon;
using NUnit.Framework;
using TCGSim;
using TCGSim.CardResources;
using TCGSim.CardScripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.TestTools;

public class MenuTests
{
    Menu menu;
    GameManager gameManager;
    LoginManager loginManager;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);

        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Menu");

    }


    [UnityTest]
    public IEnumerator MenuAfterLoadSceneTest()
    {
        yield return null;

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


    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(menu);
        Object.Destroy(gameManager);
        Object.Destroy(loginManager);
        yield return null;
    }
}
