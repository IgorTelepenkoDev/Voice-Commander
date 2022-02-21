[System.Serializable]
internal class FormationParametersData
{
    // Pairs of allowed formation size proportions
    public int[,] formationProportionOptions;

    public float formationRowInterval;
    public float formationColumnInterval;

    public float formationSpeed;
    public float formationRotationSpeed;
}
