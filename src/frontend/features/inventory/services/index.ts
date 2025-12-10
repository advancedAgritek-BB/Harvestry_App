/**
 * Inventory Services Index
 * Re-exports all inventory-related services
 */

// Core inventory
export * from './inventory.service';
export * from './scanning.service';
export * from './compliance.service';
export * from './labels.service';

// Manufacturing
export * from './product.service';
export * from './bom.service';
export * from './production.service';

// WMS
export * from './packages.service';
export * from './financial.service';
export * from './location.service';
