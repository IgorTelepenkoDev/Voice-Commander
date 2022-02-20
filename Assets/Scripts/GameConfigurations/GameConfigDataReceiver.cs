using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class GameConfigDataReceiver : MonoBehaviour
{
    public string SoldierUnitTag { get { return "Soldier Unit"; } }
    public string FormationBorderElemsTag { get { return "Formation Frame"; } }
    public string BattlefieldMapTag { get { return "Map"; } }
    public string MapSectorWaypointTag { get { return "Sector Waypoint"; } }

    public GameObject etalonSoldierUnit;
    public Vector3 SoldierUnitSize { get; private set; }

    private const string formationParametersConfigFilePath = "Assets/Resources/FormationParametersConfigData.dat";

    void Start()
    {
        if (etalonSoldierUnit != null)
        {
            SoldierUnitSize = etalonSoldierUnit.GetComponent<SpriteRenderer>().bounds.size;
        }
    }

    public List<Tuple<int,int>> GetAvailableFormationProportions()
    {
        var availableFormationProportions = new List<Tuple<int, int>>();

        var formationsParametersData = GetFormationParametersData();
        var formationProportions = formationsParametersData.formationProportionOptions;

        for (int proportionIndex = 0; proportionIndex < formationProportions.GetLength(0); proportionIndex++)
        {
            availableFormationProportions.Add(new Tuple<int, int>(formationProportions[proportionIndex, 0], formationProportions[proportionIndex, 1]));
        }

        return availableFormationProportions;
    }

    public float GetFormationRowInterval()
    {
        var formationsParametersData = GetFormationParametersData();
        return formationsParametersData.formationRowInterval;
    }

    public float GetFormationColumnInterval()
    {
        var formationsParametersData = GetFormationParametersData();
        return formationsParametersData.formationColumnInterval;
    }

    public float GetFormationSpeed()
    {
        var formationsParametersData = GetFormationParametersData();
        return formationsParametersData.formationSpeed;
    }

    private FormationParametersData GetFormationParametersData(string formationParametersFilePath = formationParametersConfigFilePath)
    {
        if (File.Exists(formationParametersFilePath))
        {
            string formationParametersFileContents = File.ReadAllText(formationParametersFilePath);
            return JsonConvert.DeserializeObject<FormationParametersData>(formationParametersFileContents);
        }
        else
        {
            Debug.LogError("formation parameters file not found");
            return null;
        }
    }
}
