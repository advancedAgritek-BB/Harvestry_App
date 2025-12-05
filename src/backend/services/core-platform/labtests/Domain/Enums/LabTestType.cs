namespace Harvestry.LabTests.Domain.Enums;

/// <summary>
/// Types of lab tests (METRC test types)
/// </summary>
public enum LabTestType
{
    // Potency tests
    Potency = 0,
    THC = 1,
    CBD = 2,
    Cannabinoids = 3,

    // Terpene tests
    Terpenes = 10,

    // Safety tests
    Microbial = 20,
    Mycotoxins = 21,
    Pesticides = 22,
    HeavyMetals = 23,
    ResidualSolvents = 24,
    ForeignMaterial = 25,
    WaterActivity = 26,
    MoistureCon = 27,

    // Other tests
    Homogeneity = 40,
    Other = 99
}



