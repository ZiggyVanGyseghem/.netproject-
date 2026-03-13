namespace OuderraadWielewaal.Models
{
    public enum ProductType
    {
        Drank,
        Versnapering
    }

    public enum BestelStatus
    {
        InDeWachtrij,
        InBereiding,
        KlaarVoorOphalen,
        Afgeleverd
    }

    public enum BetaalStatus
    {
        Open,
        Betaald,
        Geannuleerd,
        Mislukt
    }
}