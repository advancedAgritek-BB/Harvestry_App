'use client';

import React, { useState } from 'react';
import {
  Ruler,
  Tag,
  Barcode,
  Plus,
  Edit2,
  Trash2,
  Eye,
  Copy,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminTabs,
  TabPanel,
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
  Switch,
  Textarea,
} from '@/components/admin';

const INVENTORY_TABS = [
  { id: 'uom', label: 'Units of Measure', icon: Ruler },
  { id: 'labels', label: 'Label Templates', icon: Tag },
  { id: 'barcodes', label: 'Barcode Settings', icon: Barcode },
];

// Mock data
const MOCK_UOM = [
  { id: '1', code: 'g', name: 'Gram', category: 'Weight', baseUnit: true, conversionFactor: 1 },
  { id: '2', code: 'kg', name: 'Kilogram', category: 'Weight', baseUnit: false, conversionFactor: 1000 },
  { id: '3', code: 'oz', name: 'Ounce', category: 'Weight', baseUnit: false, conversionFactor: 28.3495 },
  { id: '4', code: 'lb', name: 'Pound', category: 'Weight', baseUnit: false, conversionFactor: 453.592 },
  { id: '5', code: 'mL', name: 'Milliliter', category: 'Volume', baseUnit: true, conversionFactor: 1 },
  { id: '6', code: 'L', name: 'Liter', category: 'Volume', baseUnit: false, conversionFactor: 1000 },
  { id: '7', code: 'ea', name: 'Each', category: 'Count', baseUnit: true, conversionFactor: 1 },
];

const MOCK_LABELS = [
  { id: '1', name: 'Product Label - Colorado', jurisdiction: 'Colorado', type: 'Product', format: 'GS1-128', active: true },
  { id: '2', name: 'Package Label - Colorado', jurisdiction: 'Colorado', type: 'Package', format: 'QR Code', active: true },
  { id: '3', name: 'Transport Manifest', jurisdiction: 'All', type: 'Manifest', format: 'GS1-128', active: true },
  { id: '4', name: 'Product Label - California', jurisdiction: 'California', type: 'Product', format: 'GS1-128', active: false },
];

const MOCK_BARCODES = [
  { id: '1', name: 'Default GS1-128', format: 'GS1-128', prefix: '(01)', checkDigit: true, active: true },
  { id: '2', name: 'QR Code Standard', format: 'QR Code', prefix: '', checkDigit: false, active: true },
  { id: '3', name: 'Code 128', format: 'Code 128', prefix: '', checkDigit: true, active: true },
];

const UOM_CATEGORIES = [
  { value: 'weight', label: 'Weight' },
  { value: 'volume', label: 'Volume' },
  { value: 'count', label: 'Count' },
  { value: 'length', label: 'Length' },
  { value: 'area', label: 'Area' },
];

const LABEL_TYPES = [
  { value: 'product', label: 'Product' },
  { value: 'package', label: 'Package' },
  { value: 'manifest', label: 'Manifest' },
  { value: 'batch', label: 'Batch' },
];

const BARCODE_FORMATS = [
  { value: 'gs1-128', label: 'GS1-128' },
  { value: 'qr', label: 'QR Code' },
  { value: 'code128', label: 'Code 128' },
  { value: 'datamatrix', label: 'Data Matrix' },
];

export default function InventoryAdminPage() {
  const [activeTab, setActiveTab] = useState('uom');
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);

  const uomColumns = [
    {
      key: 'code',
      header: 'Code',
      width: '80px',
      render: (item: typeof MOCK_UOM[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">{item.code}</span>
      ),
    },
    {
      key: 'name',
      header: 'Unit Name',
      sortable: true,
      render: (item: typeof MOCK_UOM[0]) => (
        <div className="font-medium text-foreground">{item.name}</div>
      ),
    },
    {
      key: 'category',
      header: 'Category',
      render: (item: typeof MOCK_UOM[0]) => (
        <span className="text-sm text-muted-foreground">{item.category}</span>
      ),
    },
    {
      key: 'baseUnit',
      header: 'Base Unit',
      render: (item: typeof MOCK_UOM[0]) => (
        item.baseUnit ? (
          <span className="text-xs bg-cyan-500/10 text-cyan-400 px-2 py-0.5 rounded">Base</span>
        ) : (
          <span className="text-xs text-muted-foreground">×{item.conversionFactor}</span>
        )
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: () => (
        <TableActions>
          <TableActionButton onClick={() => setIsModalOpen(true)}><Edit2 className="w-4 h-4" /></TableActionButton>
          <TableActionButton onClick={() => {}} variant="danger"><Trash2 className="w-4 h-4" /></TableActionButton>
        </TableActions>
      ),
    },
  ];

  const labelColumns = [
    {
      key: 'name',
      header: 'Template Name',
      sortable: true,
      render: (item: typeof MOCK_LABELS[0]) => (
        <div className="font-medium text-foreground">{item.name}</div>
      ),
    },
    {
      key: 'jurisdiction',
      header: 'Jurisdiction',
      render: (item: typeof MOCK_LABELS[0]) => (
        <span className="text-sm text-muted-foreground">{item.jurisdiction}</span>
      ),
    },
    {
      key: 'type',
      header: 'Type',
      render: (item: typeof MOCK_LABELS[0]) => (
        <span className="text-xs bg-white/5 px-2 py-0.5 rounded">{item.type}</span>
      ),
    },
    {
      key: 'format',
      header: 'Barcode Format',
      render: (item: typeof MOCK_LABELS[0]) => (
        <span className="text-xs text-cyan-400">{item.format}</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_LABELS[0]) => (
        <StatusBadge status={item.active ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '120px',
      render: () => (
        <TableActions>
          <TableActionButton onClick={() => {}}><Eye className="w-4 h-4" /></TableActionButton>
          <TableActionButton onClick={() => {}}><Copy className="w-4 h-4" /></TableActionButton>
          <TableActionButton onClick={() => setIsModalOpen(true)}><Edit2 className="w-4 h-4" /></TableActionButton>
        </TableActions>
      ),
    },
  ];

  const barcodeColumns = [
    {
      key: 'name',
      header: 'Configuration Name',
      sortable: true,
      render: (item: typeof MOCK_BARCODES[0]) => (
        <div className="font-medium text-foreground">{item.name}</div>
      ),
    },
    {
      key: 'format',
      header: 'Format',
      render: (item: typeof MOCK_BARCODES[0]) => (
        <span className="text-xs bg-white/5 px-2 py-0.5 rounded">{item.format}</span>
      ),
    },
    {
      key: 'prefix',
      header: 'Prefix',
      render: (item: typeof MOCK_BARCODES[0]) => (
        <span className="font-mono text-sm text-muted-foreground">{item.prefix || '—'}</span>
      ),
    },
    {
      key: 'checkDigit',
      header: 'Check Digit',
      render: (item: typeof MOCK_BARCODES[0]) => (
        <span className={item.checkDigit ? 'text-emerald-400' : 'text-muted-foreground'}>
          {item.checkDigit ? 'Yes' : 'No'}
        </span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_BARCODES[0]) => (
        <StatusBadge status={item.active ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: () => (
        <TableActions>
          <TableActionButton onClick={() => setIsModalOpen(true)}><Edit2 className="w-4 h-4" /></TableActionButton>
        </TableActions>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <AdminTabs tabs={INVENTORY_TABS} activeTab={activeTab} onChange={setActiveTab} />

      <TabPanel id="uom" activeTab={activeTab}>
        <AdminSection title="Units of Measure" description="Define units and conversion factors">
          <AdminCard title="UoM Definitions" icon={Ruler} actions={
            <div className="flex items-center gap-3">
              <TableSearch value={searchQuery} onChange={setSearchQuery} placeholder="Search units..." />
              <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Add Unit</Button>
            </div>
          }>
            <AdminTable columns={uomColumns} data={MOCK_UOM} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="labels" activeTab={activeTab}>
        <AdminSection title="Label Templates" description="Configure GS1/UDI label templates by jurisdiction">
          <AdminCard title="Label Template Library" icon={Tag} actions={
            <div className="flex items-center gap-3">
              <TableSearch value={searchQuery} onChange={setSearchQuery} placeholder="Search templates..." />
              <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Create Template</Button>
            </div>
          }>
            <AdminTable columns={labelColumns} data={MOCK_LABELS} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <TabPanel id="barcodes" activeTab={activeTab}>
        <AdminSection title="Barcode Settings" description="Configure barcode formats and scanner settings">
          <AdminCard title="Barcode Configuration" icon={Barcode} actions={
            <Button onClick={() => setIsModalOpen(true)}><Plus className="w-4 h-4" />Add Configuration</Button>
          }>
            <AdminTable columns={barcodeColumns} data={MOCK_BARCODES} keyField="id" />
          </AdminCard>
        </AdminSection>
      </TabPanel>

      <AdminModal isOpen={isModalOpen} onClose={() => setIsModalOpen(false)} title="Add Item" size="lg"
        footer={<><Button variant="ghost" onClick={() => setIsModalOpen(false)}>Cancel</Button><Button onClick={() => setIsModalOpen(false)}>Save</Button></>}>
        <div className="space-y-4">
          <FormField label="Name" required><Input placeholder="Enter name" /></FormField>
          <FormField label="Code" required><Input placeholder="Enter code" /></FormField>
        </div>
      </AdminModal>
    </div>
  );
}

