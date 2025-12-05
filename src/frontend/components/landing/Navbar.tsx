'use client';

import { useState, useEffect } from 'react';
import Link from 'next/link';
import { Menu, X, ArrowRight, Leaf } from 'lucide-react';
import { clsx } from 'clsx';

const navLinks = [
  { label: 'Features', href: '#features' },
  { label: 'Solutions', href: '#solutions' },
  { label: 'Integrations', href: '#integrations' },
  { label: 'Pricing', href: '#pricing' },
];

export function Navbar() {
  const [isScrolled, setIsScrolled] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [activeSection, setActiveSection] = useState('');

  useEffect(() => {
    const handleScroll = () => {
      setIsScrolled(window.scrollY > 20);
      
      // Update active section based on scroll position
      const sections = navLinks.map(link => link.href.replace('#', ''));
      for (const section of sections.reverse()) {
        const element = document.getElementById(section);
        if (element) {
          const rect = element.getBoundingClientRect();
          if (rect.top <= 100) {
            setActiveSection(section);
            break;
          }
        }
      }
    };
    
    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  return (
    <header
      className={clsx(
        'fixed top-0 left-0 right-0 z-50 transition-all duration-500',
        isScrolled
          ? 'bg-background/70 backdrop-blur-xl border-b border-border/50 shadow-lg shadow-black/5'
          : 'bg-transparent'
      )}
    >
      <nav className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-20 lg:h-24">
          {/* Logo with hover effect */}
          <Link href="/" className="flex items-center gap-3 group">
            <div className="relative">
              <div className="absolute inset-0 bg-accent-emerald/20 blur-xl rounded-full scale-150 opacity-0 group-hover:opacity-100 transition-opacity duration-500" />
              <Leaf className="relative h-10 w-10 text-accent-emerald group-hover:scale-110 transition-transform duration-300" />
            </div>
            <span className="text-2xl font-bold tracking-tight">
              <span className="text-white">Harvestry</span>
              <span className="text-accent-emerald">.io</span>
            </span>
          </Link>

          {/* Desktop Navigation with animated underline */}
          <div className="hidden lg:flex items-center gap-2">
            {navLinks.map((link) => {
              const isActive = activeSection === link.href.replace('#', '');
              return (
                <Link
                  key={link.href}
                  href={link.href}
                  className={clsx(
                    'relative px-5 py-2.5 text-base font-medium transition-colors duration-300 group',
                    isActive ? 'text-foreground' : 'text-muted-foreground hover:text-foreground'
                  )}
                >
                  {link.label}
                  {/* Animated underline */}
                  <span 
                    className={clsx(
                      'absolute bottom-0 left-1/2 -translate-x-1/2 h-0.5 bg-accent-emerald rounded-full transition-all duration-300',
                      isActive ? 'w-8' : 'w-0 group-hover:w-8'
                    )}
                  />
                </Link>
              );
            })}
          </div>

          {/* Desktop CTA with premium effects */}
          <div className="hidden lg:flex items-center gap-4">
            <Link
              href="/login"
              className="px-5 py-2.5 text-base font-medium text-muted-foreground hover:text-foreground transition-colors duration-300"
            >
              Sign In
            </Link>
            <Link
              href="#demo"
              className="group relative inline-flex items-center justify-center gap-2 px-6 py-3 text-base font-semibold text-white bg-accent-emerald rounded-xl overflow-hidden transition-all duration-300 hover:shadow-lg hover:shadow-accent-emerald/20"
            >
              {/* Animated gradient on hover */}
              <span className="absolute inset-0 bg-gradient-to-r from-accent-emerald via-emerald-400 to-accent-emerald bg-[length:200%_auto] opacity-0 group-hover:opacity-100 group-hover:animate-gradient transition-opacity duration-500" />
              <span className="relative">Book a Demo</span>
              <ArrowRight className="relative h-5 w-5 group-hover:translate-x-0.5 transition-transform duration-300" />
            </Link>
          </div>

          {/* Mobile Menu Button */}
          <button
            onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
            className="lg:hidden relative p-2 text-muted-foreground hover:text-foreground transition-colors duration-300"
            aria-label="Toggle menu"
          >
            <div className="relative w-6 h-6">
              <Menu 
                className={clsx(
                  'absolute inset-0 h-6 w-6 transition-all duration-300',
                  isMobileMenuOpen ? 'opacity-0 rotate-90 scale-50' : 'opacity-100 rotate-0 scale-100'
                )} 
              />
              <X 
                className={clsx(
                  'absolute inset-0 h-6 w-6 transition-all duration-300',
                  isMobileMenuOpen ? 'opacity-100 rotate-0 scale-100' : 'opacity-0 -rotate-90 scale-50'
                )} 
              />
            </div>
          </button>
        </div>

        {/* Mobile Menu with slide animation */}
        <div
          className={clsx(
            'lg:hidden overflow-hidden transition-all duration-500 ease-out',
            isMobileMenuOpen ? 'max-h-[400px] opacity-100' : 'max-h-0 opacity-0'
          )}
        >
          <div className="py-4 border-t border-border/50">
            <div className="flex flex-col gap-1">
              {navLinks.map((link, index) => (
                <Link
                  key={link.href}
                  href={link.href}
                  onClick={() => setIsMobileMenuOpen(false)}
                  className="px-4 py-3 text-base font-medium text-muted-foreground hover:text-foreground hover:bg-surface/50 rounded-lg transition-all duration-300"
                  style={{ 
                    transitionDelay: isMobileMenuOpen ? `${index * 50}ms` : '0ms',
                    transform: isMobileMenuOpen ? 'translateX(0)' : 'translateX(-10px)',
                    opacity: isMobileMenuOpen ? 1 : 0,
                  }}
                >
                  {link.label}
                </Link>
              ))}
              <div className="flex flex-col gap-3 pt-4 mt-2 border-t border-border/50">
                <Link
                  href="/login"
                  onClick={() => setIsMobileMenuOpen(false)}
                  className="px-4 py-3 text-base font-medium text-muted-foreground hover:text-foreground hover:bg-surface/50 rounded-lg transition-colors duration-300"
                >
                  Sign In
                </Link>
                <Link
                  href="#demo"
                  onClick={() => setIsMobileMenuOpen(false)}
                  className="mx-4 inline-flex items-center justify-center gap-2 px-5 py-3 text-base font-semibold text-white bg-accent-emerald hover:bg-accent-emerald/90 rounded-lg transition-all duration-300"
                >
                  Book a Demo
                  <ArrowRight className="h-5 w-5" />
                </Link>
              </div>
            </div>
          </div>
        </div>
      </nav>
    </header>
  );
}

