'use client';

import React, { useState } from 'react';
import {
  Package,
  Plus,
  Edit2,
  Trash2,
  Calendar,
  AlertTriangle,
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
} from '@/components/admin';

// Mock data for stock solution lots
const MOCK_LOTS = [
  {
    id: '1',
    lotCode: 'LOT-2025-001',
    product: 'Cal-Mag Plus',
    productSku: 'NUT-CALMAG-1L',
    receivedAt: '2025-11-01',
    unitQty: 10,
    unitRemaining: 7,
    concentrationFactor: 1.0,
    expiryAt: '2026-11-01',
    qcStatus: 'pass',
  },
  {
    id: '2',
    lotCode: 'LOT-2025-002',
    product: 'Flora Bloom A',
    productSku: 'NUT-BLOOM-A-1L',
    receivedAt: '2025-10-15',
    unitQty: 20,
    unitRemaining: 12,
    concentrationFactor: 1.0,
    expiryAt: '2026-10-15',
    qcStatus: 'pass',
  },
  {
    id: '3',
    lotCode: 'LOT-2025-003',
    product: 'Flora Bloom B',
    productSku: 'NUT-BLOOM-B-1L',
    receivedAt: '2025-10-15',
    unitQty: 20,
    unitRemaining: 14,
    concentrationFactor: 1.0,
    expiryAt: '2026-10-15',
    qcStatus: 'pass',
  },
  {
    id: '4',
    lotCode: 'LOT-2025-004',
    product: 'PK Booster 13-14',
    productSku: 'NUT-PK-500ML',
    receivedAt: '2025-09-01',
    unitQty: 5,
    unitRemaining: 1,
    concentrationFactor: 1.0,
    expiryAt: '2025-12-01',
    qcStatus: 'pending',
  },
  {
    id: '5',
    lotCode: 'LOT-2025-005',
    product: 'Silica Blast',
    productSku: 'NUT-SILICA-1L',
    receivedAt: '2025-08-01',
    unitQty: 8,
    unitRemaining: 0,
    concentrationFactor: 1.0,
    expiryAt: '2026-08-01',
    qcStatus: 'pass',
  },
];

const PRODUCTS = [
  { value: 'calmag', label: 'Cal-Mag Plus (NUT-CALMAG-1L)' },
  { value: 'bloom-a', label: 'Flora Bloom A (NUT-BLOOM-A-1L)' },
  { value: 'bloom-b', label: 'Flora Bloom B (NUT-BLOOM-B-1L)' },
  { value: 'pk', label: 'PK Booster 13-14 (NUT-PK-500ML)' },
  { value: 'silica', label: 'Silica Blast (NUT-SILICA-1L)' },
];

const QC_STATUSES = [
  { value: 'pending', label: 'Pending' },
  { value: 'pass', label: 'Pass' },
  { value: 'fail', label: 'Fail' },
];

export function StockSolutionLotsSection() {
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingLot, setEditingLot] = useState<typeof MOCK_LOTS[0] | null>(null);

  const filteredLots = MOCK_LOTS.filter(
    (lot) =>
      lot.lotCode.toLowerCase().includes(searchQuery.toLowerCase()) ||
      lot.product.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleEdit = (lot: typeof MOCK_LOTS[0]) => {
    setEditingLot(lot);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingLot(null);
    setIsModalOpen(true);
  };

  const isExpiringSoon = (expiryAt: string) => {
    const expiry = new Date(expiryAt);
    const now = new Date();
    const daysUntilExpiry = Math.floor((expiry.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
    return daysUntilExpiry <= 30 && daysUntilExpiry > 0;
  };

  const isExpired = (expiryAt: string) => {
    return new Date(expiryAt) < new Date();
  };

  const columns = [
    {
      key: 'lotCode',
      header: 'Lot Code',
      width: '140px',
      render: (item: typeof MOCK_LOTS[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">
          {item.lotCode}
        </span>
      ),
    },
    {
      key: 'product',
      header: 'Product',
      sortable: true,
      render: (item: typeof MOCK_LOTS[0]) => (
        <div>
          <div className="font-medium text-foreground">{item.product}</div>
          <div className="text-xs text-muted-foreground">{item.productSku}</div>
        </div>
      ),
    },
    {
      key: 'received',
      header: 'Received',
      render: (item: typeof MOCK_LOTS[0]) => (
        <span className="text-sm text-muted-foreground">
          {new Date(item.receivedAt).toLocaleDateString()}
        </span>
      ),
    },
    {
      key: 'quantity',
      header: 'Quantity',
      render: (item: typeof MOCK_LOTS[0]) => (
        <div className="flex items-center gap-2">
          <div className="w-12 h-2 bg-white/10 rounded-full overflow-hidden">
            <div 
              className={`h-full rounded-full ${
                item.unitRemaining === 0 ? 'bg-rose-500' :
                item.unitRemaining / item.unitQty < 0.2 ? 'bg-amber-500' : 
                'bg-emerald-500'
              }`}
              style={{ width: `${(item.unitRemaining / item.unitQty) * 100}%` }}
            />
          </div>
          <span className="text-xs">
            {item.unitRemaining} / {item.unitQty}
          </span>
        </div>
      ),
    },
    {
      key: 'expiry',
      header: 'Expiry',
      render: (item: typeof MOCK_LOTS[0]) => (
        <div className="flex items-center gap-1.5">
          {(isExpiringSoon(item.expiryAt) || isExpired(item.expiryAt)) && (
            <AlertTriangle className={`w-3.5 h-3.5 ${isExpired(item.expiryAt) ? 'text-rose-400' : 'text-amber-400'}`} />
          )}
          <span className={`text-sm ${
            isExpired(item.expiryAt) ? 'text-rose-400' :
            isExpiringSoon(item.expiryAt) ? 'text-amber-400' :
            'text-muted-foreground'
          }`}>
            {new Date(item.expiryAt).toLocaleDateString()}
          </span>
        </div>
      ),
    },
    {
      key: 'qcStatus',
      header: 'QC Status',
      render: (item: typeof MOCK_LOTS[0]) => (
        <StatusBadge 
          status={
            item.qcStatus === 'pass' ? 'active' : 
            item.qcStatus === 'fail' ? 'error' : 
            'pending'
          }
          label={item.qcStatus.charAt(0).toUpperCase() + item.qcStatus.slice(1)}
        />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '80px',
      render: (item: typeof MOCK_LOTS[0]) => (
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
      title="Stock Solution Lots"
      description="Track inventory lots with QC status, quantities, and expiration dates"
    >
      <AdminCard
        title="Lot Inventory"
        icon={Package}
        actions={
          <div className="flex items-center gap-3">
            <TableSearch
              value={searchQuery}
              onChange={setSearchQuery}
              placeholder="Search lots..."
            />
            <Button onClick={handleCreate}>
              <Plus className="w-4 h-4" />
              Receive Lot
            </Button>
          </div>
        }
      >
        <AdminTable
          columns={columns}
          data={filteredLots}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No stock solution lots in inventory"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingLot ? 'Edit Stock Lot' : 'Receive Stock Lot'}
        description="Record incoming inventory with QC tracking"
        size="lg"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsModalOpen(false)}>
              {editingLot ? 'Save Changes' : 'Receive Lot'}
            </Button>
          </>
        }
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Lot Code" required>
              <Input placeholder="e.g., LOT-2025-001" defaultValue={editingLot?.lotCode} />
            </FormField>
            <FormField label="Product" required>
              <Select options={PRODUCTS} defaultValue="calmag" />
            </FormField>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Quantity Received" required>
              <Input type="number" placeholder="10" defaultValue={editingLot?.unitQty} />
            </FormField>
            <FormField label="Concentration Factor">
              <Input type="number" step="0.1" placeholder="1.0" defaultValue={editingLot?.concentrationFactor} />
            </FormField>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Received Date" required>
              <Input type="date" defaultValue={editingLot?.receivedAt} />
            </FormField>
            <FormField label="Expiry Date" required>
              <Input type="date" defaultValue={editingLot?.expiryAt} />
            </FormField>
          </div>

          <FormField label="QC Status" required>
            <Select options={QC_STATUSES} defaultValue={editingLot?.qcStatus || 'pending'} />
          </FormField>
        </div>
      </AdminModal>
    </AdminSection>
  );
}

