namespace brewlog.domain.Models
{
    public record YeastStarter
    (
        double InitialCells, 
        double DryMaltExtract, 
        double WaterLitres, 
        double CalculatedYeastCells
    );
}
