# Enhanced Inventory System - Complete Implementation Plan

## Executive Summary

Transform the current lot-based inventory system into a comprehensive seed-to-sale manufacturing ERP with:
- **Inventory Classification**: Raw Materials, WIP, Finished Goods, Consumables
- **Product Catalog**: SKU definitions with full specifications
- **Bill of Materials (BOM)**: Manufacturing recipes with input/output definitions
- **Production Orders**: Work orders that consume inputs and produce outputs
- **Full Genealogy**: Multi-generational lineage from seed/clone through all transformations
- **Batch Integration**: Cultivation batches that flow into inventory

---

## 1. Core Concepts & Data Model

### 1.1 Inventory Classification

```
┌─────────────────────────────────────────────────────────────────┐
│                    INVENTORY CATEGORIES                         │
├─────────────────────────────────────────────────────────────────┤
│ RAW_MATERIAL     Seeds, Clones, Soil, Nutrients, Packaging      │
│ WORK_IN_PROGRESS Plants in grow, Drying product, Curing flower  │
│ FINISHED_GOOD    Packaged flower, Pre-rolls, Concentrates       │
│ CONSUMABLE       Labels, Bags, Jars, Cleaning supplies          │
│ BYPRODUCT        Trim, Shake, Stems (can become input to other) │
└─────────────────────────────────────────────────────────────────┘
```

### 1.2 Product vs Lot Distinction

**Product (SKU)** - The definition/template
- What CAN be produced or purchased
- Has specifications, UoM, category, BOMs
- Examples: "Blue Dream 1oz Flower", "OG Kush 1g Pre-roll"

**Lot** - The actual inventory
- Physical items in a location
- Has quantity, cost, expiration, COA status
- Links to a Product SKU
- Has lineage (where it came from)

### 1.3 Lineage Model (Genealogy)

```
Seed/Clone (Origin)
    │
    ▼
Cultivation Batch (Growing)
    │
    ├──▶ Harvest Event ──▶ Wet Weight Lot (WIP)
    │                           │
    │                           ▼
    │                      Drying Process
    │                           │
    │                           ▼
    │                      Dry Weight Lot (WIP)
    │                           │
    │         ┌─────────────────┼─────────────────┐
    │         ▼                 ▼                 ▼
    │    Trim (Byproduct)  Flower (WIP)     Shake (Byproduct)
    │         │                 │                 │
    │         ▼                 ▼                 ▼
    │    Extraction        Packaging          Edibles
    │    Production        Production         Production
    │         │                 │                 │
    │         ▼                 ▼                 ▼
    │    Concentrate       Packaged           Edible
    │    (Finished)        Flower (FG)        (Finished)
    └─────────────────────────────────────────────┘
```

---

## 2. New Type Definitions

### 2.1 Product (SKU) Types

```typescript
/** Product classification */
export type InventoryCategory = 
  | 'raw_material'
  | 'work_in_progress' 
  | 'finished_good'
  | 'consumable'
  | 'byproduct';

/** Product type for cannabis */
export type CannabisProductType =
  | 'seed' | 'clone' | 'mother_plant'
  | 'live_plant' | 'wet_flower' | 'dry_flower' | 'cured_flower'
  | 'trim' | 'shake' | 'stems'
  | 'crude_extract' | 'distillate' | 'live_resin' | 'rosin' | 'shatter' | 'wax'
  | 'preroll' | 'infused_preroll'
  | 'edible' | 'beverage' | 'capsule' | 'tincture' | 'topical'
  | 'packaged_flower' | 'packaged_concentrate' | 'packaged_edible';

/** Product SKU definition */
export interface Product {
  id: string;
  sku: string;
  name: string;
  description?: string;
  
  // Classification
  category: InventoryCategory;
  productType: CannabisProductType;
  
  // Specifications
  strainId?: string;
  strainName?: string;
  defaultUom: string;
  altUoms?: { uom: string; conversionFactor: number }[];
  
  // For finished goods
  netWeight?: number;
  netWeightUom?: string;
  packageSize?: number;
  
  // Compliance
  requiresCoa: boolean;
  shelfLifeDays?: number;
  storageRequirements?: string;
  
  // Costing
  standardCost?: number;
  costMethod: 'fifo' | 'lifo' | 'average' | 'specific';
  
  // Status
  isActive: boolean;
  isSellable: boolean;
  isPurchasable: boolean;
  isProducible: boolean;
  
  // Metadata
  attributes?: Record<string, unknown>;
  metrcItemCategory?: string;
  biotrackProductType?: string;
  
  createdAt: string;
  updatedAt: string;
}
```

### 2.2 Bill of Materials (BOM)

```typescript
/** BOM type */
export type BomType = 
  | 'production'    // Standard manufacturing
  | 'processing'    // Extraction/processing
  | 'packaging'     // Packaging operation
  | 'assembly'      // Kit assembly
  | 'disassembly';  // Breaking down

/** BOM header */
export interface BillOfMaterials {
  id: string;
  name: string;
  description?: string;
  bomType: BomType;
  
  // Output product
  outputProductId: string;
  outputProduct?: Product;
  outputQuantity: number;
  outputUom: string;
  
  // Process details
  defaultWorkCenterId?: string;
  estimatedDurationMinutes?: number;
  laborHoursPerUnit?: number;
  
  // Yield expectations
  expectedYieldPercent: number;
  yieldVarianceThreshold: number;
  
  // Versioning
  version: number;
  effectiveDate: string;
  expirationDate?: string;
  isActive: boolean;
  
  // Lines
  inputLines: BomInputLine[];
  byproductLines?: BomByproductLine[];
  
  // Compliance
  requiresQaApproval: boolean;
  requiredCertifications?: string[];
  
  createdAt: string;
  updatedAt: string;
}

/** BOM input line */
export interface BomInputLine {
  id: string;
  bomId: string;
  lineNumber: number;
  
  // Input product
  inputProductId: string;
  inputProduct?: Product;
  
  // Quantity per output unit
  quantityPer: number;
  uom: string;
  
  // Flexibility
  isOptional: boolean;
  substitutes?: string[]; // Alternative product IDs
  
  // Scrap/waste allowance
  scrapPercent: number;
}

/** BOM byproduct line */
export interface BomByproductLine {
  id: string;
  bomId: string;
  lineNumber: number;
  
  byproductProductId: string;
  byproductProduct?: Product;
  
  expectedQuantityPer: number;
  uom: string;
  
  // Whether this byproduct can be used as input elsewhere
  isRecoverable: boolean;
}
```

### 2.3 Production Order

```typescript
/** Production order status */
export type ProductionOrderStatus =
  | 'draft'
  | 'pending_materials'
  | 'ready'
  | 'in_progress'
  | 'on_hold'
  | 'completed'
  | 'cancelled';

/** Production order header */
export interface ProductionOrder {
  id: string;
  orderNumber: string;
  siteId: string;
  
  // What we're making
  bomId: string;
  bom?: BillOfMaterials;
  outputProductId: string;
  outputProduct?: Product;
  
  // Quantities
  plannedQuantity: number;
  plannedUom: string;
  actualQuantity?: number;
  
  // Status
  status: ProductionOrderStatus;
  priority: 'low' | 'normal' | 'high' | 'urgent';
  
  // Scheduling
  plannedStartDate: string;
  plannedEndDate: string;
  actualStartDate?: string;
  actualEndDate?: string;
  
  // Location
  workCenterId?: string;
  workCenterName?: string;
  
  // Source batch (for cultivation)
  sourceBatchId?: string;
  sourceBatchName?: string;
  
  // Labor
  estimatedLaborHours?: number;
  actualLaborHours?: number;
  
  // Yield tracking
  expectedYieldPercent: number;
  actualYieldPercent?: number;
  
  // Compliance
  requiresQaRelease: boolean;
  qaReleasedAt?: string;
  qaReleasedBy?: string;
  
  // Sync
  metrcProductionBatchId?: string;
  syncStatus: 'pending' | 'synced' | 'error';
  
  // Lines
  materialLines: ProductionMaterialLine[];
  outputLots?: ProductionOutputLot[];
  byproductLots?: ProductionByproductLot[];
  laborEntries?: ProductionLaborEntry[];
  
  notes?: string;
  createdAt: string;
  createdBy: string;
  updatedAt: string;
}

/** Material line - what's being consumed */
export interface ProductionMaterialLine {
  id: string;
  productionOrderId: string;
  lineNumber: number;
  
  // From BOM
  bomLineId?: string;
  productId: string;
  product?: Product;
  
  // Planned
  plannedQuantity: number;
  uom: string;
  
  // Actual consumption
  allocatedLotId?: string;
  allocatedLot?: InventoryLot;
  issuedQuantity?: number;
  issuedAt?: string;
  
  // Status
  status: 'pending' | 'allocated' | 'issued' | 'returned';
}

/** Output lot - what's being produced */
export interface ProductionOutputLot {
  id: string;
  productionOrderId: string;
  
  // The lot created
  lotId: string;
  lot?: InventoryLot;
  
  quantity: number;
  uom: string;
  
  // Cost allocation
  materialCost: number;
  laborCost: number;
  overheadCost: number;
  totalCost: number;
  unitCost: number;
  
  createdAt: string;
}

/** Labor entry */
export interface ProductionLaborEntry {
  id: string;
  productionOrderId: string;
  
  userId: string;
  userName: string;
  teamId?: string;
  teamName?: string;
  
  startTime: string;
  endTime: string;
  hoursWorked: number;
  
  laborType: 'direct' | 'indirect' | 'setup' | 'cleanup';
  hourlyRate?: number;
  totalCost?: number;
  
  notes?: string;
}
```

### 2.4 Cultivation Batch Integration

```typescript
/** Cultivation batch - source of all cannabis inventory */
export interface CultivationBatch {
  id: string;
  batchNumber: string;
  siteId: string;
  
  // Origin - the TRUE source
  originType: 'seed' | 'clone' | 'mother_cutting';
  originLotId?: string;         // Seed/clone lot consumed
  originMotherPlantId?: string; // If from mother cutting
  originGeneticId: string;      // Strain/genetic
  originGeneticName: string;
  
  // Generation tracking
  generationNumber: number;     // How many generations from original seed
  parentBatchId?: string;       // If cloned from another batch
  
  // Lifecycle
  currentPhase: 'propagation' | 'vegetative' | 'flowering' | 'harvest' | 'drying' | 'curing' | 'complete';
  phaseHistory: BatchPhaseEvent[];
  
  // Plant tracking
  initialPlantCount: number;
  currentPlantCount: number;
  plantLossEvents: PlantLossEvent[];
  
  // Location
  currentRoomId: string;
  currentZoneId?: string;
  locationHistory: BatchLocationEvent[];
  
  // Compliance
  metrcBatchId?: string;
  metrcPlantIds?: string[];
  plantTags?: string[];
  
  // Dates
  startDate: string;
  expectedHarvestDate?: string;
  actualHarvestDate?: string;
  
  // Yield tracking
  projectedYieldGrams?: number;
  actualWetWeightGrams?: number;
  actualDryWeightGrams?: number;
  
  // Cost accumulation
  seedCloneCost: number;
  laborCost: number;
  nutrientCost: number;
  utilityCost: number;
  overheadCost: number;
  totalCost: number;
  
  // Status
  status: 'active' | 'harvested' | 'destroyed' | 'cancelled';
  
  // Links to inventory
  harvestLotIds: string[];      // Lots created from harvest
  
  createdAt: string;
  updatedAt: string;
}

/** Phase transition event */
export interface BatchPhaseEvent {
  id: string;
  batchId: string;
  fromPhase: string;
  toPhase: string;
  transitionDate: string;
  notes?: string;
  performedBy: string;
}

/** Plant loss event */
export interface PlantLossEvent {
  id: string;
  batchId: string;
  lossDate: string;
  plantCount: number;
  reason: 'culling' | 'disease' | 'pest' | 'environmental' | 'male_identification' | 'other';
  notes?: string;
  recordedBy: string;
}
```

---

## 3. Enhanced Lot with Full Lineage

```typescript
/** Enhanced inventory lot with full genealogy */
export interface InventoryLot {
  id: string;
  siteId: string;
  
  // Core identification
  lotNumber: string;
  barcode: string;
  
  // Product link
  productId: string;
  product?: Product;
  
  // Classification (inherited from product but denormalized)
  category: InventoryCategory;
  productType: CannabisProductType;
  
  // Genetic traceability
  strainId?: string;
  strainName?: string;
  geneticLineage?: string;  // Full ancestry string
  
  // ORIGIN TRACKING - The key enhancement
  originType: 'cultivation' | 'purchase' | 'production' | 'receipt' | 'adjustment';
  
  // If from cultivation
  cultivationBatchId?: string;
  cultivationBatch?: CultivationBatch;
  harvestEventId?: string;
  
  // If from production
  productionOrderId?: string;
  productionOrder?: ProductionOrder;
  
  // If purchased
  purchaseOrderId?: string;
  vendorId?: string;
  vendorLotNumber?: string;
  
  // Parent lots (direct lineage)
  parentLotIds: string[];
  parentRelationships: LotLineageRelation[];
  
  // Child lots (what was created from this)
  childLotIds: string[];
  
  // Full ancestry chain (denormalized for fast queries)
  ancestryChain: string[];  // [oldest_ancestor, ..., direct_parent]
  generationDepth: number;  // How many transforms from origin
  
  // ... rest of existing lot fields ...
  quantity: number;
  uom: string;
  originalQuantity: number;
  locationId: string;
  locationPath: string;
  status: LotStatus;
  
  // Compliance
  metrcPackageId?: string;
  biotrackLotId?: string;
  
  // COA/Testing
  labOrderId?: string;
  coaStatus?: 'pending' | 'passed' | 'failed' | 'expired';
  testResults?: LabTestResults;
  
  // Costing
  materialCost: number;
  laborCost: number;
  overheadCost: number;
  totalCost: number;
  unitCost: number;
  
  // Dates
  createdAt: string;
  expirationDate?: string;
  harvestDate?: string;
  packageDate?: string;
  
  // Audit
  createdBy: string;
  updatedAt: string;
  updatedBy: string;
}

/** Lineage relationship */
export interface LotLineageRelation {
  parentLotId: string;
  childLotId: string;
  relationshipType: 'harvested_from' | 'processed_from' | 'split_from' | 'merged_from' | 'packaged_from';
  quantityConsumed: number;
  quantityProduced: number;
  conversionFactor: number;  // output/input ratio
  productionOrderId?: string;
  createdAt: string;
}
```

---

## 4. Frontend Architecture

### 4.1 New Routes Structure

```
/inventory
├── /                           # Dashboard (enhanced)
├── /products                   # Product catalog
│   ├── /                       # Product list
│   ├── /[id]                   # Product detail
│   └── /new                    # Create product
├── /boms                       # Bill of Materials
│   ├── /                       # BOM list
│   ├── /[id]                   # BOM detail/editor
│   └── /new                    # Create BOM
├── /production                 # Production orders
│   ├── /                       # Order list
│   ├── /[id]                   # Order detail/execute
│   ├── /new                    # Create order
│   └── /schedule               # Production schedule view
├── /lots                       # Lot management (existing, enhanced)
│   ├── /                       # Lot list with category filters
│   ├── /[id]                   # Lot detail with full lineage
│   └── /[id]/lineage           # Visual lineage graph
├── /batches                    # Cultivation batches
│   ├── /                       # Batch list
│   ├── /[id]                   # Batch detail
│   └── /[id]/harvest           # Harvest workflow
├── /movements                  # Movement history
├── /compliance                 # Compliance dashboard
├── /holds                      # Holds management
├── /locations                  # Location browser
├── /labels                     # Label generation
└── /reconciliation             # Reconciliation
```

### 4.2 Key UI Components

#### Product Catalog
- Product list with filtering by category, type, status
- Product detail with specifications, BOMs, inventory levels
- Quick product creation wizard

#### BOM Designer
- Visual BOM editor with drag-drop
- Input/output quantity calculator
- Yield projection tools
- Version comparison

#### Production Order Management
- Kanban board for order status
- Material availability checker
- Batch material allocation
- Yield variance analysis
- Labor time tracking

#### Lineage Visualizer
- Interactive family tree graph
- Forward tracing (where did this go?)
- Backward tracing (where did this come from?)
- Cost roll-up through lineage
- Compliance document chain

### 4.3 Enhanced Dashboard Widgets

```
┌─────────────────────────────────────────────────────────────────┐
│  INVENTORY DASHBOARD                                            │
├─────────────────────────────────────────────────────────────────┤
│ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐│
│ │ Raw Material│ │    WIP      │ │  Finished   │ │ Production  ││
│ │   $45,230   │ │  $128,500   │ │   $89,400   │ │  12 Orders  ││
│ │  156 lots   │ │   89 lots   │ │   234 lots  │ │  3 Active   ││
│ └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘│
├─────────────────────────────────────────────────────────────────┤
│ PRODUCTION SCHEDULE              │  MATERIAL AVAILABILITY       │
│ ┌─────────────────────────────┐ │ ┌─────────────────────────┐  │
│ │ Today  │ Tomorrow │ Week   │ │ │ Low Stock Items:    8   │  │
│ │   3    │    5     │   18   │ │ │ Allocated:         45%  │  │
│ │ orders │  orders  │ orders │ │ │ Reserved:          12%  │  │
│ └─────────────────────────────┘ │ └─────────────────────────┘  │
├─────────────────────────────────────────────────────────────────┤
│ ACTIVE PRODUCTION ORDERS        │  RECENT LINEAGE EVENTS       │
│ ┌─────────────────────────────┐ │ ┌─────────────────────────┐  │
│ │ PO-2025-0042 ███████░░ 70% │ │ │ Batch B-042 → 3 lots    │  │
│ │ Packaging 1oz Flower       │ │ │ LOT-5521 split → 2 lots │  │
│ │ Est: 2h 15m remaining      │ │ │ PO-041 completed        │  │
│ └─────────────────────────────┘ │ └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 5. Workflows

### 5.1 Seed-to-Sale Flow

```
1. SOURCING
   └── Purchase seeds/clones → Create Raw Material Lot
   
2. CULTIVATION
   ├── Start Batch (consume seed/clone lot)
   ├── Propagation → Vegetative → Flowering
   ├── Track plant counts, losses, costs
   └── Generate WIP value as costs accumulate

3. HARVEST
   ├── Complete Batch harvest event
   ├── Create Wet Weight WIP Lot (linked to batch)
   ├── Record wet weight, plant tags
   └── METRC harvest event

4. DRYING/CURING
   ├── Production Order: Drying Process
   ├── Input: Wet Weight Lot
   ├── Output: Dry Weight Lot(s)
   ├── Byproducts: Trim, Shake
   └── Track moisture loss yield

5. PROCESSING (if applicable)
   ├── Production Order: Extraction
   ├── Input: Trim/Flower lots
   ├── Output: Concentrate lots
   └── Track extraction yield

6. PACKAGING
   ├── Production Order: Packaging
   ├── Input: Flower/Concentrate + Packaging Materials
   ├── Output: Finished Good Lots
   ├── Apply compliance labels
   └── METRC package creation

7. TESTING
   ├── COA Order from Finished Lot
   ├── Hold until results
   └── Release or quarantine based on results

8. SALE/TRANSFER
   ├── Allocate from Finished Lots
   ├── Create transfer manifest
   └── METRC transfer event
```

### 5.2 Production Order Workflow

```
DRAFT → PENDING_MATERIALS → READY → IN_PROGRESS → COMPLETED
  │           │                │          │            │
  │           │                │          │            └── Output lots created
  │           │                │          │                Costs allocated
  │           │                │          │                METRC synced
  │           │                │          │
  │           │                │          └── Materials issued
  │           │                │              Labor tracked
  │           │                │              Progress updates
  │           │                │
  │           │                └── All materials allocated
  │           │                    QA approved (if required)
  │           │
  │           └── Material availability checked
  │               Auto-allocation attempted
  │
  └── BOM selected
      Quantities entered
      Schedule set
```

---

## 6. Implementation Phases

### Phase 1: Foundation (Week 1-2)
- [ ] Product catalog (types, services, CRUD)
- [ ] Basic product list/detail pages
- [ ] Update lot types with category field
- [ ] Database migrations for products table

### Phase 2: Bill of Materials (Week 2-3)
- [ ] BOM types and services
- [ ] BOM list/editor pages
- [ ] Visual BOM designer component
- [ ] BOM versioning

### Phase 3: Production Orders (Week 3-4)
- [ ] Production order types and services
- [ ] Order management pages
- [ ] Material allocation logic
- [ ] Yield tracking

### Phase 4: Cultivation Integration (Week 4-5)
- [ ] Cultivation batch types
- [ ] Batch → Harvest → Lot flow
- [ ] Plant tracking integration
- [ ] Cost accumulation

### Phase 5: Full Lineage (Week 5-6)
- [ ] Enhanced lineage tracking
- [ ] Visual lineage graph component
- [ ] Forward/backward tracing
- [ ] Compliance document chain

### Phase 6: Advanced Features (Week 6-7)
- [ ] Production scheduling
- [ ] Material requirements planning (MRP)
- [ ] Yield variance analysis
- [ ] Cost roll-up reports

---

## 7. API Contracts

```yaml
# Products
POST /inventory/products
GET  /inventory/products
GET  /inventory/products/:id
PUT  /inventory/products/:id

# BOMs
POST /inventory/boms
GET  /inventory/boms
GET  /inventory/boms/:id
PUT  /inventory/boms/:id
POST /inventory/boms/:id/clone  # Create new version

# Production Orders
POST /inventory/production-orders
GET  /inventory/production-orders
GET  /inventory/production-orders/:id
PUT  /inventory/production-orders/:id/status

# Material Operations
POST /inventory/production-orders/:id/allocate-materials
POST /inventory/production-orders/:id/issue-materials
POST /inventory/production-orders/:id/complete

# Lineage
GET  /inventory/lots/:id/lineage
GET  /inventory/lots/:id/ancestors
GET  /inventory/lots/:id/descendants
GET  /inventory/batches/:id/lots

# Cultivation
POST /inventory/batches
GET  /inventory/batches
GET  /inventory/batches/:id
POST /inventory/batches/:id/harvest
```

---

## 8. Compliance Integration

### METRC Mapping
- Products → METRC Item Categories
- Production Orders → METRC Production Batches
- Lots → METRC Packages
- Lineage → METRC Package Adjustments

### BioTrack Mapping
- Similar mappings for BioTrack states

### Audit Trail
- Every lineage event logged
- Production order changes tracked
- Cost allocation documented
- COA links preserved through chain

---

## 9. Success Criteria

1. **Traceability**: Any finished good can trace back to origin seed/clone in < 3 clicks
2. **Accuracy**: Inventory balances reconcile within 0.1% variance
3. **Compliance**: All METRC/BioTrack events sync within 15 minutes
4. **Usability**: Production order creation < 2 minutes
5. **Performance**: Lineage graph renders < 500ms for 10-generation depth

