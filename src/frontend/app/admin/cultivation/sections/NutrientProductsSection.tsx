'use client';

import React, { useState } from 'react';
import {
  Beaker,
  Plus,
  Edit2,
  Trash2,
  Package,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminTable,
  StatusBadge,
  TableActions,
  TableActionButton,
  TableSearch,
  Button,
  AdminModal,
  FormField,
  Input,
  Select,
  Textarea,
} from '@/components/admin';

// Mock data for nutrient products
const MOCK_PRODUCTS = [
  {
    id: '1',
    sku: 'NUT-CALMAG-1L',
    name: 'Cal-Mag Plus',
    brand: 'General Hydroponics',
    unit: 'L',
    unitCost: 15.99,
    densityGPerMl: 1.12,
    category: 'Base Nutrient',
    notes: 'Calcium-Magnesium supplement',
  },
  {
    id: '2',
    sku: 'NUT-BLOOM-A-1L',
    name: 'Flora Bloom A',
    brand: 'General Hydroponics',
    unit: 'L',
    unitCost: 18.50,
    densityGPerMl: 1.15,
    category: 'Base Nutrient',
    notes: 'Part A of 2-part bloom formula',
  },
  {
    id: '3',
    sku: 'NUT-BLOOM-B-1L',
    name: 'Flora Bloom B',
    brand: 'General Hydroponics',
    unit: 'L',
    unitCost: 18.50,
    densityGPerMl: 1.18,
    category: 'Base Nutrient',
    notes: 'Part B of 2-part bloom formula',
  },
  {
    id: '4',
    sku: 'NUT-PK-500ML',
    name: 'PK Booster 13-14',
    brand: 'Canna',
    unit: 'L',
    unitCost: 32.00,
    densityGPerMl: 1.25,
    category: 'Additive',
    notes: 'Phosphorus/Potassium boost for late flower',
  },
  {
    id: '5',
    sku: 'NUT-SILICA-1L',
    name: 'Silica Blast',
    brand: 'Botanicare',
    unit: 'L',
    unitCost: 22.00,
    densityGPerMl: 1.08,
    category: 'Additive',
    notes: 'Silica supplement for stem strength',
  },
];

const UNITS = [
  { value: 'L', label: 'Liter (L)' },
  { value: 'kg', label: 'Kilogram (kg)' },
  { value: 'gal', label: 'Gallon (gal)' },
  { value: 'lb', label: 'Pound (lb)' },
];

const CATEGORIES = [
  { value: 'base', label: 'Base Nutrient' },
  { value: 'additive', label: 'Additive' },
  { value: 'supplement', label: 'Supplement' },
  { value: 'ph', label: 'pH Adjustment' },
  { value: 'other', label: 'Other' },
];

export function NutrientProductsSection() {
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<typeof MOCK_PRODUCTS[0] | null>(null);

  const filteredProducts = MOCK_PRODUCTS.filter(
    (product) =>
      product.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      product.sku.toLowerCase().includes(searchQuery.toLowerCase()) ||
      product.brand.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleEdit = (product: typeof MOCK_PRODUCTS[0]) => {
    setEditingProduct(product);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingProduct(null);
    setIsModalOpen(true);
  };

  const columns = [
    {
      key: 'sku',
      header: 'SKU',
      width: '140px',
      render: (item: typeof MOCK_PRODUCTS[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">
          {item.sku}
        </span>
      ),
    },
    {
      key: 'name',
      header: 'Product Name',
      sortable: true,
      render: (item: typeof MOCK_PRODUCTS[0]) => (
        <div>
          <div className="font-medium text-foreground">{item.name}</div>
          <div className="text-xs text-muted-foreground">{item.brand}</div>
        </div>
      ),
    },
    {
      key: 'category',
      header: 'Category',
      render: (item: typeof MOCK_PRODUCTS[0]) => (
        <span className="text-xs bg-white/5 px-2 py-0.5 rounded">
          {item.category}
        </span>
      ),
    },
    {
      key: 'unit',
      header: 'Unit',
      render: (item: typeof MOCK_PRODUCTS[0]) => (
        <span>{item.unit}</span>
      ),
    },
    {
      key: 'cost',
      header: 'Unit Cost',
      render: (item: typeof MOCK_PRODUCTS[0]) => (
        <span className="text-emerald-400">${item.unitCost.toFixed(2)}</span>
      ),
    },
    {
      key: 'density',
      header: 'Density',
      render: (item: typeof MOCK_PRODUCTS[0]) => (
        <span className="text-muted-foreground">{item.densityGPerMl} g/mL</span>
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_PRODUCTS[0]) => (
        <TableActions>
          <TableActionButton onClick={() => handleEdit(item)}>
            <Edit2 className="w-4 h-4" />
          </TableActionButton>
          <TableActionButton onClick={() => {}} variant="danger">
            <Trash2 className="w-4 h-4" />
          </TableActionButton>
        </TableActions>
      ),
    },
  ];

  return (
    <AdminSection
      title="Nutrient Products"
      description="Manage your nutrient product catalog with SKUs, costs, and specifications"
    >
      <AdminCard
        title="Product Catalog"
        icon={Beaker}
        actions={
          <div className="flex items-center gap-3">
            <TableSearch
              value={searchQuery}
              onChange={setSearchQuery}
              placeholder="Search products..."
            />
            <Button onClick={handleCreate}>
              <Plus className="w-4 h-4" />
              Add Product
            </Button>
          </div>
        }
      >
        <AdminTable
          columns={columns}
          data={filteredProducts}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No nutrient products in catalog"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingProduct ? 'Edit Nutrient Product' : 'Create Nutrient Product'}
        description="Define product specifications and pricing"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingProduct ? 'Save Changes' : 'Create Product'}
            </Button>
          </>
        }
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="SKU" required>
              <Input placeholder="e.g., NUT-CALMAG-1L" defaultValue={editingProduct?.sku} />
            </FormField>
            <FormField label="Product Name" required>
              <Input placeholder="e.g., Cal-Mag Plus" defaultValue={editingProduct?.name} />
            </FormField>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Brand">
              <Input placeholder="e.g., General Hydroponics" defaultValue={editingProduct?.brand} />
            </FormField>
            <FormField label="Category" required>
              <Select options={CATEGORIES} defaultValue="base" />
            </FormField>
          </div>

          <div className="grid grid-cols-3 gap-4">
            <FormField label="Unit" required>
              <Select options={UNITS} defaultValue={editingProduct?.unit || 'L'} />
            </FormField>
            <FormField label="Unit Cost ($)" required>
              <Input type="number" step="0.01" placeholder="15.99" defaultValue={editingProduct?.unitCost} />
            </FormField>
            <FormField label="Density (g/mL)">
              <Input type="number" step="0.01" placeholder="1.10" defaultValue={editingProduct?.densityGPerMl} />
            </FormField>
          </div>

          <FormField label="Notes">
            <Textarea
              rows={2}
              placeholder="Additional product notes..."
              defaultValue={editingProduct?.notes}
            />
          </FormField>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

