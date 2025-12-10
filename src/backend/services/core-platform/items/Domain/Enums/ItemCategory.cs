namespace Harvestry.Items.Domain.Enums;

/// <summary>
/// METRC item categories - maps to state-specific category lists
/// </summary>
public enum ItemCategory
{
    // Flower/Plant categories
    Buds = 0,
    Shake = 1,
    Trim = 2,
    Flower = 3,
    FlowerLot = 4,

    // Concentrate categories  
    Concentrate = 10,
    ConcentrateForInfusion = 11,
    Wax = 12,
    Shatter = 13,
    Resin = 14,
    Rosin = 15,
    Oil = 16,
    Distillate = 17,
    Kief = 18,
    Hash = 19,

    // Infused product categories
    InfusedEdible = 30,
    InfusedNonEdible = 31,
    InfusedPreRoll = 32,
    InfusedBeverage = 33,
    InfusedTopical = 34,
    InfusedTincture = 35,
    InfusedCapsule = 36,
    InfusedSuppository = 37,
    InfusedTransdermalPatch = 38,

    // Pre-roll categories
    PreRoll = 50,
    PreRollInfused = 51,
    PreRollFlower = 52,

    // Vaporizer categories
    VaporizerCartridge = 60,
    VaporizerPen = 61,

    // Plant/Propagation categories
    ImmaturePlant = 70,
    Clone = 71,
    Seeds = 72,
    Tissue = 73,
    MaturePlant = 74,

    // Other categories
    Sample = 90,
    WasteProduct = 91,
    Other = 99
}








