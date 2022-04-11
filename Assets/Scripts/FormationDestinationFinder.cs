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

    private float distanceBetweenFormationsInSector;   // in front/behind
    private float intervalBetweenFormationsInSector;  // from left/right


    void Start()
    {
        configDataProvider = GameObject.FindGameObjectWithTag(gameConfigObjectTag);
        var configDataReceiver = configDataProvider.GetComponent<GameConfigDataReceiver>();

        battlefieldMapTag = configDataReceiver.BattlefieldMapTag;
        sectorWaypointTag = configDataReceiver.MapSectorWaypointTag;
        formationUnitsLayer = configDataReceiver.FormationUnitsLayer;

        distanceBetweenFormationsInSector = configDataReceiver.GetDistanceBetweenFormations();
        intervalBetweenFormationsInSector = configDataReceiver.GetIntervalBetweenFormations();
    }

    public Vector2 GetProperWayDestination(GameObject destinationSector)
    {
        var sectorWaypoint = GetClosestSectorWaypoint(GetSectorWaypoints(destinationSector));

        if (IsPositionFreeForFormationDestination(sectorWaypoint.transform.position))
        {
            return sectorWaypoint.transform.position;
        }
        else
        {
            var locatedFormationsInSector = destinationSector.GetComponent<MapSectorController>().LocatedFormations;
            if(locatedFormationsInSector == null || locatedFormationsInSector.Count == 0)
            {
                var availablePositions = new List<Vector2>();

                float checkedWidthIntervalInSectorFromWaypoint = gameObject.GetComponent<FormationController>().GetFormationAreaObj().GetComponent<SpriteRenderer>().bounds.size.x * 1.1f;
                float checkedHeightIntervalInSectorFromWaypoint = gameObject.GetComponent<FormationController>().GetFormationAreaObj().GetComponent<SpriteRenderer>().bounds.size.y * 0.7f;

                var positionBehindSectorWaypoint = GetPositionForFormationBehindPoint(sectorWaypoint.transform.position, checkedHeightIntervalInSectorFromWaypoint);
                if (IsPositionFreeForFormationDestination(positionBehindSectorWaypoint))
                {
                    availablePositions.Add(positionBehindSectorWaypoint);
                }

                var positionLeftwardSectorWaypoint = GetPositionForFormationLeftwardPoint(sectorWaypoint.transform.position, checkedWidthIntervalInSectorFromWaypoint);
                if (IsPositionFreeForFormationDestination(positionLeftwardSectorWaypoint))
                {
                    availablePositions.Add(positionLeftwardSectorWaypoint);
                }

                var positionRightwardSectorWaypoint = GetPositionForFormationRightwardPoint(sectorWaypoint.transform.position, checkedWidthIntervalInSectorFromWaypoint);
                if (IsPositionFreeForFormationDestination(positionRightwardSectorWaypoint))
                {
                    availablePositions.Add(positionRightwardSectorWaypoint);
                }


                if(availablePositions.Count == 0)
                {
                    return sectorWaypoint.transform.position;
                }
                else
                {
                    var closestAvailablePosition = availablePositions.OrderBy(position => Vector2.Distance(gameObject.transform.position, position)).FirstOrDefault();
                    return closestAvailablePosition;
                }
            }
            else
            {
                var availablePositions = new List<Vector2>();

                foreach (var locatedFormation in locatedFormationsInSector)
                {
                    var currentFormationSize = gameObject.GetComponent<FormationController>().GetFormationAreaObj().GetComponent<SpriteRenderer>().bounds.size;
                    
                    GameObject locatedFormationArea = locatedFormation.GetComponent<FormationController>().GetFormationAreaObj();
                    var locatedFormationWidth = locatedFormationArea.GetComponent<SpriteRenderer>().bounds.size.x;
                    var locatedFormationHeight = locatedFormationArea.GetComponent<SpriteRenderer>().bounds.size.y;

                    var locatedFormationBottomPoint = new Vector2(locatedFormation.transform.position.x, locatedFormation.transform.position.y - locatedFormationHeight / 2);
                    var positionBehindLocatedFromation = GetPositionForFormationBehindPoint(locatedFormationBottomPoint, distanceBetweenFormationsInSector);
                    if(IsPositionFreeForFormationDestination(positionBehindLocatedFromation))
                    {
                        availablePositions.Add(positionBehindLocatedFromation);
                    }

                    var locatedFormationTopPoint = new Vector2(locatedFormation.transform.position.x, locatedFormation.transform.position.y + locatedFormationHeight / 2);
                    var positionInFrontOfLocatedFormation = GetPositionForFormationInFrontOfPoint(locatedFormationTopPoint, distanceBetweenFormationsInSector);
                    if(IsPositionFreeForFormationDestination(positionInFrontOfLocatedFormation))
                    {
                        availablePositions.Add(positionInFrontOfLocatedFormation);
                    }

                    var locatedFormationLeftPoint = new Vector2(locatedFormation.transform.position.x - locatedFormationWidth / 2, locatedFormation.transform.position.y);
                    var positionLefwardLocatedFormation = GetPositionForFormationLeftwardPoint(locatedFormationLeftPoint, intervalBetweenFormationsInSector);
                    if(IsPositionFreeForFormationDestination(positionLefwardLocatedFormation))
                    {
                        availablePositions.Add(positionLefwardLocatedFormation);
                    }

                    var locatedFormationRightPoint = new Vector2(locatedFormation.transform.position.x + locatedFormationWidth / 2, locatedFormation.transform.position.y);
                    var positionRightwardLocatedFormation = GetPositionForFormationRightwardPoint(locatedFormationRightPoint, intervalBetweenFormationsInSector);
                    if(IsPositionFreeForFormationDestination(positionRightwardLocatedFormation))
                    {
                        availablePositions.Add(positionRightwardLocatedFormation);
                    }
                }

                if(availablePositions.Count == 0)
                {
                    return sectorWaypoint.transform.position;
                }
                else
                {
                    availablePositions = availablePositions.OrderBy(position => Vector2.Distance(gameObject.transform.position, position)).ToList();    //The closest positions first
                    availablePositions = availablePositions.OrderByDescending(position => IsPositionWithinSector(position, destinationSector)).ToList();    //Positions that are within the sector first

                    return availablePositions.FirstOrDefault();
                }
            }
        }
    }

    private Vector2 GetPositionForFormationBehindPoint(Vector2 pointPosition, float gapSize = 0)
    {
        var formationArea = gameObject.GetComponent<FormationController>().GetFormationAreaObj();
        var formationSize = formationArea.GetComponent<SpriteRenderer>().bounds.size;

        var positionForFormationBehindPoint = new Vector2(pointPosition.x, pointPosition.y - gapSize - formationSize.y / 2);
        return positionForFormationBehindPoint;
    }

    private Vector2 GetPositionForFormationInFrontOfPoint(Vector2 pointPosition, float gapSize = 0)
    {
        var formationArea = gameObject.GetComponent<FormationController>().GetFormationAreaObj();
        var formationSize = formationArea.GetComponent<SpriteRenderer>().bounds.size;

        var positionForFormationInFrontOfPoint = new Vector2(pointPosition.x, pointPosition.y + gapSize + formationSize.y / 2);
        return positionForFormationInFrontOfPoint;
    }

    private Vector2 GetPositionForFormationLeftwardPoint(Vector2 pointPosition, float gapSize = 0)
    {
        var formationArea = gameObject.GetComponent<FormationController>().GetFormationAreaObj();
        var formationSize = formationArea.GetComponent<SpriteRenderer>().bounds.size;

        var positionForFormationLeftwardPoint = new Vector2(pointPosition.x - gapSize - formationSize.x / 2, pointPosition.y);
        return positionForFormationLeftwardPoint;
    }

    private Vector2 GetPositionForFormationRightwardPoint(Vector2 pointPosition, float gapSize = 0)
    {
        var formationArea = gameObject.GetComponent<FormationController>().GetFormationAreaObj();
        var formationSize = formationArea.GetComponent<SpriteRenderer>().bounds.size;

        var positionForFormationRightwardPoint = new Vector2(pointPosition.x + gapSize + formationSize.x / 2, pointPosition.y);
        return positionForFormationRightwardPoint;
    }

    private bool IsPositionWithinSector(Vector2 checkedPosition, GameObject mapSector)
    {
        Collider2D sectorCollider = mapSector.GetComponent<Collider2D>();

        if(sectorCollider.bounds.Contains(checkedPosition))
        {
            return true;
        }
        else
        {
            return false;
        }
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

    public GameObject GetClosestSectorWaypoint(GameObject[] mapSectorWaypoints)
    {
        if (mapSectorWaypoints == null)
        {
            return null;
        }

        var closestWaypoint = mapSectorWaypoints.
            OrderBy(waypoint => Vector2.Distance(gameObject.transform.position, waypoint.transform.position)).FirstOrDefault();

        return closestWaypoint;
    }

    public GameObject[] GetSectorWaypoints(GameObject targetSector)
    {
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

    public GameObject GetMapSector(int sectorLineIndex, int sectorIndex)
    {
        var battlefieldMap = GameObject.FindGameObjectWithTag(battlefieldMapTag);
        var mapSectors = battlefieldMap.GetComponent<MapController>().BattlefieldSectors;

        try
        {
            var targetSector = mapSectors[sectorLineIndex][sectorIndex];
            return targetSector;
        }
        catch (IndexOutOfRangeException)
        {
            Debug.LogError("Wrong front line or sector index - out of range");
            return null;
        }
    }
}
