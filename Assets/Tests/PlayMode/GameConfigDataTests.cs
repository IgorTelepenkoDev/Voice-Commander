using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GameConfigDataTests
{
    private const string gameConfigObjectTag = "Game Configurator";

    [OneTimeSetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("Main Scene");
    }

    [UnityTest]
    public IEnumerator Config_data_provider_is_available()
    {
        yield return null;

        var configDataProvider = GameObject.FindGameObjectWithTag(gameConfigObjectTag);
        if(configDataProvider == null)
        {
            Assert.Fail();
        }

        var configDataReceiver = configDataProvider.GetComponent<GameConfigDataReceiver>();
        if(configDataReceiver == null)
        {
            Assert.Fail();
        }

        Assert.Pass();
    }
}
