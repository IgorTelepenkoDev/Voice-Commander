using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MapTests
{
    [OneTimeSetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("Main Scene");
    }

    [UnityTest]
    public IEnumerator Map_sectors_are_pre_defined()
    {
        yield return null;

        var battlefieldMap = GameObject.FindGameObjectWithTag("Map");
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
