using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEditor;

public class FormationTests
{
    private const string gameConfigObjectTag = "Game Configurator";

    [OneTimeSetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("Main Scene");
    }

    [UnityTest]
    public IEnumerator Formation_proportions_chosen_properly()
    {
        var formationConfigDataProvider = GameObject.FindGameObjectWithTag(gameConfigObjectTag);
        var formationConfigDataReceiver = formationConfigDataProvider.GetComponent<GameConfigDataReceiver>();

        var allAvailableProportions = formationConfigDataReceiver.GetAvailableFormationProportions();

        var formationGameObject = GameObject.Instantiate(GetFormationPrefab(), new Vector3(0, 0, 0), Quaternion.identity);
        var formationController = formationGameObject.GetComponent<FormationController>();

        yield return null;  // for Start() to be called in formationController

        int testedProportionCoefficient;
        int testedNumberUnits;

        foreach(Tuple<int, int> proportion in allAvailableProportions)
        {
            testedProportionCoefficient = 1;
            testedNumberUnits = proportion.Item1 * testedProportionCoefficient * proportion.Item2 * testedProportionCoefficient;

            var formationUnits = new List<GameObject>();
            for (int soldierUnitIndex = 0; soldierUnitIndex < testedNumberUnits; soldierUnitIndex++)
            {
                GameObject soldierUnit = GameObject.Instantiate(GetSoldierUnitPrefab(), formationGameObject.transform.position, formationGameObject.transform.rotation);
                formationUnits.Add(soldierUnit);
            }
            formationController.FormationSoldierUnits = formationUnits;

            Assert.AreEqual(testedProportionCoefficient, formationController.RowsInProportionCoefficient);
            Assert.AreEqual(proportion, formationController.CurrentFormationProportion);
        }

        foreach (Tuple<int, int> proportion in allAvailableProportions)
        {
            testedProportionCoefficient = 2;
            testedNumberUnits = proportion.Item1 * testedProportionCoefficient * proportion.Item2 * testedProportionCoefficient;

            var formationUnits = new List<GameObject>();
            for (int soldierUnitIndex = 0; soldierUnitIndex < testedNumberUnits; soldierUnitIndex++)
            {
                GameObject soldierUnit = GameObject.Instantiate(GetSoldierUnitPrefab(), formationGameObject.transform.position, formationGameObject.transform.rotation);
                formationUnits.Add(soldierUnit);
            }
            formationController.FormationSoldierUnits = formationUnits;

            Assert.AreEqual(testedProportionCoefficient, formationController.RowsInProportionCoefficient);
            Assert.AreEqual(proportion, formationController.CurrentFormationProportion);
        }

        Assert.Pass();
        GameObject.Destroy(formationGameObject);
    }

    public static GameObject GetFormationPrefab()
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/formation.prefab");
    }

    public static GameObject GetSoldierUnitPrefab()
    {
        return AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/soldier unit.prefab");
    }
}
