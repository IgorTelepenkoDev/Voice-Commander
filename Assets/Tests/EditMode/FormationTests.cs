using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FormationTests
{
    private readonly List<Tuple<int, int>> allFormationProportions = new List<Tuple<int, int>>()
    {
        Tuple.Create(3,5),
        Tuple.Create (3,4),
        Tuple.Create (2,3)
    };

    [Test]
    public void Available_proportions_proper()
    {
        var formationConfigDataProvider = new GameObject();
        formationConfigDataProvider.AddComponent<GameConfigDataReceiver>();
        var formationConfigDataReceiver = formationConfigDataProvider.GetComponent<GameConfigDataReceiver>();

        var availableProportions = formationConfigDataReceiver.GetAvailableFormationProportions();

        CollectionAssert.AreEqual(allFormationProportions, availableProportions);
    }
}
