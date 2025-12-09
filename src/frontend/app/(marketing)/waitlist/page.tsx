'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { ArrowLeft, Leaf, Mail, User, Building2, Ruler, Loader2 } from 'lucide-react';

export default function WaitlistPage() {
  const router = useRouter();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState({
    email: '',
    name: '',
    company: '',
    facility_size: '',
  });

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      const response = await fetch('/api/v1/waitlist', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ...formData,
          source: 'waitlist_page',
        }),
      });

      const result = await response.json();

      if (result.success) {
        router.push('/waitlist/thank-you');
      } else {
        setError(result.error || 'Something went wrong. Please try again.');
      }
    } catch {
      setError('Network error. Please check your connection and try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setFormData(prev => ({ ...prev, [e.target.name]: e.target.value }));
  };

  return (
    <div className="min-h-screen bg-background relative overflow-hidden">
      {/* Background Effects */}
      <div className="absolute inset-0">
        <div className="absolute top-1/4 left-1/4 w-[600px] h-[600px] bg-accent-emerald/10 rounded-full blur-[150px]" />
        <div className="absolute bottom-1/4 right-1/4 w-[500px] h-[500px] bg-accent-cyan/8 rounded-full blur-[120px]" />
      </div>

      {/* Content */}
      <div className="relative z-10 flex flex-col items-center justify-center min-h-screen px-4 py-12">
        {/* Back Link */}
        <Link 
          href="/"
          className="absolute top-6 left-6 flex items-center gap-2 text-muted-foreground hover:text-foreground transition-colors"
        >
          <ArrowLeft className="h-4 w-4" />
          <span className="text-sm font-medium">Back to Home</span>
        </Link>

        {/* Form Card */}
        <div className="w-full max-w-md">
          {/* Header */}
          <div className="text-center mb-8">
            <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-accent-emerald/10 border border-accent-emerald/20 mb-6">
              <Leaf className="h-8 w-8 text-accent-emerald" />
            </div>
            <h1 className="text-3xl sm:text-4xl font-bold mb-3">
              Get Early Access
            </h1>
            <p className="text-muted-foreground text-lg">
              Be the first to experience the future of cultivation management.
            </p>
          </div>

          {/* Value Props */}
          <div className="grid grid-cols-2 gap-3 mb-8">
            {[
              'Priority Onboarding',
              'Founding Member Pricing',
              'Direct Input on Features',
              'Exclusive Beta Access',
            ].map((benefit) => (
              <div 
                key={benefit}
                className="flex items-center gap-2 px-3 py-2 rounded-lg bg-surface/50 border border-border/50 text-sm"
              >
                <span className="w-1.5 h-1.5 rounded-full bg-accent-emerald flex-shrink-0" />
                <span className="text-muted-foreground">{benefit}</span>
              </div>
            ))}
          </div>

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
            {/* Email - Required */}
            <div>
              <label htmlFor="email" className="block text-sm font-medium mb-2">
                Email Address <span className="text-accent-amber">*</span>
              </label>
              <div className="relative">
                <Mail className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground" />
                <input
                  type="email"
                  id="email"
                  name="email"
                  required
                  value={formData.email}
                  onChange={handleChange}
                  placeholder="you@cultivation.com"
                  className="w-full pl-11 pr-4 py-3 rounded-xl bg-surface border border-border focus:border-accent-emerald focus:ring-1 focus:ring-accent-emerald outline-none transition-all"
                />
              </div>
            </div>

            {/* Name - Optional */}
            <div>
              <label htmlFor="name" className="block text-sm font-medium mb-2">
                Your Name
              </label>
              <div className="relative">
                <User className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground" />
                <input
                  type="text"
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleChange}
                  placeholder="John Smith"
                  className="w-full pl-11 pr-4 py-3 rounded-xl bg-surface border border-border focus:border-accent-emerald focus:ring-1 focus:ring-accent-emerald outline-none transition-all"
                />
              </div>
            </div>

            {/* Company - Optional */}
            <div>
              <label htmlFor="company" className="block text-sm font-medium mb-2">
                Company / Facility Name
              </label>
              <div className="relative">
                <Building2 className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground" />
                <input
                  type="text"
                  id="company"
                  name="company"
                  value={formData.company}
                  onChange={handleChange}
                  placeholder="Green Valley Farms"
                  className="w-full pl-11 pr-4 py-3 rounded-xl bg-surface border border-border focus:border-accent-emerald focus:ring-1 focus:ring-accent-emerald outline-none transition-all"
                />
              </div>
            </div>

            {/* Facility Size - Optional */}
            <div>
              <label htmlFor="facility_size" className="block text-sm font-medium mb-2">
                Facility Size
              </label>
              <div className="relative">
                <Ruler className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-muted-foreground" />
                <select
                  id="facility_size"
                  name="facility_size"
                  value={formData.facility_size}
                  onChange={handleChange}
                  className="w-full pl-11 pr-4 py-3 rounded-xl bg-surface border border-border focus:border-accent-emerald focus:ring-1 focus:ring-accent-emerald outline-none transition-all appearance-none cursor-pointer"
                >
                  <option value="">Select size (optional)</option>
                  <option value="under_5k">Under 5,000 sq ft</option>
                  <option value="5k_15k">5,000 - 15,000 sq ft</option>
                  <option value="15k_50k">15,000 - 50,000 sq ft</option>
                  <option value="50k_plus">50,000+ sq ft</option>
                </select>
              </div>
            </div>

            {/* Error Message */}
            {error && (
              <div className="p-3 rounded-lg bg-red-500/10 border border-red-500/20 text-red-400 text-sm">
                {error}
              </div>
            )}

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full py-4 px-6 rounded-xl bg-accent-emerald text-white font-semibold text-lg hover:bg-accent-emerald/90 disabled:opacity-50 disabled:cursor-not-allowed transition-all flex items-center justify-center gap-2 mt-6"
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="h-5 w-5 animate-spin" />
                  Joining...
                </>
              ) : (
                'Join the Waitlist'
              )}
            </button>

            {/* Privacy Note */}
            <p className="text-xs text-center text-muted-foreground mt-4">
              We respect your privacy. No spam, ever.{' '}
              <Link href="/privacy" className="text-accent-emerald hover:underline">
                Privacy Policy
              </Link>
            </p>
          </form>
        </div>
      </div>
    </div>
  );
}

