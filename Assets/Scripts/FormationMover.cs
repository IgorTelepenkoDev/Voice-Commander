using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FormationMover : MonoBehaviour
{
    public bool IsMoving { get; private set; }
    private float moveSpeed;

    private string battlefieldMapTag;
    private string sectorWaypointTag;

    private GameObject configDataProvider;
    private const string gameConfigObjectTag = "Game Configurator";


    void Start()
    {
        configDataProvider = GameObject.FindGameObjectWithTag(gameConfigObjectTag);
        var configDataReceiver = configDataProvider.GetComponent<GameConfigDataReceiver>();

        battlefieldMapTag = configDataReceiver.BattlefieldMapTag;
        sectorWaypointTag = configDataReceiver.MapSectorWaypointTag;
        moveSpeed = configDataReceiver.GetFormationSpeed();

        IsMoving = false;
        //MoveToSector(2, 2);
    }

    public void MoveToSector(int sectorLineindex, int sectorIndex)
    {
        var destination = GetClosestSectorWaypoint(GetSectorWaypoints(sectorLineindex, sectorIndex));

        if (destination != null && moveSpeed > 0)
        {
            transform.rotation = GetRotationTowardsTarget(destination); // make smooth rotation before and after the movement
            StartCoroutine(MoveFormation(destination, moveSpeed));
        }        
    }

    public void StopMovement()
    {
        IsMoving = false;
        Debug.Log("The formation movement is stopped");
    }

    private IEnumerator MoveFormation(GameObject destinationWaypoint, float speed)
    {
        IsMoving = true;

        while(!gameObject.transform.position.Equals(destinationWaypoint.transform.position) && IsMoving)
        {
            var moveVector = Vector2.MoveTowards(transform.position, destinationWaypoint.transform.position, speed * Time.deltaTime);
            transform.position = moveVector;
            yield return new WaitForEndOfFrame();
        }

        if (IsMoving)
        {
            transform.rotation = destinationWaypoint.transform.rotation;
            IsMoving = false;
        }
    }

    private Quaternion GetRotationTowardsTarget (GameObject target)
    {
        Vector2 direction = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        var offset = 270f;
        return Quaternion.Euler(Vector3.forward * (angle + offset));
    }

    private GameObject GetClosestSectorWaypoint(GameObject[] mapSectorWaypoints)
    {
        if (mapSectorWaypoints == null)
        {
            return null;
        }

        var closestWaypoint = mapSectorWaypoints.
            OrderBy(waypoint => Vector2.Distance(gameObject.transform.position, waypoint.transform.position)).FirstOrDefault();

        return closestWaypoint;
    }

    private GameObject[] GetSectorWaypoints(int sectorLineIndex, int sectorIndex)
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
