'use client';

import React, { useEffect, useState, useCallback } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import {
  ChevronLeft,
  Dna,
  Edit2,
  Trash2,
  FlaskConical,
  Leaf,
  Droplets,
  ThermometerSun,
  Wind,
  Sun,
  Beaker,
  TrendingUp,
  Calendar,
  Clock,
  Eye,
  Building2,
  Palette,
  Flower2,
  Scissors,
  FileText,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { useGeneticsStore } from '@/features/genetics/stores';
import { GeneticsModal } from '@/features/genetics/components';
import {
  GENETIC_TYPE_CONFIG,
  YIELD_POTENTIAL_CONFIG,
  LEAF_SHAPE_CONFIG,
  BUD_STRUCTURE_CONFIG,
  TRICHOME_DENSITY_CONFIG,
  AROMA_INTENSITY_CONFIG,
  COLA_STRUCTURE_CONFIG,
  CANOPY_BEHAVIOR_CONFIG,
  TRAINING_RESPONSE_CONFIG,
  type Genetics,
  type UpdateGeneticsRequest,
} from '@/features/genetics/types';

// =============================================================================
// HELPER COMPONENTS
// =============================================================================

function StatCard({
  label,
  value,
  subValue,
  icon: Icon,
  color,
}: {
  label: string;
  value: string | number;
  subValue?: string;
  icon: React.ElementType;
  color: string;
}) {
  return (
    <div className="bg-surface border border-border rounded-xl p-4">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-sm text-muted-foreground">{label}</p>
          <p className="text-2xl font-bold text-foreground mt-1">{value}</p>
          {subValue && (
            <p className="text-xs text-muted-foreground mt-0.5">{subValue}</p>
          )}
        </div>
        <div
          className="w-10 h-10 rounded-lg flex items-center justify-center"
          style={{ backgroundColor: `${color}15` }}
        >
          <Icon className="w-5 h-5" style={{ color }} />
        </div>
      </div>
    </div>
  );
}

function SectionCard({
  title,
  icon: Icon,
  children,
}: {
  title: string;
  icon: React.ElementType;
  children: React.ReactNode;
}) {
  return (
    <div className="bg-surface border border-border rounded-xl overflow-hidden">
      <div className="flex items-center gap-2 px-4 py-3 border-b border-border bg-muted/20">
        <Icon className="w-4 h-4 text-emerald-400" />
        <h3 className="text-sm font-semibold text-foreground">{title}</h3>
      </div>
      <div className="p-4">{children}</div>
    </div>
  );
}

function CannabinoidBar({
  label,
  min,
  max,
  color,
  maxRange = 30,
}: {
  label: string;
  min: number;
  max: number;
  color: string;
  maxRange?: number;
}) {
  const minPercent = (min / maxRange) * 100;
  const maxPercent = (max / maxRange) * 100;

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between text-sm">
        <span className="text-muted-foreground">{label}</span>
        <span className="font-medium text-foreground">
          {min === max ? `${min}%` : `${min}% - ${max}%`}
        </span>
      </div>
      <div className="h-3 bg-muted/30 rounded-full overflow-hidden relative">
        <div
          className="absolute h-full rounded-full transition-all"
          style={{
            left: `${minPercent}%`,
            width: `${maxPercent - minPercent + 2}%`,
            backgroundColor: color,
          }}
        />
      </div>
      <div className="flex justify-between text-xs text-muted-foreground">
        <span>0%</span>
        <span>{maxRange}%</span>
      </div>
    </div>
  );
}

function PropertyRow({
  label,
  value,
}: {
  label: string;
  value: string | number | undefined | null;
}) {
  if (!value) return null;
  return (
    <div className="flex items-center justify-between py-2 border-b border-border/50 last:border-0">
      <span className="text-sm text-muted-foreground">{label}</span>
      <span className="text-sm font-medium text-foreground capitalize">{value}</span>
    </div>
  );
}

function TerpeneTag({ name, value }: { name: string; value: number }) {
  return (
    <div className="flex items-center gap-2 px-3 py-2 bg-emerald-500/10 border border-emerald-500/20 rounded-lg">
      <Droplets className="w-4 h-4 text-emerald-400" />
      <span className="text-sm font-medium text-emerald-400 capitalize">{name}</span>
      {value > 0 && (
        <span className="text-xs text-emerald-400/70">{(value * 100).toFixed(0)}%</span>
      )}
    </div>
  );
}

// =============================================================================
// MAIN COMPONENT
// =============================================================================

export default function GeneticsDetailPage() {
  const params = useParams();
  const router = useRouter();
  const geneticsId = params.id as string;

  // Store
  const {
    genetics: allGenetics,
    geneticsLoading,
    loadGenetics,
    updateGenetics,
    deleteGenetics,
    setSiteId,
  } = useGeneticsStore();

  // Local state
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Find the genetics
  const genetics = allGenetics.find((g) => g.id === geneticsId);

  // Load genetics on mount
  useEffect(() => {
    setSiteId('site-1');
  }, [setSiteId]);

  // Handle edit submit
  const handleEditSubmit = useCallback(async (data: UpdateGeneticsRequest) => {
    setIsSubmitting(true);
    try {
      await updateGenetics(geneticsId, data);
      setIsEditModalOpen(false);
    } catch (error) {
      console.error('Failed to update genetics:', error);
    } finally {
      setIsSubmitting(false);
    }
  }, [geneticsId, updateGenetics]);

  // Handle delete
  const handleDelete = useCallback(async () => {
    if (genetics && confirm(`Are you sure you want to delete "${genetics.name}"? This action cannot be undone.`)) {
      try {
        await deleteGenetics(geneticsId);
        router.push('/library/genetics');
      } catch (error) {
        console.error('Failed to delete genetics:', error);
      }
    }
  }, [genetics, geneticsId, deleteGenetics, router]);

  // Loading state
  if (geneticsLoading && !genetics) {
    return (
      <div className="flex items-center justify-center h-full">
        <div className="flex flex-col items-center gap-3">
          <div className="w-8 h-8 border-2 border-emerald-500/30 border-t-emerald-500 rounded-full animate-spin" />
          <p className="text-sm text-muted-foreground">Loading genetics...</p>
        </div>
      </div>
    );
  }

  // Not found state
  if (!genetics) {
    return (
      <div className="flex flex-col items-center justify-center h-full text-center p-6">
        <Dna className="w-12 h-12 text-muted-foreground mb-4" />
        <h2 className="text-lg font-semibold text-foreground mb-2">Genetics not found</h2>
        <p className="text-sm text-muted-foreground mb-4">
          The genetics you're looking for doesn't exist or has been deleted.
        </p>
        <Link
          href="/library/genetics"
          className="flex items-center gap-2 text-emerald-400 hover:text-emerald-300"
        >
          <ChevronLeft className="w-4 h-4" />
          Back to Genetics Library
        </Link>
      </div>
    );
  }

  const typeConfig = GENETIC_TYPE_CONFIG[genetics.geneticType];
  const yieldConfig = YIELD_POTENTIAL_CONFIG[genetics.yieldPotential];

  return (
    <div className="flex flex-col h-full">
      {/* Header */}
      <div className="px-6 py-4 border-b border-border bg-surface/30">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Link
              href="/library/genetics"
              className="p-2 rounded-lg hover:bg-muted/50 text-muted-foreground hover:text-foreground transition-colors"
            >
              <ChevronLeft className="w-5 h-5" />
            </Link>
            <div
              className="w-12 h-12 rounded-xl flex items-center justify-center"
              style={{ backgroundColor: `${typeConfig.color}15` }}
            >
              <Dna className="w-6 h-6" style={{ color: typeConfig.color }} />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h1 className="text-xl font-bold text-foreground">{genetics.name}</h1>
                <span
                  className="px-2 py-0.5 text-xs font-medium rounded-full"
                  style={{
                    backgroundColor: `${typeConfig.color}20`,
                    color: typeConfig.color,
                  }}
                >
                  {typeConfig.label}
                </span>
              </div>
              <p className="text-sm text-muted-foreground mt-0.5 line-clamp-1 max-w-xl">
                {genetics.description}
              </p>
            </div>
          </div>

          {/* Actions */}
          <div className="flex items-center gap-2">
            <button
              onClick={() => setIsEditModalOpen(true)}
              className="flex items-center gap-2 px-4 py-2 rounded-lg bg-muted/50 text-foreground hover:bg-muted transition-colors"
            >
              <Edit2 className="w-4 h-4" />
              Edit
            </button>
            <button
              onClick={handleDelete}
              className="flex items-center gap-2 px-4 py-2 rounded-lg bg-rose-500/10 text-rose-400 hover:bg-rose-500/20 transition-colors"
            >
              <Trash2 className="w-4 h-4" />
              Delete
            </button>
          </div>
        </div>
      </div>

      {/* Content */}
      <div className="flex-1 overflow-auto p-6">
        <div className="max-w-5xl mx-auto space-y-6">
          {/* Stats Grid */}
          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            <StatCard
              label="THC Range"
              value={genetics.thcMin === genetics.thcMax 
                ? `${genetics.thcMin}%` 
                : `${genetics.thcMin}-${genetics.thcMax}%`}
              icon={FlaskConical}
              color="#10B981"
            />
            <StatCard
              label="CBD Range"
              value={genetics.cbdMin === genetics.cbdMax 
                ? `${genetics.cbdMin}%` 
                : `${genetics.cbdMin}-${genetics.cbdMax}%`}
              icon={Beaker}
              color="#8B5CF6"
            />
            <StatCard
              label="Flowering Time"
              value={genetics.floweringTimeDays ? `${genetics.floweringTimeDays}` : 'Variable'}
              subValue={genetics.floweringTimeDays ? 'days' : undefined}
              icon={Calendar}
              color="#F59E0B"
            />
            <StatCard
              label="Yield Potential"
              value={yieldConfig.label}
              icon={TrendingUp}
              color={yieldConfig.color}
            />
          </div>

          {/* Main Content Grid */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Left Column */}
            <div className="space-y-6">
              {/* Description */}
              <SectionCard title="Description" icon={Dna}>
                <p className="text-sm text-foreground/80 leading-relaxed">
                  {genetics.description}
                </p>
                {genetics.breedingNotes && (
                  <div className="mt-4 pt-4 border-t border-border">
                    <p className="text-xs font-medium text-muted-foreground mb-1">Breeding Notes</p>
                    <p className="text-sm text-foreground/70">{genetics.breedingNotes}</p>
                  </div>
                )}
              </SectionCard>

              {/* Cannabinoid Profile */}
              <SectionCard title="Cannabinoid Profile" icon={FlaskConical}>
                <div className="space-y-6">
                  <CannabinoidBar
                    label="THC"
                    min={genetics.thcMin}
                    max={genetics.thcMax}
                    color="#10B981"
                  />
                  <CannabinoidBar
                    label="CBD"
                    min={genetics.cbdMin}
                    max={genetics.cbdMax}
                    color="#8B5CF6"
                  />
                </div>
              </SectionCard>

              {/* Terpene Profile */}
              {genetics.terpeneProfile && (
                <SectionCard title="Terpene Profile" icon={Droplets}>
                  {genetics.terpeneProfile.dominantTerpenes && 
                   Object.keys(genetics.terpeneProfile.dominantTerpenes).length > 0 ? (
                    <div className="space-y-4">
                      <div className="flex flex-wrap gap-2">
                        {Object.entries(genetics.terpeneProfile.dominantTerpenes).map(
                          ([name, value]) => (
                            <TerpeneTag key={name} name={name} value={value} />
                          )
                        )}
                      </div>
                      
                      {genetics.terpeneProfile.aromaDescriptors && 
                       genetics.terpeneProfile.aromaDescriptors.length > 0 && (
                        <div>
                          <p className="text-xs font-medium text-muted-foreground mb-2">Aroma</p>
                          <div className="flex flex-wrap gap-1.5">
                            {genetics.terpeneProfile.aromaDescriptors.map((desc) => (
                              <span
                                key={desc}
                                className="px-2 py-1 text-xs bg-muted/50 text-foreground/70 rounded capitalize"
                              >
                                {desc}
                              </span>
                            ))}
                          </div>
                        </div>
                      )}

                      {genetics.terpeneProfile.overallProfile && (
                        <p className="text-sm text-foreground/70 italic">
                          "{genetics.terpeneProfile.overallProfile}"
                        </p>
                      )}
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground">No terpene data available</p>
                  )}
                </SectionCard>
              )}

              {/* Visual Characteristics */}
              {genetics.visualCharacteristics && 
               Object.values(genetics.visualCharacteristics).some(Boolean) && (
                <SectionCard title="Visual Characteristics" icon={Palette}>
                  <div className="space-y-4">
                    <div className="space-y-1">
                      {genetics.visualCharacteristics.leafShape && (
                        <PropertyRow
                          label="Leaf Shape"
                          value={LEAF_SHAPE_CONFIG[genetics.visualCharacteristics.leafShape]?.label}
                        />
                      )}
                      {genetics.visualCharacteristics.budStructure && (
                        <PropertyRow
                          label="Bud Structure"
                          value={BUD_STRUCTURE_CONFIG[genetics.visualCharacteristics.budStructure]?.label}
                        />
                      )}
                      {genetics.visualCharacteristics.trichomeDensity && (
                        <PropertyRow
                          label="Trichome Density"
                          value={TRICHOME_DENSITY_CONFIG[genetics.visualCharacteristics.trichomeDensity]?.label}
                        />
                      )}
                      {genetics.visualCharacteristics.pistilColor && (
                        <PropertyRow
                          label="Pistil Color"
                          value={genetics.visualCharacteristics.pistilColor}
                        />
                      )}
                    </div>

                    {genetics.visualCharacteristics.primaryColors && 
                     genetics.visualCharacteristics.primaryColors.length > 0 && (
                      <div>
                        <p className="text-xs font-medium text-muted-foreground mb-2">Primary Colors</p>
                        <div className="flex flex-wrap gap-1.5">
                          {genetics.visualCharacteristics.primaryColors.map((color) => (
                            <span
                              key={color}
                              className="px-2 py-1 text-xs bg-violet-500/10 text-violet-400 rounded capitalize"
                            >
                              {color}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}
                  </div>
                </SectionCard>
              )}

              {/* Aroma Profile (Phenotype) */}
              {genetics.aromaProfile && 
               Object.values(genetics.aromaProfile).some(Boolean) && (
                <SectionCard title="Aroma Profile" icon={Flower2}>
                  <div className="space-y-4">
                    {genetics.aromaProfile.primaryScents && 
                     genetics.aromaProfile.primaryScents.length > 0 && (
                      <div>
                        <p className="text-xs font-medium text-muted-foreground mb-2">Primary Scents</p>
                        <div className="flex flex-wrap gap-1.5">
                          {genetics.aromaProfile.primaryScents.map((scent) => (
                            <span
                              key={scent}
                              className="px-2 py-1 text-xs bg-amber-500/10 text-amber-400 rounded capitalize"
                            >
                              {scent}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}

                    {genetics.aromaProfile.secondaryNotes && 
                     genetics.aromaProfile.secondaryNotes.length > 0 && (
                      <div>
                        <p className="text-xs font-medium text-muted-foreground mb-2">Secondary Notes</p>
                        <div className="flex flex-wrap gap-1.5">
                          {genetics.aromaProfile.secondaryNotes.map((note) => (
                            <span
                              key={note}
                              className="px-2 py-1 text-xs bg-muted/50 text-foreground/70 rounded capitalize"
                            >
                              {note}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}

                    {genetics.aromaProfile.intensity && (
                      <PropertyRow
                        label="Intensity"
                        value={AROMA_INTENSITY_CONFIG[genetics.aromaProfile.intensity]?.label}
                      />
                    )}

                    {genetics.aromaProfile.developmentNotes && (
                      <p className="text-sm text-foreground/70 italic">
                        "{genetics.aromaProfile.developmentNotes}"
                      </p>
                    )}
                  </div>
                </SectionCard>
              )}
            </div>

            {/* Right Column */}
            <div className="space-y-6">
              {/* Growth Characteristics */}
              <SectionCard title="Growth Characteristics" icon={Leaf}>
                {genetics.growthCharacteristics && 
                 Object.values(genetics.growthCharacteristics).some(Boolean) ? (
                  <div className="space-y-1">
                    <PropertyRow
                      label="Stretch Tendency"
                      value={genetics.growthCharacteristics.stretchTendency}
                    />
                    <PropertyRow
                      label="Branching Pattern"
                      value={genetics.growthCharacteristics.branchingPattern}
                    />
                    <PropertyRow
                      label="Leaf Morphology"
                      value={genetics.growthCharacteristics.leafMorphology}
                    />
                    <PropertyRow
                      label="Internode Spacing"
                      value={genetics.growthCharacteristics.internodeSpacing}
                    />
                    <PropertyRow
                      label="Root Vigour"
                      value={genetics.growthCharacteristics.rootVigour}
                    />
                    <PropertyRow
                      label="Nutrient Sensitivity"
                      value={genetics.growthCharacteristics.nutrientSensitivity}
                    />
                    <PropertyRow
                      label="Light Intensity Preference"
                      value={genetics.growthCharacteristics.lightIntensityPreference}
                    />
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">No growth data available</p>
                )}
              </SectionCard>

              {/* Environmental Preferences */}
              <SectionCard title="Environmental Preferences" icon={ThermometerSun}>
                {genetics.growthCharacteristics && (
                  genetics.growthCharacteristics.optimalTemperatureMin || 
                  genetics.growthCharacteristics.optimalHumidityMin
                ) ? (
                  <div className="space-y-4">
                    {genetics.growthCharacteristics.optimalTemperatureMin && (
                      <div className="flex items-center gap-4">
                        <div className="w-10 h-10 rounded-lg bg-orange-500/10 flex items-center justify-center">
                          <Sun className="w-5 h-5 text-orange-400" />
                        </div>
                        <div>
                          <p className="text-xs text-muted-foreground">Optimal Temperature</p>
                          <p className="text-sm font-medium text-foreground">
                            {genetics.growthCharacteristics.optimalTemperatureMin}°F -{' '}
                            {genetics.growthCharacteristics.optimalTemperatureMax}°F
                          </p>
                        </div>
                      </div>
                    )}
                    {genetics.growthCharacteristics.optimalHumidityMin && (
                      <div className="flex items-center gap-4">
                        <div className="w-10 h-10 rounded-lg bg-blue-500/10 flex items-center justify-center">
                          <Wind className="w-5 h-5 text-blue-400" />
                        </div>
                        <div>
                          <p className="text-xs text-muted-foreground">Optimal Humidity</p>
                          <p className="text-sm font-medium text-foreground">
                            {genetics.growthCharacteristics.optimalHumidityMin}% -{' '}
                            {genetics.growthCharacteristics.optimalHumidityMax}% RH
                          </p>
                        </div>
                      </div>
                    )}
                  </div>
                ) : (
                  <p className="text-sm text-muted-foreground">No environmental preferences set</p>
                )}
              </SectionCard>

              {/* Growth Pattern */}
              {genetics.growthPattern && 
               Object.values(genetics.growthPattern).some(Boolean) && (
                <SectionCard title="Growth Pattern" icon={Scissors}>
                  <div className="space-y-4">
                    <div className="space-y-1">
                      {genetics.growthPattern.colaStructure && (
                        <PropertyRow
                          label="Cola Structure"
                          value={COLA_STRUCTURE_CONFIG[genetics.growthPattern.colaStructure]?.label}
                        />
                      )}
                      {genetics.growthPattern.canopyBehavior && (
                        <PropertyRow
                          label="Canopy Behavior"
                          value={CANOPY_BEHAVIOR_CONFIG[genetics.growthPattern.canopyBehavior]?.label}
                        />
                      )}
                      {genetics.growthPattern.trainingResponse && (
                        <PropertyRow
                          label="Training Response"
                          value={TRAINING_RESPONSE_CONFIG[genetics.growthPattern.trainingResponse]?.label}
                        />
                      )}
                      {genetics.growthPattern.internodeLength && (
                        <PropertyRow
                          label="Internode Length"
                          value={genetics.growthPattern.internodeLength}
                        />
                      )}
                      {genetics.growthPattern.lateralBranching && (
                        <PropertyRow
                          label="Lateral Branching"
                          value={genetics.growthPattern.lateralBranching}
                        />
                      )}
                    </div>

                    {genetics.growthPattern.preferredTrainingMethods && 
                     genetics.growthPattern.preferredTrainingMethods.length > 0 && (
                      <div>
                        <p className="text-xs font-medium text-muted-foreground mb-2">Preferred Training Methods</p>
                        <div className="flex flex-wrap gap-1.5">
                          {genetics.growthPattern.preferredTrainingMethods.map((method) => (
                            <span
                              key={method}
                              className="px-2 py-1 text-xs bg-cyan-500/10 text-cyan-400 rounded"
                            >
                              {method}
                            </span>
                          ))}
                        </div>
                      </div>
                    )}

                    {genetics.growthPattern.additionalNotes && (
                      <p className="text-sm text-foreground/70 italic">
                        "{genetics.growthPattern.additionalNotes}"
                      </p>
                    )}
                  </div>
                </SectionCard>
              )}

              {/* Expression Notes */}
              {genetics.expressionNotes && (
                <SectionCard title="Expression Notes" icon={Eye}>
                  <p className="text-sm text-foreground/80 leading-relaxed">
                    {genetics.expressionNotes}
                  </p>
                </SectionCard>
              )}

              {/* Source Information */}
              {(genetics.breeder || genetics.seedBank || genetics.cultivationNotes) && (
                <SectionCard title="Source & Cultivation" icon={Building2}>
                  <div className="space-y-4">
                    {(genetics.breeder || genetics.seedBank) && (
                      <div className="space-y-1">
                        {genetics.breeder && (
                          <PropertyRow label="Breeder" value={genetics.breeder} />
                        )}
                        {genetics.seedBank && (
                          <PropertyRow label="Seed Bank" value={genetics.seedBank} />
                        )}
                      </div>
                    )}

                    {genetics.cultivationNotes && (
                      <div className="pt-2 border-t border-border">
                        <p className="text-xs font-medium text-muted-foreground mb-2">Cultivation Notes</p>
                        <p className="text-sm text-foreground/80 leading-relaxed">
                          {genetics.cultivationNotes}
                        </p>
                      </div>
                    )}
                  </div>
                </SectionCard>
              )}

              {/* Metadata */}
              <SectionCard title="Metadata" icon={Clock}>
                <div className="space-y-1 text-sm">
                  <div className="flex items-center justify-between py-2 border-b border-border/50">
                    <span className="text-muted-foreground">Created</span>
                    <span className="text-foreground">
                      {new Date(genetics.createdAt).toLocaleDateString('en-US', {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric',
                      })}
                    </span>
                  </div>
                  <div className="flex items-center justify-between py-2">
                    <span className="text-muted-foreground">Last Updated</span>
                    <span className="text-foreground">
                      {new Date(genetics.updatedAt).toLocaleDateString('en-US', {
                        year: 'numeric',
                        month: 'short',
                        day: 'numeric',
                      })}
                    </span>
                  </div>
                </div>
              </SectionCard>
            </div>
          </div>
        </div>
      </div>

      {/* Edit Modal */}
      <GeneticsModal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        onSubmit={handleEditSubmit}
        genetics={genetics}
        isLoading={isSubmitting}
      />
    </div>
  );
}


