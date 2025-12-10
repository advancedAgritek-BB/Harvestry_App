namespace Harvestry.Packages.Domain.Enums;

/// <summary>
/// Classification of inventory for financial and operational reporting
/// </summary>
public enum InventoryCategory
{
    /// <summary>
    /// Raw materials and inputs for production (seeds, clones, nutrients, packaging)
    /// </summary>
    RawMaterial = 0,

    /// <summary>
    /// Work in progress - items currently being processed (plants, drying flower, curing)
    /// </summary>
    WorkInProgress = 1,

    /// <summary>
    /// Finished goods ready for sale (packaged flower, concentrates, edibles)
    /// </summary>
    FinishedGood = 2,

    /// <summary>
    /// Consumables used in operations (labels, bags, cleaning supplies)
    /// </summary>
    Consumable = 3,

    /// <summary>
    /// Byproducts that can be reprocessed (trim, shake, stems)
    /// </summary>
    Byproduct = 4
}



