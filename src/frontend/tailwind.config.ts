import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
    "./features/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
    "./layouts/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['var(--font-primary)', 'ui-sans-serif', 'system-ui', '-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'Roboto', 'Helvetica Neue', 'Arial', 'sans-serif'],
        mono: ['var(--font-mono)', 'ui-monospace', 'SFMono-Regular', 'Menlo', 'Monaco', 'Consolas', 'Liberation Mono', 'Courier New', 'monospace'],
      },
      colors: {
        // Using rgb() with CSS variables for opacity modifier support
        background: "rgb(var(--bg-primary-rgb) / <alpha-value>)",
        foreground: "rgb(var(--text-primary-rgb) / <alpha-value>)",
        surface: "rgb(var(--bg-surface-rgb) / <alpha-value>)",
        elevated: "rgb(var(--bg-elevated-rgb) / <alpha-value>)",
        hover: "rgb(var(--bg-hover-rgb) / <alpha-value>)",
        border: "rgb(var(--border-rgb) / <alpha-value>)",
        primary: {
          DEFAULT: "rgb(var(--accent-cyan-rgb) / <alpha-value>)",
          foreground: "#ffffff"
        },
        muted: {
          DEFAULT: "rgb(var(--bg-hover-rgb) / <alpha-value>)",
          foreground: "rgb(var(--text-muted-rgb) / <alpha-value>)"
        },
        popover: {
          DEFAULT: "rgb(var(--bg-elevated-rgb) / <alpha-value>)",
          foreground: "rgb(var(--text-primary-rgb) / <alpha-value>)"
        },
        // Semantic accent colors with opacity support
        accent: {
          cyan: "rgb(var(--accent-cyan-rgb) / <alpha-value>)",
          emerald: "rgb(var(--accent-emerald-rgb) / <alpha-value>)",
          amber: "rgb(var(--accent-amber-rgb) / <alpha-value>)",
          rose: "rgb(var(--accent-rose-rgb) / <alpha-value>)",
          sky: "rgb(var(--accent-sky-rgb) / <alpha-value>)",
          violet: "rgb(var(--accent-violet-rgb) / <alpha-value>)",
        }
      },
      // Ring offset color uses theme variable
      ringOffsetColor: {
        background: "rgb(var(--bg-primary-rgb))",
        surface: "rgb(var(--bg-surface-rgb))",
      },
      // Fill colors for SVG elements using theme variables
      fill: {
        surface: "rgb(var(--bg-surface-rgb))",
        background: "rgb(var(--bg-primary-rgb))",
        elevated: "rgb(var(--bg-elevated-rgb))",
        foreground: "rgb(var(--text-primary-rgb))",
        muted: "rgb(var(--text-muted-rgb))",
        border: "rgb(var(--border-rgb))",
      },
      // Stroke colors for SVG elements using theme variables
      stroke: {
        surface: "rgb(var(--bg-surface-rgb))",
        background: "rgb(var(--bg-primary-rgb))",
        foreground: "rgb(var(--text-primary-rgb))",
        muted: "rgb(var(--text-muted-rgb))",
        border: "rgb(var(--border-rgb))",
      },
    },
  },
  plugins: [],
};
export default config;
