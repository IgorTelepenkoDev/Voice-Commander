using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    // should be filled in Editor from the scene
    public List<GameObject> DefenceSectors;
    public List<GameObject> ThirdFrontSectors;
    public List<GameObject> SecondFrontSectors;
    public List<GameObject> FirstFrontSectors;

    private const int battlefieldSectorLines = 4;

    public GameObject[][] BattlefieldSectors = new GameObject[battlefieldSectorLines][];

    void Start()
    {
        if (DefenceSectors != null && ThirdFrontSectors != null && SecondFrontSectors != null && FirstFrontSectors != null)
        {
            BattlefieldSectors[0] = new GameObject[DefenceSectors.Count];
            for (int defenceSectorInd = 0; defenceSectorInd < DefenceSectors.Count; defenceSectorInd++)
            {
                BattlefieldSectors[0][defenceSectorInd] = DefenceSectors[defenceSectorInd];
            }

            BattlefieldSectors[1] = new GameObject[ThirdFrontSectors.Count];
            for(int thirdFrontSectorInd = 0; thirdFrontSectorInd < ThirdFrontSectors.Count; thirdFrontSectorInd ++)
            {
                BattlefieldSectors[1][thirdFrontSectorInd] = ThirdFrontSectors[thirdFrontSectorInd];
            }

            BattlefieldSectors[2] = new GameObject[SecondFrontSectors.Count];
            for (int secondFrontSectorInd = 0; secondFrontSectorInd < SecondFrontSectors.Count; secondFrontSectorInd++)
            {
                BattlefieldSectors[2][secondFrontSectorInd] = SecondFrontSectors[secondFrontSectorInd];
            }

            BattlefieldSectors[3] = new GameObject[FirstFrontSectors.Count];
            for (int firstFrontSectorInd = 0; firstFrontSectorInd < FirstFrontSectors.Count; firstFrontSectorInd++)
            {
                BattlefieldSectors[3][firstFrontSectorInd] = FirstFrontSectors[firstFrontSectorInd];
            }
        }
    }

}
