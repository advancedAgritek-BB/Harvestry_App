'use client';

import React, { useEffect, useMemo, useRef, useState } from 'react';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { ChevronDown, LogOut, Moon, Sun, User as UserIcon, LogIn } from 'lucide-react';
import { useAuthStore } from '@/stores/auth/authStore';
import { useThemeStore } from '@/stores/app/themeStore';

interface MenuItemProps {
  icon: React.ElementType;
  label: string;
  onClick?: () => void;
  href?: string;
  rightSlot?: React.ReactNode;
  disabled?: boolean;
}

function MenuItem({ icon: Icon, label, onClick, href, rightSlot, disabled }: MenuItemProps) {
  const content = (
    <div className="flex items-center gap-3 px-3 py-2.5 rounded-lg hover:bg-[var(--bg-tile-hover)] transition-colors">
      <Icon className="w-4 h-4 text-[var(--text-muted)]" />
      <span className="flex-1 text-sm text-[var(--text-primary)]">{label}</span>
      {rightSlot}
    </div>
  );

  if (href) {
    return (
      <Link
        href={href}
        aria-disabled={disabled ? 'true' : 'false'}
        tabIndex={disabled ? -1 : 0}
        className={disabled ? 'opacity-50 pointer-events-none' : ''}
        onClick={onClick}
      >
        {content}
      </Link>
    );
  }

  return (
    <button
      type="button"
      disabled={disabled}
      className={disabled ? 'w-full text-left opacity-50 cursor-not-allowed' : 'w-full text-left'}
      onClick={onClick}
    >
      {content}
    </button>
  );
}

export function UserProfileMenu() {
  const router = useRouter();
  const pathname = usePathname();
  const prevPathnameRef = useRef(pathname);

  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);

  const theme = useThemeStore((s) => s.theme);
  const toggleTheme = useThemeStore((s) => s.toggleTheme);

  const [isOpen, setIsOpen] = useState(false);

  const displayName = useMemo(() => {
    if (!user) return 'Account';
    return user.name || user.email || 'Account';
  }, [user]);

  const avatarUrl = user?.avatarUrl || '/images/user-avatar.png';

  const closeMenu = () => setIsOpen(false);

  // Close on navigation (only when pathname actually changes)
  useEffect(() => {
    const prev = prevPathnameRef.current;
    if (prev !== pathname) {
      prevPathnameRef.current = pathname;
      if (isOpen) closeMenu();
      return;
    }

    // Keep ref current even if this runs before first change
    prevPathnameRef.current = pathname;
  }, [pathname, isOpen]);

  // Close on Escape
  useEffect(() => {
    if (!isOpen) return;

    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        e.preventDefault();
        closeMenu();
      }
    };

    window.addEventListener('keydown', onKeyDown);
    return () => window.removeEventListener('keydown', onKeyDown);
  }, [isOpen]);

  const handleLogout = () => {
    closeMenu();
    logout();
    router.push('/login');
  };

  const ThemeIcon = theme === 'dark' ? Moon : Sun;

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setIsOpen((v) => !v)}
        className="flex items-center gap-3 pl-2 pr-1 py-1 rounded-full hover:bg-[var(--bg-tile)] transition-colors"
        aria-haspopup="true"
      >
        <span className="text-sm font-medium text-[var(--text-primary)] hidden md:block">
          {displayName}
        </span>
        <img
          src={avatarUrl}
          alt="User avatar"
          className="w-10 h-10 rounded-full ring-2 ring-[var(--bg-surface)] shadow-lg object-cover"
        />
        <ChevronDown className="w-4 h-4 text-[var(--text-muted)] hidden lg:block" />
      </button>

      {isOpen && (
        <>
          <div className="fixed inset-0 z-40" onClick={closeMenu} />

          <div
            className="absolute right-0 mt-2 w-64 bg-[var(--bg-surface)] rounded-xl shadow-xl border border-[var(--border)] overflow-hidden z-50 p-2"
          >
            <div className="px-3 py-2">
              <div className="text-xs text-[var(--text-muted)]">Signed in as</div>
              <div className="text-sm font-semibold text-[var(--text-primary)] truncate">
                {user?.email ?? 'Not signed in'}
              </div>
            </div>

            <div className="h-px bg-[var(--border)] my-1" />

            {user ? (
              <>
                <MenuItem
                  icon={UserIcon}
                  label="My Profile"
                  href="/dashboard/profile"
                  onClick={closeMenu}
                />

                <MenuItem
                  icon={ThemeIcon}
                  label="Theme"
                  rightSlot={
                    <span className="text-xs text-[var(--text-muted)]">
                      {theme === 'dark' ? 'Midnight' : 'Daylight'}
                    </span>
                  }
                  onClick={toggleTheme}
                />

                <div className="h-px bg-[var(--border)] my-1" />

                <MenuItem icon={LogOut} label="Sign Out" onClick={handleLogout} />
              </>
            ) : (
              <>
                <MenuItem
                  icon={LogIn}
                  label="Sign In"
                  href="/login"
                  onClick={closeMenu}
                />
              </>
            )}
          </div>
        </>
      )}
    </div>
  );
}
