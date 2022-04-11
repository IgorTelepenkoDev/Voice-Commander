[System.Serializable]
internal class FormationParametersData
{
    // Pairs of allowed formation size proportions
    public int[,] formationProportionOptions;

    public float formationRowInterval;
    public float formationColumnInterval;
    public float distanceBetweenFormations;
    public float intervalBetweenFormations;

    public float formationSpeed;    // distance / Time.deltaTime
    public float formationRotationSpeed;    // radians / Time.deltaTime
}
