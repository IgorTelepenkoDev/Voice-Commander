using System;
using System.Collections.Generic;
using UnityEngine;

public class FormationController : MonoBehaviour
{
    public List<Tuple<int, int>> AvailableFormationProportions
    {
        get { return _availableFormationProportions; }
        set { _availableFormationProportions = value; }
    }

    public Vector3 SoldierUnitSize
    {
        get { return _soldierUnitSize; }
        set { _soldierUnitSize = value; }
    }

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


    private List<GameObject> _formationSoldierUnits;
    private int _formationUnits;
    private int rowsInProportionCoefficient;

    // To be read from config file (json) in a centralized manner:
    private List<Tuple<int, int>> _availableFormationProportions;
    private Vector3 _soldierUnitSize;

    private const float rowGapBetweenUnits = 0;
    private const float columnGapBetweenUnits = 0;

    // To make in a centralized manner
    private const string soldierUnitTag = "Soldier Unit";
    private const string fromationBorderElemsTag = "Formation Frame";
    private const string formationAreaObjName = "area";


    void Start()
    {
        AvailableFormationProportions = new List<Tuple<int, int>>
        {
            Tuple.Create( 3, 5 ),
            Tuple.Create( 3, 4 ),
            Tuple.Create( 2, 3 )
        };  // provide reading from json (in a centrilized manner)

        var exampleUnit = GameObject.FindGameObjectWithTag(soldierUnitTag);
        //Debug.Log("sprite size = " + exampleUnit.GetComponent<SpriteRenderer>().bounds.size);
        SoldierUnitSize = exampleUnit.GetComponent<SpriteRenderer>().bounds.size;

        FormationSoldierUnits = GetUnitsInFormation();
    }


    void Update()
    {

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
        //Debug.Log("Proportion = " + formationProportion);
        //Debug.Log("Coefficient or rows/proportion row = " + rowsInProportionCoefficient);

        GameObject formationFrameArea = null;
        foreach (Transform childObj in gameObject.transform)
        {
            if (childObj.gameObject.tag == fromationBorderElemsTag && childObj.gameObject.name == formationAreaObjName)
            {
                formationFrameArea = childObj.gameObject;
                break;
            }
        }

        if (rowsInProportionCoefficient != 0 && FormationUnits != 0)
        {
            if (formationFrameArea != null)
            {
                formationFrameArea.transform.localScale = new Vector3(
                    (SoldierUnitSize.x + columnGapBetweenUnits) * formationProportion.Item2 * rowsInProportionCoefficient - columnGapBetweenUnits,
                    (SoldierUnitSize.y + rowGapBetweenUnits) * formationProportion.Item1 * rowsInProportionCoefficient - rowGapBetweenUnits);

                var unitPositions = GetFormationPositions(formationFrameArea, formationProportion.Item1 * rowsInProportionCoefficient, formationProportion.Item2 * rowsInProportionCoefficient);

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

        foreach (var proportion in AvailableFormationProportions)
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
            rowsInProportionCoefficient = Convert.ToInt32(unitsInFormationProportionsCoefficient);
            return chosenFormationProportion;
        }
        else
        {
            int assumedFormationUnits = FormationUnits;
            while (chosenFormationProportion == null)
            {
                assumedFormationUnits++;
                foreach (var proportion in _availableFormationProportions)
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

            rowsInProportionCoefficient = Convert.ToInt32(unitsInFormationProportionsCoefficient);
            return chosenFormationProportion;
        }
    }

    private List<Vector3> GetFormationPositions(GameObject formationArea, int formationRows, int formationColumns)
    {
        var formationAreaBounds = formationArea.GetComponent<SpriteRenderer>().bounds;
        var areaTopLeftCorner = new Vector3(formationAreaBounds.min.x, formationAreaBounds.max.y);

        var resultPositions = new List<Vector3>();

        var startPosition = new Vector3(areaTopLeftCorner.x + SoldierUnitSize.x / 2, areaTopLeftCorner.y - SoldierUnitSize.y / 2);

        for (int rowIndex = 0; rowIndex < formationRows; rowIndex++)
        {
            var rowY = startPosition.y - (SoldierUnitSize.y + rowGapBetweenUnits) * rowIndex;
            var columnX = startPosition.x;
            for (int columnIndex = 0; columnIndex < formationColumns; columnIndex++)
            {
                if (columnIndex != 0)
                    columnX += SoldierUnitSize.x + columnGapBetweenUnits;
                resultPositions.Add(new Vector3(columnX, rowY));
            }
        }

        return resultPositions;
    }
}
