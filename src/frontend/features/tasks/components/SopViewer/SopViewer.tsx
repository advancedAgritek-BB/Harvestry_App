'use client';

/**
 * SopViewer Component
 * Interactive SOP viewer with checklist functionality and image support
 */

import { useState, useCallback } from 'react';
import { 
  CheckCircle2, Circle, Clock, AlertTriangle, 
  Lightbulb, ChevronDown, ChevronUp, Image as ImageIcon,
  Package, Shield
} from 'lucide-react';
import type { StandardOperatingProcedure, SopStep, SopSubStep, SopProgress } from '../../types/sop.types';

interface SopViewerProps {
  sop: StandardOperatingProcedure;
  progress?: SopProgress;
  onStepComplete?: (stepId: string) => void;
  onSubStepComplete?: (stepId: string, subStepId: string) => void;
  readOnly?: boolean;
}

export function SopViewer({
  sop,
  progress,
  onStepComplete,
  onSubStepComplete,
  readOnly = false,
}: SopViewerProps) {
  const [expandedStepId, setExpandedStepId] = useState<string | null>(
    sop.steps?.[0]?.id || null
  );
  const [imageModalUrl, setImageModalUrl] = useState<string | null>(null);

  const isStepCompleted = useCallback((stepId: string) => {
    return progress?.completedStepIds?.includes(stepId) ?? false;
  }, [progress]);

  const isSubStepCompleted = useCallback((subStepId: string) => {
    return progress?.completedSubStepIds?.includes(subStepId) ?? false;
  }, [progress]);

  const getStepProgress = useCallback((step: SopStep) => {
    if (!step.subSteps?.length) return isStepCompleted(step.id) ? 100 : 0;
    const completed = step.subSteps.filter(s => isSubStepCompleted(s.id)).length;
    return Math.round((completed / step.subSteps.length) * 100);
  }, [isStepCompleted, isSubStepCompleted]);

  const totalProgress = useCallback(() => {
    if (!sop.steps?.length) return 0;
    const totalSteps = sop.steps.reduce((acc, step) => 
      acc + (step.subSteps?.length || 1), 0);
    const completedSteps = sop.steps.reduce((acc, step) => {
      if (step.subSteps?.length) {
        return acc + step.subSteps.filter(s => isSubStepCompleted(s.id)).length;
      }
      return acc + (isStepCompleted(step.id) ? 1 : 0);
    }, 0);
    return Math.round((completedSteps / totalSteps) * 100);
  }, [sop.steps, isStepCompleted, isSubStepCompleted]);

  // If no structured steps, show legacy content
  if (!sop.steps?.length && sop.content) {
    return (
      <div className="space-y-4">
        <div 
          className="prose prose-sm dark:prose-invert max-w-none text-[var(--text-primary)]"
          dangerouslySetInnerHTML={{ __html: sop.content }}
        />
      </div>
    );
  }

  if (!sop.steps?.length) {
    return (
      <div className="text-center py-8 text-[var(--text-muted)]">
        No steps defined for this SOP
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Header with Progress */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h3 className="font-semibold text-[var(--text-primary)]">{sop.title}</h3>
          {sop.description && (
            <p className="text-sm text-[var(--text-muted)] mt-1">{sop.description}</p>
          )}
        </div>
        <div className="flex items-center gap-4">
          {sop.estimatedTotalMinutes && (
            <div className="flex items-center gap-1.5 text-sm text-[var(--text-muted)]">
              <Clock className="w-4 h-4" />
              <span>~{sop.estimatedTotalMinutes} min</span>
            </div>
          )}
          <div className="flex items-center gap-2">
            <div className="w-24 h-2 bg-[var(--bg-tile)] rounded-full overflow-hidden">
              <div 
                className="h-full bg-[var(--accent-emerald)] transition-all duration-300"
                style={{ width: `${totalProgress()}%` }}
              />
            </div>
            <span className="text-sm font-medium text-[var(--text-primary)]">
              {totalProgress()}%
            </span>
          </div>
        </div>
      </div>

      {/* Equipment & Safety Notes */}
      {(sop.requiredEquipment?.length || sop.safetyNotes?.length) && (
        <div className="grid grid-cols-2 gap-4 mb-6">
          {sop.requiredEquipment?.length && (
            <div className="task-card p-4 rounded-lg">
              <div className="flex items-center gap-2 text-[var(--text-muted)] mb-2">
                <Package className="w-4 h-4" />
                <span className="text-xs font-medium uppercase">Required Equipment</span>
              </div>
              <ul className="space-y-1">
                {sop.requiredEquipment.map((item, i) => (
                  <li key={i} className="text-sm text-[var(--text-primary)] flex items-center gap-2">
                    <span className="w-1.5 h-1.5 rounded-full bg-[var(--accent-cyan)]" />
                    {item}
                  </li>
                ))}
              </ul>
            </div>
          )}
          {sop.safetyNotes?.length && (
            <div className="task-card p-4 rounded-lg border-l-2 border-amber-500">
              <div className="flex items-center gap-2 text-amber-400 mb-2">
                <Shield className="w-4 h-4" />
                <span className="text-xs font-medium uppercase">Safety Notes</span>
              </div>
              <ul className="space-y-1">
                {sop.safetyNotes.map((note, i) => (
                  <li key={i} className="text-sm text-[var(--text-primary)]">{note}</li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}

      {/* Steps */}
      <div className="space-y-3">
        {sop.steps.map((step, index) => (
          <StepCard
            key={step.id}
            step={step}
            stepNumber={index + 1}
            isExpanded={expandedStepId === step.id}
            isCompleted={isStepCompleted(step.id)}
            progress={getStepProgress(step)}
            onToggle={() => setExpandedStepId(expandedStepId === step.id ? null : step.id)}
            onComplete={() => onStepComplete?.(step.id)}
            onSubStepComplete={(subStepId) => onSubStepComplete?.(step.id, subStepId)}
            isSubStepCompleted={isSubStepCompleted}
            onImageClick={setImageModalUrl}
            readOnly={readOnly}
          />
        ))}
      </div>

      {/* Image Modal */}
      {imageModalUrl && (
        <div 
          className="fixed inset-0 z-[60] bg-black/80 flex items-center justify-center p-4"
          onClick={() => setImageModalUrl(null)}
        >
          <img 
            src={imageModalUrl} 
            alt="SOP step image" 
            className="max-w-full max-h-full object-contain rounded-lg"
          />
        </div>
      )}
    </div>
  );
}

interface StepCardProps {
  step: SopStep;
  stepNumber: number;
  isExpanded: boolean;
  isCompleted: boolean;
  progress: number;
  onToggle: () => void;
  onComplete: () => void;
  onSubStepComplete: (subStepId: string) => void;
  isSubStepCompleted: (subStepId: string) => boolean;
  onImageClick: (url: string) => void;
  readOnly: boolean;
}

function StepCard({
  step,
  stepNumber,
  isExpanded,
  isCompleted,
  progress,
  onToggle,
  onComplete,
  onSubStepComplete,
  isSubStepCompleted,
  onImageClick,
  readOnly,
}: StepCardProps) {
  const hasSubSteps = step.subSteps && step.subSteps.length > 0;
  const allSubStepsCompleted = hasSubSteps && step.subSteps!.every(s => isSubStepCompleted(s.id));
  const showAsCompleted = isCompleted || allSubStepsCompleted;

  return (
    <div className={`task-card rounded-lg overflow-hidden transition-all ${
      showAsCompleted ? 'opacity-75' : ''
    }`}>
      {/* Step Header */}
      <button
        onClick={onToggle}
        className="w-full flex items-center gap-4 p-4 text-left hover:bg-[var(--bg-tile-hover)] transition-colors"
      >
        <div className="flex items-center gap-3 flex-1">
          <div className={`flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center font-semibold text-sm ${
            showAsCompleted 
              ? 'bg-emerald-500/20 text-emerald-400' 
              : 'bg-[var(--accent-cyan)]/20 text-[var(--accent-cyan)]'
          }`}>
            {showAsCompleted ? <CheckCircle2 className="w-5 h-5" /> : stepNumber}
          </div>
          <div className="flex-1 min-w-0">
            <h4 className={`font-medium text-sm ${
              showAsCompleted ? 'text-[var(--text-muted)] line-through' : 'text-[var(--text-primary)]'
            }`}>
              {step.title}
            </h4>
            {step.estimatedMinutes && (
              <span className="text-xs text-[var(--text-subtle)]">~{step.estimatedMinutes} min</span>
            )}
          </div>
        </div>
        
        {hasSubSteps && (
          <div className="flex items-center gap-2">
            <div className="w-16 h-1.5 bg-[var(--bg-tile)] rounded-full overflow-hidden">
              <div 
                className="h-full bg-[var(--accent-emerald)] transition-all duration-300"
                style={{ width: `${progress}%` }}
              />
            </div>
            <span className="text-xs text-[var(--text-muted)] w-8">{progress}%</span>
          </div>
        )}
        
        {isExpanded ? (
          <ChevronUp className="w-5 h-5 text-[var(--text-muted)]" />
        ) : (
          <ChevronDown className="w-5 h-5 text-[var(--text-muted)]" />
        )}
      </button>

      {/* Expanded Content */}
      {isExpanded && (
        <div className="px-4 pb-4 border-t border-[var(--border)]">
          {/* Description */}
          {step.description && (
            <p className="text-sm text-[var(--text-muted)] mt-4 mb-4">{step.description}</p>
          )}

          {/* Main Step Image */}
          {step.imageUrl && (
            <button
              onClick={() => onImageClick(step.imageUrl!)}
              className="relative w-full mb-4 rounded-lg overflow-hidden group"
            >
              <img 
                src={step.imageUrl} 
                alt={step.imageCaption || step.title}
                className="w-full h-48 object-cover"
              />
              <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                <ImageIcon className="w-8 h-8 text-white" />
              </div>
              {step.imageCaption && (
                <div className="absolute bottom-0 left-0 right-0 bg-black/60 px-3 py-2">
                  <p className="text-xs text-white">{step.imageCaption}</p>
                </div>
              )}
            </button>
          )}

          {/* Warning */}
          {step.warningText && (
            <div className="flex items-start gap-3 p-3 mb-4 rounded-lg bg-rose-500/10 border border-rose-500/20">
              <AlertTriangle className="w-5 h-5 text-rose-400 flex-shrink-0 mt-0.5" />
              <p className="text-sm text-rose-300">{step.warningText}</p>
            </div>
          )}

          {/* Tip */}
          {step.tipText && (
            <div className="flex items-start gap-3 p-3 mb-4 rounded-lg bg-amber-500/10 border border-amber-500/20">
              <Lightbulb className="w-5 h-5 text-amber-400 flex-shrink-0 mt-0.5" />
              <p className="text-sm text-amber-300">{step.tipText}</p>
            </div>
          )}

          {/* Sub-steps as checklist */}
          {hasSubSteps ? (
            <div className="space-y-2 mt-4">
              {step.subSteps!.map((subStep) => (
                <SubStepItem
                  key={subStep.id}
                  subStep={subStep}
                  isCompleted={isSubStepCompleted(subStep.id)}
                  onComplete={() => onSubStepComplete(subStep.id)}
                  onImageClick={onImageClick}
                  readOnly={readOnly}
                />
              ))}
            </div>
          ) : (
            !readOnly && (
              <button
                onClick={onComplete}
                disabled={isCompleted}
                className={`mt-4 w-full flex items-center justify-center gap-2 py-3 rounded-lg font-medium text-sm transition-all ${
                  isCompleted
                    ? 'bg-emerald-500/20 text-emerald-400 cursor-default'
                    : 'bg-[var(--accent-cyan)] text-white hover:opacity-90'
                }`}
              >
                {isCompleted ? (
                  <>
                    <CheckCircle2 className="w-4 h-4" />
                    Step Completed
                  </>
                ) : (
                  <>
                    <Circle className="w-4 h-4" />
                    Mark as Complete
                  </>
                )}
              </button>
            )
          )}
        </div>
      )}
    </div>
  );
}

interface SubStepItemProps {
  subStep: SopSubStep;
  isCompleted: boolean;
  onComplete: () => void;
  onImageClick: (url: string) => void;
  readOnly: boolean;
}

function SubStepItem({ subStep, isCompleted, onComplete, onImageClick, readOnly }: SubStepItemProps) {
  return (
    <div className={`flex items-start gap-3 p-3 rounded-lg transition-all ${
      isCompleted ? 'bg-emerald-500/5' : 'bg-[var(--bg-tile)] hover:bg-[var(--bg-tile-hover)]'
    }`}>
      <button
        onClick={onComplete}
        disabled={readOnly}
        className={`flex-shrink-0 mt-0.5 transition-colors ${
          readOnly ? 'cursor-default' : 'cursor-pointer'
        }`}
      >
        {isCompleted ? (
          <CheckCircle2 className="w-5 h-5 text-emerald-400" />
        ) : (
          <Circle className="w-5 h-5 text-[var(--text-subtle)] hover:text-[var(--accent-cyan)]" />
        )}
      </button>
      <div className="flex-1 min-w-0">
        <p className={`text-sm ${
          isCompleted ? 'text-[var(--text-muted)] line-through' : 'text-[var(--text-primary)]'
        }`}>
          {subStep.text}
        </p>
        {subStep.imageUrl && (
          <button
            onClick={() => onImageClick(subStep.imageUrl!)}
            className="mt-2 relative rounded overflow-hidden group"
          >
            <img 
              src={subStep.imageUrl} 
              alt=""
              className="h-20 w-auto object-cover rounded"
            />
            <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
              <ImageIcon className="w-4 h-4 text-white" />
            </div>
          </button>
        )}
      </div>
    </div>
  );
}








