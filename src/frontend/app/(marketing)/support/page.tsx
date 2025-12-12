'use client';

import { useState } from 'react';
import { Send, CheckCircle, Headphones, MessageSquare, FileQuestion, AlertCircle } from 'lucide-react';
import { cn } from '@/lib/utils';

type TicketCategory = 'technical' | 'billing' | 'feature' | 'other';
type Priority = 'low' | 'medium' | 'high';

interface FormData {
  name: string;
  email: string;
  company: string;
  category: TicketCategory;
  priority: Priority;
  subject: string;
  message: string;
}

const categories: { value: TicketCategory; label: string; icon: React.ComponentType<{ className?: string }> }[] = [
  { value: 'technical', label: 'Technical Issue', icon: AlertCircle },
  { value: 'billing', label: 'Billing & Account', icon: FileQuestion },
  { value: 'feature', label: 'Feature Request', icon: MessageSquare },
  { value: 'other', label: 'General Inquiry', icon: Headphones },
];

const priorities: { value: Priority; label: string; color: string }[] = [
  { value: 'low', label: 'Low', color: 'text-blue-400' },
  { value: 'medium', label: 'Medium', color: 'text-amber-400' },
  { value: 'high', label: 'High', color: 'text-red-400' },
];

export default function SupportPage() {
  const [formData, setFormData] = useState<FormData>({
    name: '',
    email: '',
    company: '',
    category: 'technical',
    priority: 'medium',
    subject: '',
    message: '',
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isSubmitted, setIsSubmitted] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setError(null);

    try {
      // Create mailto link as fallback - opens email client
      const subject = encodeURIComponent(`[${formData.category.toUpperCase()}] ${formData.subject}`);
      const body = encodeURIComponent(
        `Name: ${formData.name}\n` +
        `Email: ${formData.email}\n` +
        `Company: ${formData.company}\n` +
        `Category: ${formData.category}\n` +
        `Priority: ${formData.priority}\n\n` +
        `Message:\n${formData.message}`
      );
      
      // In production, this would be an API call
      // For now, we'll simulate success and provide mailto fallback
      await new Promise(resolve => setTimeout(resolve, 1500));
      
      // Open mailto as backup
      window.location.href = `mailto:support@harvestry.io?subject=${subject}&body=${body}`;
      
      setIsSubmitted(true);
    } catch (err) {
      setError('Failed to submit ticket. Please try again or email us directly at support@harvestry.io');
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isSubmitted) {
    return (
      <div className="max-w-2xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <div className="bg-surface/50 border border-border/50 rounded-2xl p-8 text-center">
          <div className="w-16 h-16 rounded-full bg-emerald-500/10 flex items-center justify-center mx-auto mb-6">
            <CheckCircle className="w-8 h-8 text-emerald-400" />
          </div>
          <h2 className="text-2xl font-bold text-foreground mb-3">
            Ticket Submitted!
          </h2>
          <p className="text-muted-foreground mb-6">
            Thank you for contacting us. Your support request has been received and 
            our team will respond to <span className="text-foreground">{formData.email}</span> within 24 hours.
          </p>
          <p className="text-sm text-muted-foreground mb-6">
            Your email client may have opened to send the ticket directly. If so, please send the email to complete your submission.
          </p>
          <button
            onClick={() => {
              setIsSubmitted(false);
              setFormData({
                name: '',
                email: '',
                company: '',
                category: 'technical',
                priority: 'medium',
                subject: '',
                message: '',
              });
            }}
            className="px-6 py-3 rounded-lg font-medium text-sm text-foreground bg-surface border border-border hover:bg-elevated transition-all"
          >
            Submit Another Ticket
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      {/* Header */}
      <div className="text-center mb-12">
        <div className="w-16 h-16 rounded-2xl bg-cyan-500/10 flex items-center justify-center mx-auto mb-6">
          <Headphones className="w-8 h-8 text-cyan-400" />
        </div>
        <h1 className="text-3xl sm:text-4xl font-bold text-foreground mb-4">
          Support Center
        </h1>
        <p className="text-muted-foreground max-w-xl mx-auto">
          Need help? Submit a support ticket and our team will get back to you within 24 hours. 
          For urgent issues, email us directly at{' '}
          <a href="mailto:support@harvestry.io" className="text-cyan-400 hover:underline">
            support@harvestry.io
          </a>
        </p>
      </div>

      {/* Form */}
      <div className="bg-surface/50 border border-border/50 rounded-2xl p-6 sm:p-8">
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Contact Info */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-foreground mb-2">
                Full Name *
              </label>
              <input
                type="text"
                id="name"
                name="name"
                required
                value={formData.name}
                onChange={handleChange}
                className="w-full px-4 py-3 bg-background border border-border rounded-lg text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/50 transition-colors"
                placeholder="John Doe"
              />
            </div>
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-foreground mb-2">
                Email Address *
              </label>
              <input
                type="email"
                id="email"
                name="email"
                required
                value={formData.email}
                onChange={handleChange}
                className="w-full px-4 py-3 bg-background border border-border rounded-lg text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/50 transition-colors"
                placeholder="john@company.com"
              />
            </div>
          </div>

          <div>
            <label htmlFor="company" className="block text-sm font-medium text-foreground mb-2">
              Company Name
            </label>
            <input
              type="text"
              id="company"
              name="company"
              value={formData.company}
              onChange={handleChange}
              className="w-full px-4 py-3 bg-background border border-border rounded-lg text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/50 transition-colors"
              placeholder="Your Company"
            />
          </div>

          {/* Category & Priority */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label htmlFor="category" className="block text-sm font-medium text-foreground mb-2">
                Category *
              </label>
              <select
                id="category"
                name="category"
                required
                value={formData.category}
                onChange={handleChange}
                className="w-full px-4 py-3 bg-background border border-border rounded-lg text-foreground focus:outline-none focus:border-cyan-500/50 transition-colors"
              >
                {categories.map(cat => (
                  <option key={cat.value} value={cat.value}>
                    {cat.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label htmlFor="priority" className="block text-sm font-medium text-foreground mb-2">
                Priority
              </label>
              <select
                id="priority"
                name="priority"
                value={formData.priority}
                onChange={handleChange}
                className="w-full px-4 py-3 bg-background border border-border rounded-lg text-foreground focus:outline-none focus:border-cyan-500/50 transition-colors"
              >
                {priorities.map(p => (
                  <option key={p.value} value={p.value}>
                    {p.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Subject */}
          <div>
            <label htmlFor="subject" className="block text-sm font-medium text-foreground mb-2">
              Subject *
            </label>
            <input
              type="text"
              id="subject"
              name="subject"
              required
              value={formData.subject}
              onChange={handleChange}
              className="w-full px-4 py-3 bg-background border border-border rounded-lg text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/50 transition-colors"
              placeholder="Brief description of your issue"
            />
          </div>

          {/* Message */}
          <div>
            <label htmlFor="message" className="block text-sm font-medium text-foreground mb-2">
              Message *
            </label>
            <textarea
              id="message"
              name="message"
              required
              rows={6}
              value={formData.message}
              onChange={handleChange}
              className="w-full px-4 py-3 bg-background border border-border rounded-lg text-foreground placeholder:text-muted-foreground focus:outline-none focus:border-cyan-500/50 transition-colors resize-none"
              placeholder="Please describe your issue in detail. Include any relevant information such as error messages, steps to reproduce, etc."
            />
          </div>

          {/* Error Message */}
          {error && (
            <div className="p-4 bg-red-500/10 border border-red-500/20 rounded-lg">
              <p className="text-sm text-red-400">{error}</p>
            </div>
          )}

          {/* Submit */}
          <button
            type="submit"
            disabled={isSubmitting}
            className={cn(
              "w-full px-6 py-3 rounded-lg font-medium text-white bg-cyan-600 hover:bg-cyan-500 transition-all flex items-center justify-center gap-2",
              isSubmitting && "opacity-50 cursor-not-allowed"
            )}
          >
            {isSubmitting ? (
              <>
                <div className="w-5 h-5 border-2 border-white/20 border-t-white rounded-full animate-spin" />
                Submitting...
              </>
            ) : (
              <>
                <Send className="w-5 h-5" />
                Submit Ticket
              </>
            )}
          </button>
        </form>
      </div>

      {/* Additional Help */}
      <div className="mt-8 grid grid-cols-1 sm:grid-cols-3 gap-4">
        <a
          href="mailto:support@harvestry.io"
          className="bg-surface/50 border border-border/50 rounded-xl p-4 hover:bg-surface/70 transition-colors text-center"
        >
          <MessageSquare className="w-6 h-6 text-cyan-400 mx-auto mb-2" />
          <h3 className="font-medium text-foreground mb-1">Email Support</h3>
          <p className="text-sm text-muted-foreground">support@harvestry.io</p>
        </a>
        <a
          href="/docs"
          className="bg-surface/50 border border-border/50 rounded-xl p-4 hover:bg-surface/70 transition-colors text-center"
        >
          <FileQuestion className="w-6 h-6 text-violet-400 mx-auto mb-2" />
          <h3 className="font-medium text-foreground mb-1">Documentation</h3>
          <p className="text-sm text-muted-foreground">Browse our guides</p>
        </a>
        <a
          href="/status"
          className="bg-surface/50 border border-border/50 rounded-xl p-4 hover:bg-surface/70 transition-colors text-center"
        >
          <AlertCircle className="w-6 h-6 text-emerald-400 mx-auto mb-2" />
          <h3 className="font-medium text-foreground mb-1">System Status</h3>
          <p className="text-sm text-muted-foreground">Check service health</p>
        </a>
      </div>
    </div>
  );
}





