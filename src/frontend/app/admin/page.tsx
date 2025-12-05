'use client';

import { redirect } from 'next/navigation';

// Redirect to cultivation tab by default
export default function AdminPage() {
  redirect('/admin/cultivation');
}

