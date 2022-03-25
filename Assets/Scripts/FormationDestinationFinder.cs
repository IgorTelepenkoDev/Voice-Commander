using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FormationDestinationFinder : MonoBehaviour
{

    private string battlefieldMapTag;
    private string sectorWaypointTag;
    private string formationUnitsLayer;

    private GameObject configDataProvider;
    private const string gameConfigObjectTag = "Game Configurator";
    private const int limitOfForeignUnitsDetectedInFormationDestination = 150;

    void Start()
    {
        configDataProvider = GameObject.FindGameObjectWithTag(gameConfigObjectTag);
        var configDataReceiver = configDataProvider.GetComponent<GameConfigDataReceiver>();

        battlefieldMapTag = configDataReceiver.BattlefieldMapTag;
        sectorWaypointTag = configDataReceiver.MapSectorWaypointTag;
        formationUnitsLayer = configDataReceiver.FormationUnitsLayer;
    }

    void Update()
    {
        //Debug.Log("is the west test position free: " + IsPositionFreeForFormationDestination(new Vector2(-7.2f, -1.6f)));
    }

    public bool IsPositionFreeForFormationDestination(Vector2 checkedPosition)
    {
        var allForeignUnitsDetected = GetForeignObjectsOnFormationLocationPosition(checkedPosition, formationUnitsLayer);

        int unitsStoppedInCheckedPosition = 0;
        foreach (var soldierUnit in allForeignUnitsDetected)
        {
            if(soldierUnit.GetComponent<UnitController>().IsMoving == false)
            {
                unitsStoppedInCheckedPosition++;
            }
        }

        if(unitsStoppedInCheckedPosition == 0)
        {
            return true;
        }
        else return false;
    }

    private List<GameObject> GetForeignObjectsOnFormationLocationPosition(Vector2 locationPosition, string foreignObjectsLayer)
    {
        GameObject formationArea = gameObject.GetComponent<FormationController>().GetFormationAreaObj();
        Vector2 checkedAreaSize = new Vector2(formationArea.GetComponent<SpriteRenderer>().bounds.size.x, formationArea.GetComponent<SpriteRenderer>().bounds.size.y);

        Collider2D[] foreignObjectsColliders = new Collider2D[limitOfForeignUnitsDetectedInFormationDestination];
        ContactFilter2D foreignLayerContactFilter = new ContactFilter2D();
        foreignLayerContactFilter.SetLayerMask(LayerMask.GetMask(foreignObjectsLayer));

        Physics2D.OverlapBox(locationPosition, checkedAreaSize, 0f, foreignLayerContactFilter, foreignObjectsColliders);    // detects the foreign units (colliders)

        List<GameObject> foreignObjectsDetected = new List<GameObject>();
        foreach(var foreignObjectCollider in foreignObjectsColliders)
        {
            if(foreignObjectCollider != null)
            {
                foreignObjectsDetected.Add(foreignObjectCollider.gameObject);
            }
        }

        return foreignObjectsDetected;
    }

    internal GameObject GetClosestSectorWaypoint(GameObject[] mapSectorWaypoints)
    {
        if (mapSectorWaypoints == null)
        {
            return null;
        }

        var closestWaypoint = mapSectorWaypoints.
            OrderBy(waypoint => Vector2.Distance(gameObject.transform.position, waypoint.transform.position)).FirstOrDefault();

        return closestWaypoint;
    }

    internal GameObject[] GetSectorWaypoints(int sectorLineIndex, int sectorIndex)
    {
        var battlefieldMap = GameObject.FindGameObjectWithTag(battlefieldMapTag);
        var mapSectors = battlefieldMap.GetComponent<MapController>().BattlefieldSectors;

        GameObject targetSector;

        try
        {
            targetSector = mapSectors[sectorLineIndex][sectorIndex];
        }
        catch (IndexOutOfRangeException)
        {

            Debug.LogError("Wrong front line or sector index - out of range");
            return null;
        }

        List<GameObject> targetWaypoints = new List<GameObject>();

        foreach (Transform childSectorObj in targetSector.transform)
        {
            if (childSectorObj.gameObject.tag == sectorWaypointTag)
            {
                targetWaypoints.Add(childSectorObj.gameObject);
            }
        }

        return targetWaypoints.ToArray();
    }
}
