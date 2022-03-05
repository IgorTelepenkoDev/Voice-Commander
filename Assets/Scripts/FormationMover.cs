using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FormationMover : MonoBehaviour
{
    public bool IsMoving { get; private set; }
    public bool IsRotating { get; private set; }
    private float moveSpeed;
    private float rotationSpeed;

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
        rotationSpeed = configDataReceiver.GetFormationRotationSpeed();

        IsMoving = false;
        IsRotating = false;

        //transform.rotation = new Quaternion(0, 0, -0.9f, 0.3f);
        //MoveToSector(2, 3);
    }

    public void MoveToSector(int sectorLineindex, int sectorIndex)
    {
        float formationMoveSpeed = moveSpeed;
        float formationRotationSpeed = rotationSpeed;
        var destination = GetClosestSectorWaypoint(GetSectorWaypoints(sectorLineindex, sectorIndex));
        var distanceToDestination = Vector2.Distance(transform.position, destination.transform.position);
        var rotationTowardsTarget = Quaternion.Angle(transform.rotation, GetRotationTowardsTarget(destination));

        if (destination != null && formationMoveSpeed > 0)
        {
            float availableDistanceForInitialRotationCoeff = 0.2f;
            var maxAvailableDistanceForInitialRotation = distanceToDestination * availableDistanceForInitialRotationCoeff;

            if (rotationTowardsTarget / formationRotationSpeed <= maxAvailableDistanceForInitialRotation / formationMoveSpeed)
            {
                // If distance is big enough, the intial formation rotation to be started (otherwise skip)
                StartCoroutine(RotateFromation(GetRotationTowardsTarget(destination), formationRotationSpeed));
            }
            else
            {
                var arrivalRotation = Quaternion.Angle(transform.rotation, destination.transform.rotation);
                if (arrivalRotation / formationRotationSpeed >= distanceToDestination / formationMoveSpeed)
                {
                    // If the distance is too small for any (initial or arrival formation rotation), arrival rotation to be started
                    StartCoroutine(RotateFromation(destination.transform.rotation, formationRotationSpeed));
                }
            }
            
            StartCoroutine(MoveFormation(destination, formationMoveSpeed));
        }        
    }

    public void StopAllMovement()
    {
        IsMoving = false;
        IsRotating = false;
        Debug.Log("The formation movement (and rotation) is stopped");
    }

    private IEnumerator MoveFormation(GameObject destinationWaypoint, float speed)
    {
        IsMoving = true;

        while(!gameObject.transform.position.Equals(destinationWaypoint.transform.position) && IsMoving)
        {
            var moveVector = Vector2.MoveTowards(transform.position, destinationWaypoint.transform.position, speed * Time.deltaTime);
            transform.position = moveVector;

            var distanceToDestination = Vector2.Distance(transform.position, destinationWaypoint.transform.position);
            var arrivalRotationAngle = Quaternion.Angle(transform.rotation, destinationWaypoint.transform.rotation);
            if(IsRotating == false && 
                IsDistnaceProperForFormationRotationOnMovement(distanceToDestination, speed, arrivalRotationAngle, rotationSpeed))
            {
                StartCoroutine(RotateFromation(destinationWaypoint.transform.rotation, rotationSpeed)); // Formation arrival rotation to be started
            }

            yield return new WaitForEndOfFrame();
        }

        if (IsMoving)
        {
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

    private IEnumerator RotateFromation(Quaternion targetRotation, float rotationSpeed = 0)
    {
        IsRotating = true;
        
        float rotationAngleError = 1f;
        while (Mathf.Abs(Quaternion.Angle(transform.rotation, targetRotation)) > rotationAngleError && IsRotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        
        if(IsRotating)
        {
            IsRotating = false;
        }
    }

    private bool IsDistnaceProperForFormationRotationOnMovement(float distanceToDestination, float moveSpeed, float rotationAngle, float rotationSpeed)
    {
        var requiredDistanceError = Time.deltaTime;
        if (Mathf.Abs(distanceToDestination / moveSpeed - rotationAngle / rotationSpeed) < requiredDistanceError)
        {
            //Debug.Log("The distance is appropraite for the rotation");
            return true;
        }
        else
        {
            return false;
        }
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
