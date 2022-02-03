using System;
using System.Collections.Generic;
using UnityEngine;

public class FormationController : MonoBehaviour
{
    public int FormationUnits
    {
        get { return _formationUnits; }
        set
        {
            _formationUnits = value;
            OrganizeFormation();
            // should be provided change of the UI formation border size
        }
    }

    public List<GameObject> FormationSoldierUnits
    {
        get { return _formationSoldierUnits; }
        set
        {
            _formationSoldierUnits = value;
            FormationUnits = _formationSoldierUnits.Count;
        }
    }

    public int RowsInProportionCoefficient { get; private set; }
    public Tuple<int, int> CurrentFormationProportion { get; private set; }

    private List<GameObject> _formationSoldierUnits;
    private int _formationUnits;

    private GameObject configDataProvider;
    private const string gameConfigObjectTag = "Game Configurator";

    private List<Tuple<int, int>> availableFormationProportions;
    private Vector3 soldierUnitSize;
    private float rowInterval;
    private float columnInterval;

    private string soldierUnitTag;
    private string formationBorderElemsTag;

    private const string formationAreaObjName = "area";

    
    void Start()
    {
        configDataProvider = GameObject.FindGameObjectWithTag(gameConfigObjectTag);
        var configDataReceiver = configDataProvider.GetComponent<GameConfigDataReceiver>();

        availableFormationProportions = configDataReceiver.GetAvailableFormationProportions();
        soldierUnitSize = configDataReceiver.SoldierUnitSize;
        rowInterval = configDataReceiver.GetFormationRowInterval();
        columnInterval = configDataReceiver.GetFormationColumnInterval();
        soldierUnitTag = configDataReceiver.SoldierUnitTag;
        formationBorderElemsTag = configDataReceiver.FormationBorderElemsTag;


        FormationSoldierUnits = GetUnitsInFormation();
    }


    public List<GameObject> GetUnitsInFormation()
    {
        List<GameObject> formationUnits = new List<GameObject>();

        foreach (Transform childObj in gameObject.transform)
        {
            if (childObj.gameObject.tag == soldierUnitTag)
            {
                formationUnits.Add(childObj.gameObject);
            }
        }
        return formationUnits;
    }

    private void OrganizeFormation()
    {
        var formationProportion = GetBestFormationProportion();
        CurrentFormationProportion = formationProportion;

        GameObject formationFrameArea = null;
        foreach (Transform childObj in gameObject.transform)
        {
            if (childObj.gameObject.tag == formationBorderElemsTag && childObj.gameObject.name == formationAreaObjName)
            {
                formationFrameArea = childObj.gameObject;
                break;
            }
        }

        if (RowsInProportionCoefficient != 0 && FormationUnits != 0)
        {
            if (formationFrameArea != null)
            {
                formationFrameArea.transform.localScale = new Vector3(
                    (soldierUnitSize.x + columnInterval) * formationProportion.Item2 * RowsInProportionCoefficient - columnInterval,
                    (soldierUnitSize.y + rowInterval) * formationProportion.Item1 * RowsInProportionCoefficient - rowInterval);

                var unitPositions = GetFormationPositions(formationFrameArea, formationProportion.Item1 * RowsInProportionCoefficient, formationProportion.Item2 * RowsInProportionCoefficient);

                for (int unitPositionIndex = 0; unitPositionIndex < FormationUnits; unitPositionIndex++)
                {
                    FormationSoldierUnits[unitPositionIndex].transform.position = unitPositions[unitPositionIndex];
                }
            }
        }
    }

    private Tuple<int, int> GetBestFormationProportion()
    {
        Tuple<int, int> chosenFormationProportion = null;
        double unitsInFormationProportionsCoefficient = 0;

        foreach (var proportion in availableFormationProportions)
        {
            if (FormationUnits % (proportion.Item1 * proportion.Item2) == 0)
            {
                unitsInFormationProportionsCoefficient = Math.Sqrt(FormationUnits / (proportion.Item1 * proportion.Item2));
                if (unitsInFormationProportionsCoefficient != double.NaN && unitsInFormationProportionsCoefficient % 1 == 0)
                {
                    chosenFormationProportion = proportion;
                    break;
                }
            }
        }

        if (chosenFormationProportion != null)
        {
            RowsInProportionCoefficient = Convert.ToInt32(unitsInFormationProportionsCoefficient);
            return chosenFormationProportion;
        }
        else
        {
            int assumedFormationUnits = FormationUnits;
            while (chosenFormationProportion == null)
            {
                assumedFormationUnits++;
                foreach (var proportion in availableFormationProportions)
                {
                    if (assumedFormationUnits % (proportion.Item1 * proportion.Item2) == 0)
                    {
                        unitsInFormationProportionsCoefficient = Math.Sqrt(assumedFormationUnits / (proportion.Item1 * proportion.Item2));
                        if (unitsInFormationProportionsCoefficient != double.NaN && unitsInFormationProportionsCoefficient % 1 == 0)
                        {
                            //Debug.Log("for chosen proportion, the number of units places = " + assumedFormationUnits);
                            chosenFormationProportion = proportion;
                            break;
                        }
                    }
                }
            }

            RowsInProportionCoefficient = Convert.ToInt32(unitsInFormationProportionsCoefficient);
            return chosenFormationProportion;
        }
    }

    private List<Vector3> GetFormationPositions(GameObject formationArea, int formationRows, int formationColumns)
    {
        var formationAreaBounds = formationArea.GetComponent<SpriteRenderer>().bounds;
        var areaTopLeftCorner = new Vector3(formationAreaBounds.min.x, formationAreaBounds.max.y);

        var resultPositions = new List<Vector3>();

        var startPosition = new Vector3(areaTopLeftCorner.x + soldierUnitSize.x / 2, areaTopLeftCorner.y - soldierUnitSize.y / 2);

        for (int rowIndex = 0; rowIndex < formationRows; rowIndex++)
        {
            var rowY = startPosition.y - (soldierUnitSize.y + rowInterval) * rowIndex;
            var columnX = startPosition.x;
            for (int columnIndex = 0; columnIndex < formationColumns; columnIndex++)
            {
                if (columnIndex != 0)
                    columnX += soldierUnitSize.x + columnInterval;
                resultPositions.Add(new Vector3(columnX, rowY));
            }
        }

        return resultPositions;
    }
}
