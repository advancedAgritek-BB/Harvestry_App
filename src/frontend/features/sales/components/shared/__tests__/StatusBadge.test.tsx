/**
 * @jest-environment jsdom
 */
import { render, screen } from '@testing-library/react';
import { StatusBadge, ComplianceBadge, MetrcBadge } from '../StatusBadge';

describe('StatusBadge', () => {
  it('renders with Draft status', () => {
    render(<StatusBadge status="Draft" />);
    expect(screen.getByText('Draft')).toBeInTheDocument();
  });

  it('renders with Submitted status', () => {
    render(<StatusBadge status="Submitted" />);
    expect(screen.getByText('Submitted')).toBeInTheDocument();
  });

  it('renders with Allocated status', () => {
    render(<StatusBadge status="Allocated" />);
    expect(screen.getByText('Allocated')).toBeInTheDocument();
  });

  it('renders with Shipped status', () => {
    render(<StatusBadge status="Shipped" />);
    expect(screen.getByText('Shipped')).toBeInTheDocument();
  });

  it('renders with Cancelled status', () => {
    render(<StatusBadge status="Cancelled" />);
    expect(screen.getByText('Cancelled')).toBeInTheDocument();
  });

  it('renders without icon when showIcon is false', () => {
    render(<StatusBadge status="Draft" showIcon={false} />);
    expect(screen.getByText('Draft')).toBeInTheDocument();
  });

  it('handles unknown status gracefully', () => {
    render(<StatusBadge status="Unknown" />);
    expect(screen.getByText('Unknown')).toBeInTheDocument();
  });
});

describe('ComplianceBadge', () => {
  it('renders Verified status', () => {
    render(<ComplianceBadge status="Verified" />);
    expect(screen.getByText('Verified')).toBeInTheDocument();
  });

  it('renders Pending status', () => {
    render(<ComplianceBadge status="Pending" />);
    expect(screen.getByText('Pending')).toBeInTheDocument();
  });

  it('renders Failed status', () => {
    render(<ComplianceBadge status="Failed" />);
    expect(screen.getByText('Failed')).toBeInTheDocument();
  });

  it('renders Unknown status as Not Verified', () => {
    render(<ComplianceBadge status="Unknown" />);
    expect(screen.getByText('Not Verified')).toBeInTheDocument();
  });

  it('renders without label when showLabel is false', () => {
    render(<ComplianceBadge status="Verified" showLabel={false} />);
    expect(screen.queryByText('Verified')).not.toBeInTheDocument();
  });
});

describe('MetrcBadge', () => {
  it('renders Synced status', () => {
    render(<MetrcBadge status="Synced" />);
    expect(screen.getByText('Synced')).toBeInTheDocument();
  });

  it('renders Pending status', () => {
    render(<MetrcBadge status="Pending" />);
    expect(screen.getByText('Pending')).toBeInTheDocument();
  });

  it('renders Failed status', () => {
    render(<MetrcBadge status="Failed" />);
    expect(screen.getByText('Failed')).toBeInTheDocument();
  });

  it('renders null status as dash', () => {
    render(<MetrcBadge status={null} />);
    expect(screen.getByText('—')).toBeInTheDocument();
  });
});
