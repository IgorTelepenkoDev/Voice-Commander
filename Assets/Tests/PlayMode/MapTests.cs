using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MapTests
{
    private const string gameConfigObjectTag = "Game Configurator";

    private int defenceSectors = 1;
    private int thirdLineSectors = 5;
    private int secondLineSectors = 5;
    private int firstLineSectors = 5;

    private int defenceSectorWaypoints = 3;
    private int thirdLineSectorWaypoints = 1;
    private int secondLineSectorWaypoints = 1;
    private int firstLineSectorWaypoints = 1;

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

        Assert.AreEqual(defenceSectors, mapController.DefenceSectors.Count);
        Assert.AreEqual(thirdLineSectors, mapController.ThirdFrontSectors.Count);
        Assert.AreEqual(secondLineSectors, mapController.SecondFrontSectors.Count);
        Assert.AreEqual(firstLineSectors, mapController.FirstFrontSectors.Count);
    }

    [UnityTest]
    public IEnumerator Get_map_sector_waypoints()
    {
        GameObject testFormation = GameObject.Instantiate(FormationTests.GetFormationPrefab(), new Vector3(0, 0, 0), Quaternion.identity);
        var mapDestinationFinder = testFormation.GetComponent<FormationDestinationFinder>();

        yield return null;

        int defenceLineIndex = 0;
        int defenceSectorIndex = 0;
        var foundDefenceSectorWaypoints = mapDestinationFinder.GetSectorWaypoints(defenceLineIndex, defenceSectorIndex);
        Assert.AreEqual(defenceSectorWaypoints, foundDefenceSectorWaypoints.Length);

        int thirdLineIndex = 1;
        for(int thirdLineSectorIndex = 0; thirdLineSectorIndex < thirdLineSectors; thirdLineSectorIndex++)
        {
            var foundThirdLineSectorWaypoints = mapDestinationFinder.GetSectorWaypoints(thirdLineIndex, thirdLineSectorIndex);
            Assert.AreEqual(thirdLineSectorWaypoints, foundThirdLineSectorWaypoints.Length);
        }

        int secondLineIndex = 2;
        for (int secondLineSectorIndex = 0; secondLineSectorIndex < secondLineSectors; secondLineSectorIndex++)
        {
            var foundSecondLineSectorWaypoints = mapDestinationFinder.GetSectorWaypoints(secondLineIndex, secondLineSectorIndex);
            Assert.AreEqual(secondLineSectorWaypoints, foundSecondLineSectorWaypoints.Length);
        }

        int firstLineIndex = 3;
        for (int firstLineSectorIndex = 0; firstLineSectorIndex < firstLineSectors; firstLineSectorIndex++)
        {
            var foundFirstLineSectorWaypoints = mapDestinationFinder.GetSectorWaypoints(firstLineIndex, firstLineSectorIndex);
            Assert.AreEqual(firstLineSectorWaypoints, foundFirstLineSectorWaypoints.Length);
        }

        GameObject.Destroy(testFormation);
    }
}
