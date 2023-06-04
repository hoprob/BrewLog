export class YeastStarter{
    public initialCells: number = 0;
    public dryMaltExtract: number;
    public waterLitres: number;
    public calculatedYeastCells: number;

    constructor(initialCells: number){
        this.initialCells = initialCells;
        this.dryMaltExtract = 0;
        this.waterLitres = 0;
        this.calculatedYeastCells = 0;
    }
}