'use client';

import Link from 'next/link';
import { Linkedin, Twitter, Leaf } from 'lucide-react';

const footerLinks = {
  product: {
    title: 'Product',
    links: [
      { label: 'Features', href: '#features' },
      { label: 'Solutions', href: '#solutions' },
      { label: 'Integrations', href: '#integrations' },
      { label: 'Pricing', href: '#pricing' },
      { label: 'Security', href: '/security' },
    ],
  },
  company: {
    title: 'Company',
    links: [
      { label: 'About', href: '/about' },
      { label: 'Careers', href: '/careers' },
      { label: 'Blog', href: '/blog' },
      { label: 'Press', href: '/press' },
      { label: 'Contact', href: '/contact' },
    ],
  },
  resources: {
    title: 'Resources',
    links: [
      { label: 'Documentation', href: '/docs' },
      { label: 'API Reference', href: '/api' },
      { label: 'Status', href: 'https://status.harvestry.io' },
      { label: 'Support', href: '/support' },
      { label: 'Community', href: '/community' },
    ],
  },
  legal: {
    title: 'Legal',
    links: [
      { label: 'Privacy Policy', href: '/privacy' },
      { label: 'Terms of Service', href: '/terms' },
      { label: 'Cookie Policy', href: '/cookies' },
      { label: 'GDPR', href: '/gdpr' },
    ],
  },
};

export function Footer() {
  return (
    <footer className="border-t border-border bg-surface/30">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Main Footer */}
        <div className="py-12 grid grid-cols-2 md:grid-cols-6 gap-8">
          {/* Brand */}
          <div className="col-span-2">
            <Link href="/" className="flex items-center gap-2 mb-4">
              <Leaf className="h-8 w-8 text-accent-emerald" />
              <span className="text-xl font-bold">
                <span className="text-white">Harvestry</span>
                <span className="text-accent-emerald">.io</span>
              </span>
            </Link>
            <p className="text-sm text-muted-foreground mb-6 max-w-xs">
              The Modern Cultivation Operating System. Unify your grow. Simplify compliance. Optimize everything.
            </p>
            <div className="flex gap-4">
              <a 
                href="https://twitter.com/harvestryio" 
                target="_blank" 
                rel="noopener noreferrer"
                className="p-2 rounded-lg bg-surface hover:bg-elevated border border-border transition-colors"
                aria-label="Twitter"
              >
                <Twitter className="h-5 w-5 text-muted-foreground" />
              </a>
              <a 
                href="https://linkedin.com/company/harvestry" 
                target="_blank" 
                rel="noopener noreferrer"
                className="p-2 rounded-lg bg-surface hover:bg-elevated border border-border transition-colors"
                aria-label="LinkedIn"
              >
                <Linkedin className="h-5 w-5 text-muted-foreground" />
              </a>
            </div>
          </div>

          {/* Links */}
          {Object.values(footerLinks).map((section) => (
            <div key={section.title}>
              <h3 className="font-semibold text-foreground mb-4">{section.title}</h3>
              <ul className="space-y-3">
                {section.links.map((link) => (
                  <li key={link.label}>
                    <Link 
                      href={link.href}
                      className="text-sm text-muted-foreground hover:text-foreground transition-colors"
                    >
                      {link.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* Bottom Bar */}
        <div className="py-6 border-t border-border flex flex-col sm:flex-row items-center justify-between gap-4">
          <p className="text-sm text-muted-foreground">
            © {new Date().getFullYear()} Harvestry, Inc. All rights reserved.
          </p>
          <div className="flex items-center gap-6">
            <span className="text-sm text-muted-foreground">
              Made with 🌱 for cultivators everywhere
            </span>
          </div>
        </div>
      </div>
    </footer>
  );
}

