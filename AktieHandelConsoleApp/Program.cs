using AktiehandelRepoLib;
using AktieHandelRepoLib;

AktieHandelRepo repository = new AktieHandelRepo();

repository.Add(new AktieHandel
{
    Navn = "Tesla",
    Antal = 10,
    HandelsPris = 250m
});

repository.Add(new AktieHandel
{
    Navn = "Apple",
    Antal = 15,
    HandelsPris = 180m
});

repository.Add(new AktieHandel
{
    Navn = "Novo Nordisk",
    Antal = -5,
    HandelsPris = 700m
});

Console.WriteLine("ALLE HANDLER");
Console.WriteLine("----------------");

foreach (AktieHandel handel in repository.GetAll())
{
    Console.WriteLine(handel);
}

Console.WriteLine();

Console.WriteLine("GET BY ID");
Console.WriteLine("----------------");

AktieHandel? fundet = repository.GetById(1);

if (fundet != null)
{
    Console.WriteLine(fundet);
}

Console.WriteLine();

Console.WriteLine("HANDLER MED PRIS OVER 200");
Console.WriteLine("----------------");

foreach (AktieHandel handel in repository.Get(200m, null))
{
    Console.WriteLine(handel);
}

Console.WriteLine();

Console.WriteLine("SORTERET EFTER PRIS");
Console.WriteLine("----------------");

foreach (AktieHandel handel in repository.Get(0m, null, "pris"))
{
    Console.WriteLine(handel);
}