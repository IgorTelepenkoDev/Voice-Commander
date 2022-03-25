using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationMover : MonoBehaviour
{
    public bool IsMoving 
    { 
        get { return _isMoving; } 
        private set 
        { 
            _isMoving = value;
            SetStateForUnitsInFormation(currentFormationController, _isMoving);
        } 
    }
    public bool IsRotating { get; private set; }
    private float moveSpeed;
    private float rotationSpeed;

    private bool _isMoving;

    private GameObject configDataProvider;
    private const string gameConfigObjectTag = "Game Configurator";

    private FormationDestinationFinder destinationFinder;
    private FormationController currentFormationController;

    void Start()
    {
        configDataProvider = GameObject.FindGameObjectWithTag(gameConfigObjectTag);
        var configDataReceiver = configDataProvider.GetComponent<GameConfigDataReceiver>();

        moveSpeed = configDataReceiver.GetFormationSpeed();
        rotationSpeed = configDataReceiver.GetFormationRotationSpeed();

        destinationFinder = gameObject.GetComponent<FormationDestinationFinder>();
        currentFormationController = gameObject.GetComponent<FormationController>();

        IsMoving = false;
        IsRotating = false;

        //MoveToSector(1, 0);        
    }

    public void MoveToSector(int sectorLineindex, int sectorIndex)
    {
        float formationMoveSpeed = moveSpeed;
        float formationRotationSpeed = rotationSpeed;
        var destination = destinationFinder.GetClosestSectorWaypoint(destinationFinder.GetSectorWaypoints(sectorLineindex, sectorIndex));
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

    private void SetStateForUnitsInFormation(FormationController formationController, bool areUnitsMoving)
    {
        var formationUnits = formationController.FormationSoldierUnits;
        foreach (var unit in formationUnits)
        {
            unit.GetComponent<UnitController>().IsMoving = areUnitsMoving;
        }
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
}
