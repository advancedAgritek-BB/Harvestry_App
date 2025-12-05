'use client';

import React, { useState } from 'react';
import {
  FlaskConical,
  Plus,
  Edit2,
  Trash2,
  Copy,
  Lock,
  History,
  ChevronRight,
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
  Switch,
  Textarea,
} from '@/components/admin';

// Mock data for feed recipes
const MOCK_RECIPES = [
  {
    id: '1',
    code: 'FLW-W4-6',
    name: 'Flower Week 4-6',
    version: 'v2.1',
    versions: 3,
    description: 'High P/K boost for mid-flower',
    components: ['Cal-Mag (2mL/L)', 'Bloom A (3mL/L)', 'Bloom B (3mL/L)', 'PK Boost (1mL/L)'],
    targetEc: '2.4 mS/cm',
    targetPh: '5.9',
    author: 'Brandon B.',
    locked: true,
    active: true,
  },
  {
    id: '2',
    code: 'FLW-W7-9',
    name: 'Flower Week 7-9',
    version: 'v2.0',
    versions: 2,
    description: 'Late flower ripening formula',
    components: ['Cal-Mag (1.5mL/L)', 'Bloom A (2.5mL/L)', 'Bloom B (2.5mL/L)', 'Ripen (2mL/L)'],
    targetEc: '2.0 mS/cm',
    targetPh: '6.0',
    author: 'Sarah M.',
    locked: true,
    active: true,
  },
  {
    id: '3',
    code: 'VEG-STD',
    name: 'Veg Standard',
    version: 'v1.3',
    versions: 4,
    description: 'Balanced veg growth formula',
    components: ['Cal-Mag (2mL/L)', 'Grow A (2mL/L)', 'Grow B (2mL/L)', 'Silica (0.5mL/L)'],
    targetEc: '1.8 mS/cm',
    targetPh: '5.8',
    author: 'Mike T.',
    locked: false,
    active: true,
  },
  {
    id: '4',
    code: 'FLUSH',
    name: 'Flush Only',
    version: 'v1.0',
    versions: 1,
    description: 'Plain water flush',
    components: ['RO Water only'],
    targetEc: '< 0.3 mS/cm',
    targetPh: '6.0',
    author: 'Brandon B.',
    locked: true,
    active: true,
  },
];

export function FeedRecipesSection() {
  const [searchQuery, setSearchQuery] = useState('');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingRecipe, setEditingRecipe] = useState<typeof MOCK_RECIPES[0] | null>(null);

  const filteredRecipes = MOCK_RECIPES.filter(
    (recipe) =>
      recipe.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      recipe.code.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleEdit = (recipe: typeof MOCK_RECIPES[0]) => {
    setEditingRecipe(recipe);
    setIsModalOpen(true);
  };

  const handleCreate = () => {
    setEditingRecipe(null);
    setIsModalOpen(true);
  };

  const columns = [
    {
      key: 'code',
      header: 'Code',
      width: '100px',
      render: (item: typeof MOCK_RECIPES[0]) => (
        <span className="font-mono text-xs bg-white/5 px-2 py-0.5 rounded">
          {item.code}
        </span>
      ),
    },
    {
      key: 'name',
      header: 'Recipe Name',
      sortable: true,
      render: (item: typeof MOCK_RECIPES[0]) => (
        <div>
          <div className="flex items-center gap-2">
            <span className="font-medium text-foreground">{item.name}</span>
            {item.locked && <Lock className="w-3 h-3 text-amber-400" />}
          </div>
          <div className="text-xs text-muted-foreground">{item.description}</div>
        </div>
      ),
    },
    {
      key: 'version',
      header: 'Version',
      render: (item: typeof MOCK_RECIPES[0]) => (
        <div className="flex items-center gap-2">
          <span className="text-sm text-cyan-400">{item.version}</span>
          <span className="text-xs text-muted-foreground">({item.versions} total)</span>
        </div>
      ),
    },
    {
      key: 'components',
      header: 'Components',
      render: (item: typeof MOCK_RECIPES[0]) => (
        <div className="text-xs text-muted-foreground max-w-[200px] truncate">
          {item.components.join(', ')}
        </div>
      ),
    },
    {
      key: 'targets',
      header: 'Targets',
      render: (item: typeof MOCK_RECIPES[0]) => (
        <div className="text-xs space-y-0.5">
          <div>EC: <span className="text-emerald-400">{item.targetEc}</span></div>
          <div>pH: <span className="text-violet-400">{item.targetPh}</span></div>
        </div>
      ),
    },
    {
      key: 'author',
      header: 'Author',
      render: (item: typeof MOCK_RECIPES[0]) => (
        <span className="text-sm text-muted-foreground">{item.author}</span>
      ),
    },
    {
      key: 'status',
      header: 'Status',
      render: (item: typeof MOCK_RECIPES[0]) => (
        <StatusBadge status={item.active ? 'active' : 'inactive'} />
      ),
    },
    {
      key: 'actions',
      header: '',
      width: '120px',
      render: (item: typeof MOCK_RECIPES[0]) => (
        <TableActions>
          <TableActionButton onClick={() => {}}>
            <History className="w-4 h-4" />
          </TableActionButton>
          <TableActionButton onClick={() => handleEdit(item)}>
            <Edit2 className="w-4 h-4" />
          </TableActionButton>
          <TableActionButton onClick={() => {}}>
            <Copy className="w-4 h-4" />
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
      title="Feed Recipes"
      description="Manage versioned nutrient recipes with component ratios and target values"
    >
      <AdminCard
        title="Recipe Library"
        icon={FlaskConical}
        actions={
          <div className="flex items-center gap-3">
            <TableSearch
              value={searchQuery}
              onChange={setSearchQuery}
              placeholder="Search recipes..."
            />
            <Button onClick={handleCreate}>
              <Plus className="w-4 h-4" />
              Create Recipe
            </Button>
          </div>
        }
      >
        <div className="text-xs text-muted-foreground mb-4 p-3 bg-white/5 rounded-lg">
          <strong>Version Control:</strong> Recipes are immutable once locked. Create a new version 
          to make changes. Locked recipes cannot be modified or deleted.
        </div>
        <AdminTable
          columns={columns}
          data={filteredRecipes}
          keyField="id"
          onRowClick={handleEdit}
          emptyMessage="No feed recipes configured"
        />
      </AdminCard>

      {/* Create/Edit Modal */}
      <AdminModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingRecipe ? `Edit Recipe: ${editingRecipe.name}` : 'Create Feed Recipe'}
        description={editingRecipe?.locked ? 'This recipe is locked. Create a new version to make changes.' : 'Define nutrient components and target values'}
        size="xl"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsModalOpen(false)}>
              Cancel
            </Button>
            {editingRecipe?.locked ? (
              <Button onClick={() => setIsModalOpen(false)}>
                Create New Version
              </Button>
            ) : (
              <Button onClick={() => setIsModalOpen(false)}>
                {editingRecipe ? 'Save Changes' : 'Create Recipe'}
              </Button>
            )}
          </>
        }
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Recipe Code" required>
              <Input 
                placeholder="e.g., FLW-W4-6" 
                defaultValue={editingRecipe?.code}
                disabled={editingRecipe?.locked}
              />
            </FormField>
            <FormField label="Recipe Name" required>
              <Input 
                placeholder="e.g., Flower Week 4-6" 
                defaultValue={editingRecipe?.name}
                disabled={editingRecipe?.locked}
              />
            </FormField>
          </div>

          <FormField label="Description">
            <Textarea 
              rows={2}
              placeholder="Brief description of the recipe's purpose"
              defaultValue={editingRecipe?.description}
              disabled={editingRecipe?.locked}
            />
          </FormField>

          <div className="p-4 bg-white/5 rounded-lg space-y-4">
            <div className="flex items-center justify-between">
              <h4 className="text-sm font-medium text-foreground">Recipe Components</h4>
              {!editingRecipe?.locked && (
                <Button size="sm" variant="ghost">
                  <Plus className="w-4 h-4" />
                  Add Component
                </Button>
              )}
            </div>
            <div className="space-y-2">
              {(editingRecipe?.components || []).map((comp, idx) => (
                <div key={idx} className="flex items-center gap-3 p-2 bg-white/5 rounded-lg">
                  <ChevronRight className="w-4 h-4 text-muted-foreground" />
                  <span className="flex-1 text-sm">{comp}</span>
                  {!editingRecipe?.locked && (
                    <button className="text-rose-400 hover:text-rose-300">
                      <Trash2 className="w-4 h-4" />
                    </button>
                  )}
                </div>
              ))}
              {!editingRecipe && (
                <div className="text-center py-4 text-sm text-muted-foreground">
                  No components added yet. Click "Add Component" to start.
                </div>
              )}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <FormField label="Target EC (mS/cm)" required>
              <Input 
                placeholder="e.g., 2.4" 
                defaultValue={editingRecipe?.targetEc?.replace(/[^0-9.]/g, '')}
                disabled={editingRecipe?.locked}
              />
            </FormField>
            <FormField label="Target pH" required>
              <Input 
                placeholder="e.g., 5.9" 
                defaultValue={editingRecipe?.targetPh}
                disabled={editingRecipe?.locked}
              />
            </FormField>
          </div>

          {!editingRecipe?.locked && (
            <div className="flex items-center justify-between p-3 bg-amber-500/10 border border-amber-500/20 rounded-lg">
              <div>
                <div className="text-sm font-medium text-amber-200">Lock Recipe</div>
                <div className="text-xs text-amber-200/70">
                  Locked recipes cannot be modified. Lock when ready for production use.
                </div>
              </div>
              <Switch checked={false} onChange={() => {}} />
            </div>
          )}
        </div>
      </AdminModal>
    </AdminSection>
  );
}

