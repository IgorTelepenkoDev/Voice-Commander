using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class FormationMoveTests
{
    [OneTimeSetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("Main Scene");
    }

    [UnityTest]
    public IEnumerator Formation_units_state_changed_in_movement()
    {
        GameObject testFormation = GameObject.Instantiate(FormationTests.GetFormationPrefab(), new Vector3(0, 0, 0), Quaternion.identity);
        
        var formationUnits = new List<GameObject>();
        formationUnits.Add(GameObject.Instantiate(FormationTests.GetSoldierUnitPrefab(), testFormation.transform.position, testFormation.transform.rotation));
        formationUnits.Add(GameObject.Instantiate(FormationTests.GetSoldierUnitPrefab(), testFormation.transform.position, testFormation.transform.rotation));
        formationUnits.Add(GameObject.Instantiate(FormationTests.GetSoldierUnitPrefab(), testFormation.transform.position, testFormation.transform.rotation));

        yield return null;

        testFormation.GetComponent<FormationController>().FormationSoldierUnits = formationUnits;

        foreach (var soldierUnit in testFormation.GetComponent<FormationController>().FormationSoldierUnits)
        {
            Assert.AreEqual(false, soldierUnit.GetComponent<UnitController>().IsMoving);
        }

        int testDestinationSectorLineIndex = 3;
        int testDestinationSectorIndex = 0;

        testFormation.GetComponent<FormationMover>().MoveToSector(testDestinationSectorLineIndex, testDestinationSectorIndex);

        foreach (var soldierUnit in testFormation.GetComponent<FormationController>().FormationSoldierUnits)
        {
            Assert.AreEqual(true, soldierUnit.GetComponent<UnitController>().IsMoving);
        }

        testFormation.GetComponent<FormationMover>().StopAllMovement();

        foreach (var soldierUnit in testFormation.GetComponent<FormationController>().FormationSoldierUnits)
        {
            Assert.AreEqual(false, soldierUnit.GetComponent<UnitController>().IsMoving);
        }


        GameObject.Destroy(testFormation);
        foreach (var soldierUnit in formationUnits)
        {
            GameObject.Destroy(soldierUnit);
        }
    }

    [UnityTest]
    public IEnumerator Is_destination_position_available()
    {
        GameObject testMovingFormation = GameObject.Instantiate(FormationTests.GetFormationPrefab(), new Vector3(0, 0, 0), Quaternion.identity);
        yield return null;
        testMovingFormation.GetComponent<FormationController>().FormationSoldierUnits = new List<GameObject> { GameObject.Instantiate(FormationTests.GetSoldierUnitPrefab(), testMovingFormation.transform.position, testMovingFormation.transform.rotation) };
        yield return null;

        Vector2 checkedDestinationPosition = new Vector2(-8.0f, 4.4f);
        // The position should be free from any formation
        Assert.AreEqual(true, testMovingFormation.GetComponent<FormationDestinationFinder>().IsPositionFreeForFormationDestination(checkedDestinationPosition));

        GameObject formationlocatedInDestination = GameObject.Instantiate(FormationTests.GetFormationPrefab(), checkedDestinationPosition, Quaternion.identity);
        yield return null;
        formationlocatedInDestination.GetComponent<FormationController>().FormationSoldierUnits = new List<GameObject> { GameObject.Instantiate(FormationTests.GetSoldierUnitPrefab(), formationlocatedInDestination.transform.position, formationlocatedInDestination.transform.rotation) };
        yield return null;
        // The position is occupied by a formation
        Assert.AreEqual(false, testMovingFormation.GetComponent<FormationDestinationFinder>().IsPositionFreeForFormationDestination(checkedDestinationPosition));

        foreach (var soldierUnit in formationlocatedInDestination.GetComponent<FormationController>().FormationSoldierUnits)
        {
            soldierUnit.GetComponent<UnitController>().IsMoving = true;
        }
        // The checked position is occupied by a formation that is moving
        Assert.AreEqual(true, testMovingFormation.GetComponent<FormationDestinationFinder>().IsPositionFreeForFormationDestination(checkedDestinationPosition));

        GameObject.Destroy(formationlocatedInDestination);
        GameObject.Destroy(testMovingFormation);
    }
}
