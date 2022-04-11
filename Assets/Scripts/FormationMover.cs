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
    public Tuple<int, int> LocationSectorIndices { get; private set; }
    private float moveSpeed;
    private float rotationSpeed;

    private bool _isMoving;

    private string battlefieldMapTag;

    private GameObject configDataProvider;
    private const string gameConfigObjectTag = "Game Configurator";

    private FormationDestinationFinder destinationFinder;
    private FormationController currentFormationController;

    private Quaternion defaultFormationRotation = Quaternion.identity;

    void Start()
    {
        configDataProvider = GameObject.FindGameObjectWithTag(gameConfigObjectTag);
        var configDataReceiver = configDataProvider.GetComponent<GameConfigDataReceiver>();

        battlefieldMapTag = configDataReceiver.BattlefieldMapTag;

        moveSpeed = configDataReceiver.GetFormationSpeed();
        rotationSpeed = configDataReceiver.GetFormationRotationSpeed();

        destinationFinder = gameObject.GetComponent<FormationDestinationFinder>();
        currentFormationController = gameObject.GetComponent<FormationController>();

        IsMoving = false;
        IsRotating = false;
        LocationSectorIndices = null;

        //MoveToSector(1, 1);
    }

    public void MoveToSector(int sectorLineindex, int sectorIndex)
    {
        float formationMoveSpeed = moveSpeed;
        float formationRotationSpeed = rotationSpeed;

        var destination = destinationFinder.GetProperWayDestination(destinationFinder.GetMapSector(sectorLineindex, sectorIndex));

        var distanceToDestination = Vector2.Distance(transform.position, destination);
        var rotationTowardsTarget = Quaternion.Angle(transform.rotation, GetRotationTowardsTarget(destination));

        if (destination != null && formationMoveSpeed > 0)
        {
            float availableDistanceForInitialRotationCoeff = 0.25f;
            var maxAvailableDistanceForInitialRotation = distanceToDestination * availableDistanceForInitialRotationCoeff;

            if (rotationTowardsTarget / formationRotationSpeed <= maxAvailableDistanceForInitialRotation / formationMoveSpeed)
            {
                // If distance is big enough, the intial formation rotation to be started (otherwise skip)
                StartCoroutine(RotateFromation(GetRotationTowardsTarget(destination), formationRotationSpeed));
            }
            else
            {
                var arrivalRotation = Quaternion.Angle(transform.rotation, defaultFormationRotation);
                if (arrivalRotation / formationRotationSpeed >= distanceToDestination / formationMoveSpeed)
                {
                    // If the distance is too small for any (initial or arrival formation rotation), arrival rotation to be started
                    StartCoroutine(RotateFromation(defaultFormationRotation, formationRotationSpeed));
                }
            }

            RemoveFormationFromSector();
            LocationSectorIndices = new Tuple<int, int>(sectorLineindex, sectorIndex);
            StartCoroutine(MoveFormation(destination, formationMoveSpeed));

            int locatedFormationsInDestinationsSector = destinationFinder.GetMapSector(sectorLineindex, sectorIndex).GetComponent<MapSectorController>().LocatedFormations.Count;
            StartCoroutine(CheckForDestinationPositionChange(destination, new Tuple<int, int>(sectorLineindex, sectorIndex), locatedFormationsInDestinationsSector));
        }        
    }

    private IEnumerator MoveFormation(Vector2 destinationWaypointPosition, float speed)
    {
        IsMoving = true;

        while (!gameObject.transform.position.Equals(destinationWaypointPosition) && IsMoving)
        {
            var moveVector = Vector2.MoveTowards(transform.position, destinationWaypointPosition, speed * Time.deltaTime);
            transform.position = moveVector;

            var distanceToDestination = Vector2.Distance(transform.position, destinationWaypointPosition);
            var arrivalRotationAngle = Quaternion.Angle(transform.rotation, defaultFormationRotation);
            if (IsRotating == false &&
                IsDistnaceProperForFormationRotationOnMovement(distanceToDestination, speed, arrivalRotationAngle, rotationSpeed))
            {
                StartCoroutine(RotateFromation(defaultFormationRotation, rotationSpeed)); // Formation arrival rotation to be started
            }

            yield return new WaitForEndOfFrame();
        }

        if (IsMoving)
        {
            IsMoving = false;
            AttachFormationToSector();
        }
    }

    private IEnumerator CheckForDestinationPositionChange(Vector2 previouslyChosenDestination, Tuple<int, int> destinationMapSectorIndices, int previouslyLocatedFromationsInSector)
    {
        yield return new WaitForEndOfFrame();

        var destinationMapSector = destinationFinder.GetMapSector(destinationMapSectorIndices.Item1, destinationMapSectorIndices.Item2);
        while (IsMoving)
        {
            int currentlyLocatedFromationsInSector = destinationMapSector.GetComponent<MapSectorController>().LocatedFormations.Count;
            if (currentlyLocatedFromationsInSector != previouslyLocatedFromationsInSector)
            {
                var currentlyChosenDestination = destinationFinder.GetProperWayDestination(destinationMapSector);
                if (!currentlyChosenDestination.Equals(previouslyChosenDestination))
                {
                    StartCoroutine(RestartMovement(destinationMapSectorIndices.Item1, destinationMapSectorIndices.Item2));
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator RestartMovement(int destinationSectorLineIndex, int destinationSectorIndex)
    {
        StopAllMovement();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        MoveToSector(destinationSectorLineIndex, destinationSectorIndex);
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

    private Quaternion GetRotationTowardsTarget (Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        var offset = 270f;
        return Quaternion.Euler(Vector3.forward * (angle + offset));
    }

    private IEnumerator RotateFromation(Quaternion targetRotation, float rotationSpeed = 0)
    {
        IsRotating = true;
        
        float rotationAngleError = 0.1f;
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

    private void AttachFormationToSector(int sectorLineIndex, int sectorIndex)
    {
        var battlefieldMap = GameObject.FindGameObjectWithTag(battlefieldMapTag);
        var mapSectors = battlefieldMap.GetComponent<MapController>().BattlefieldSectors;

        var selectedSector = mapSectors[sectorLineIndex][sectorIndex];
        if(!selectedSector.GetComponent<MapSectorController>().LocatedFormations.Contains(gameObject))
        {
            selectedSector.GetComponent<MapSectorController>().LocatedFormations.Add(gameObject);
        }
        
        LocationSectorIndices = new Tuple<int, int>(sectorLineIndex, sectorIndex);
    }

    private void AttachFormationToSector()
    {
        if (LocationSectorIndices == null)
        {
            Debug.Log("The formation cannot be attached to the sector, no sector indices are specified");
            return;
        }

        var battlefieldMap = GameObject.FindGameObjectWithTag(battlefieldMapTag);
        var mapSectors = battlefieldMap.GetComponent<MapController>().BattlefieldSectors;

        var selectedSector = mapSectors[LocationSectorIndices.Item1][LocationSectorIndices.Item2];
        if (!selectedSector.GetComponent<MapSectorController>().LocatedFormations.Contains(gameObject))
        {
            selectedSector.GetComponent<MapSectorController>().LocatedFormations.Add(gameObject);
        }
    }

    private void RemoveFormationFromSector()
    {
        if(LocationSectorIndices == null)
        {
            Debug.Log("The formation is not attached to any map sector, cannot be removed from a sector");
            return;
        }

        var battlefieldMap = GameObject.FindGameObjectWithTag(battlefieldMapTag);
        var mapSectors = battlefieldMap.GetComponent<MapController>().BattlefieldSectors;

        var selectedSector = mapSectors[LocationSectorIndices.Item1][LocationSectorIndices.Item2];
        selectedSector.GetComponent<MapSectorController>().LocatedFormations.Remove(gameObject);
        LocationSectorIndices = null;
        
    }
}
