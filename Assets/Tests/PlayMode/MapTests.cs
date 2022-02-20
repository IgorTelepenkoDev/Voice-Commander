using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MapTests
{
    private const string gameConfigObjectTag = "Game Configurator";

    [OneTimeSetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("Main Scene");
    }

    [UnityTest]
    public IEnumerator Map_sectors_are_pre_defined()
    {
        yield return null;

        var configDataProvider = GameObject.FindGameObjectWithTag(gameConfigObjectTag);
        string battlefieldMapTag = configDataProvider.GetComponent<GameConfigDataReceiver>().BattlefieldMapTag;

        var battlefieldMap = GameObject.FindGameObjectWithTag(battlefieldMapTag);
        var mapController = battlefieldMap.GetComponent<MapController>();

        if (mapController.DefenceSectors == null || mapController.ThirdFrontSectors == null ||
            mapController.SecondFrontSectors == null || mapController.FirstFrontSectors == null)
        {
            Assert.Fail("The battlefield sectors are not defined in Editor");
        }

        int defenceSectors = 1;
        int thirdLineSectors = 5;
        int secondLineSectors = 5;
        int firstLineSectors = 5;

        Assert.AreEqual(defenceSectors, mapController.DefenceSectors.Count);
        Assert.AreEqual(thirdLineSectors, mapController.ThirdFrontSectors.Count);
        Assert.AreEqual(secondLineSectors, mapController.SecondFrontSectors.Count);
        Assert.AreEqual(firstLineSectors, mapController.FirstFrontSectors.Count);
    }
}
