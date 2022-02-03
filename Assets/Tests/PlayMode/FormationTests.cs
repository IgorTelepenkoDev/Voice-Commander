using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class FormationTests
{
    [OneTimeSetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("Main Scene");
    }

    [UnityTest]
    public IEnumerator Formation_proportions_chosen_properly()
    {
        var formationConfigDataProvider = new GameObject();
        formationConfigDataProvider.AddComponent<GameConfigDataReceiver>();
        var formationConfigDataReceiver = formationConfigDataProvider.GetComponent<GameConfigDataReceiver>();

        var allAvailableProportions = formationConfigDataReceiver.GetAvailableFormationProportions();

        var formationGameObject = new GameObject();
        formationGameObject.AddComponent<FormationController>();
        var formationController = formationGameObject.GetComponent<FormationController>();

        yield return null;  // for Start() to be called in formationController

        int testedProportionCoefficient;
        int testedNumberUnits;

        foreach(Tuple<int, int> proportion in allAvailableProportions)
        {
            testedProportionCoefficient = 1;
            testedNumberUnits = proportion.Item1 * testedProportionCoefficient * proportion.Item2 * testedProportionCoefficient;

            formationController.FormationUnits = testedNumberUnits;
            
            Assert.AreEqual(testedProportionCoefficient, formationController.RowsInProportionCoefficient);
            Assert.AreEqual(proportion, formationController.CurrentFormationProportion);
        }

        foreach (Tuple<int, int> proportion in allAvailableProportions)
        {
            testedProportionCoefficient = 2;
            testedNumberUnits = proportion.Item1 * testedProportionCoefficient * proportion.Item2 * testedProportionCoefficient;

            formationController.FormationUnits = testedNumberUnits;

            Assert.AreEqual(testedProportionCoefficient, formationController.RowsInProportionCoefficient);
            Assert.AreEqual(proportion, formationController.CurrentFormationProportion);
        }

        Assert.Pass();
    }
}
