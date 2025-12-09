'use client';

import { Check, X, ArrowRight } from 'lucide-react';
import { PROBLEMS, SOLUTIONS } from './constants/dashboardData';

export function MobileProblemSolution() {
  return (
    <div className="w-full px-4 py-12">
      {/* Header */}
      <div className="text-center mb-8">
        <h2 className="text-2xl font-bold mb-3">
          <span className="text-foreground">Cultivation Has Evolved. </span>
          <span className="text-accent-amber">Your Tools Haven&apos;t.</span>
        </h2>
        <p className="text-sm text-muted-foreground">
          It&apos;s no longer just about growing—it&apos;s about precision, compliance, traceability, and operational excellence at scale.
        </p>
      </div>

      {/* Problems - The Old Way */}
      <div className="mb-8">
        <div className="flex items-center gap-2 mb-4">
          <div className="p-1.5 rounded-lg bg-accent-rose/10">
            <X className="h-4 w-4 text-accent-rose" />
          </div>
          <h3 className="text-lg font-semibold">The Old Way</h3>
        </div>
        <div className="space-y-3">
          {PROBLEMS.map((problem) => {
            const Icon = problem.icon;
            return (
              <div
                key={problem.title}
                className="p-4 rounded-xl bg-accent-rose/5 border border-accent-rose/10"
              >
                <div className="flex items-start gap-3">
                  <div className="p-1.5 rounded-lg bg-accent-rose/10 flex-shrink-0">
                    <Icon className="h-4 w-4 text-accent-rose" />
                  </div>
                  <div>
                    <h4 className="font-semibold text-foreground text-sm mb-1">
                      {problem.title}
                    </h4>
                    <p className="text-xs text-muted-foreground leading-relaxed">
                      {problem.description}
                    </p>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Solutions - The Harvestry Way */}
      <div>
        <div className="flex items-center gap-2 mb-4">
          <div className="p-1.5 rounded-lg bg-accent-emerald/10">
            <Check className="h-4 w-4 text-accent-emerald" />
          </div>
          <h3 className="text-lg font-semibold">The Harvestry Way</h3>
        </div>
        <div className="p-4 rounded-xl bg-accent-emerald/5 border border-accent-emerald/10">
          <ul className="space-y-3">
            {SOLUTIONS.map((solution, index) => (
              <li key={index} className="flex items-start gap-2.5">
                <Check className="h-4 w-4 text-accent-emerald flex-shrink-0 mt-0.5" />
                <span className="text-sm text-foreground">{solution}</span>
              </li>
            ))}
          </ul>
          <div className="mt-5 pt-4 border-t border-accent-emerald/10">
            <a
              href="#features"
              className="inline-flex items-center gap-2 text-accent-emerald font-medium text-sm hover:gap-3 transition-all"
            >
              See all features
              <ArrowRight className="h-4 w-4" />
            </a>
          </div>
        </div>
      </div>
    </div>
  );
}
