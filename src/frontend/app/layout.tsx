import type { Metadata } from 'next';
import { Inter, JetBrains_Mono } from 'next/font/google';

import '@/styles/globals.css';
import { ThemeProvider } from '@/components/common/ThemeProvider';
import { AuthProvider } from '@/providers/AuthProvider';
import { DemoSeeder } from '@/components/demo/DemoSeeder';

const fontSans = Inter({ 
  subsets: ['latin'], 
  variable: '--font-primary',
  display: 'swap',
});

const fontMono = JetBrains_Mono({ 
  subsets: ['latin'], 
  variable: '--font-mono',
  display: 'swap',
});

export const metadata: Metadata = {
  title: 'Harvestry',
  description: 'Cannabis cultivation management platform - Grow Smarter. Stay Compliant. Scale Confidently.',
  icons: {
    icon: [
      { url: '/icon.svg', type: 'image/svg+xml' },
    ],
    shortcut: '/icon.svg',
    apple: '/images/Harvestry_logo_1.png',
  },
  manifest: '/manifest.json',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className={`${fontSans.variable} ${fontMono.variable}`} suppressHydrationWarning>
      <body className="min-h-screen bg-background text-foreground font-sans antialiased selection:bg-cyan-500/30 selection:text-primary">
        <ThemeProvider defaultTheme="dark">
          <AuthProvider>
            <DemoSeeder />
            {children}
          </AuthProvider>
        </ThemeProvider>
      </body>
    </html>
  );
}
