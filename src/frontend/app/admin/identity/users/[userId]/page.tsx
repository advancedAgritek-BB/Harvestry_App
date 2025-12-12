'use client';

import React, { useState, useEffect } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Link from 'next/link';
import {
  User,
  ChevronLeft,
  Save,
  AlertCircle,
  X,
  Plus,
  Trash2,
  DollarSign,
  Award,
  Briefcase,
  MapPin,
  Users,
  Calendar,
  Clock,
  Gift,
  CalendarDays,
  ExternalLink,
  CalendarClock,
} from 'lucide-react';
import {
  AdminCard,
  AdminSection,
  AdminGrid,
  FormField,
  Input,
  Select,
  Textarea,
  Button,
  AdminModal,
  Checkbox,
  StatusBadge,
} from '@/components/admin';
import * as EmployeeService from '@/features/labor/services/employee.service';
import type {
  EmployeeProfile,
  PayType,
  EmployeeStatus,
  TimeOffType,
} from '@/features/labor/types/employee.types';

const PAY_TYPE_OPTIONS = [
  { value: 'hourly', label: 'Hourly' },
  { value: 'salary', label: 'Salary' },
];

const STATUS_OPTIONS = [
  { value: 'active', label: 'Active' },
  { value: 'inactive', label: 'Inactive' },
  { value: 'terminated', label: 'Terminated' },
];

const TIME_OFF_TYPE_OPTIONS = [
  { value: 'pto', label: 'PTO (Paid Time Off)' },
  { value: 'sick', label: 'Sick Leave' },
  { value: 'unpaid', label: 'Unpaid Leave' },
  { value: 'other', label: 'Other' },
];

// Mock site ID - in production this would come from auth context
const CURRENT_SITE_ID = 'site-1';

export default function UserProfilePage() {
  const params = useParams();
  const router = useRouter();
  const userId = params.userId as string;

  const [employee, setEmployee] = useState<EmployeeProfile | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [availableRooms, setAvailableRooms] = useState<string[]>([]);

  // Form state
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [email, setEmail] = useState('');
  const [role, setRole] = useState('');
  const [payType, setPayType] = useState<PayType>('hourly');
  const [rate, setRate] = useState('');
  const [status, setStatus] = useState<EmployeeStatus>('active');
  const [selectedRooms, setSelectedRooms] = useState<string[]>([]);
  const [availabilityNotes, setAvailabilityNotes] = useState('');
  const [birthday, setBirthday] = useState('');

  // Skills modal state
  const [isSkillModalOpen, setIsSkillModalOpen] = useState(false);
  const [newSkill, setNewSkill] = useState('');

  // Certification modal state
  const [isCertModalOpen, setIsCertModalOpen] = useState(false);
  const [newCertName, setNewCertName] = useState('');
  const [newCertExpiry, setNewCertExpiry] = useState('');

  // Time off modal state
  const [isTimeOffModalOpen, setIsTimeOffModalOpen] = useState(false);
  const [timeOffType, setTimeOffType] = useState<TimeOffType>('pto');
  const [timeOffStartDate, setTimeOffStartDate] = useState('');
  const [timeOffEndDate, setTimeOffEndDate] = useState('');
  const [timeOffNotes, setTimeOffNotes] = useState('');
  const [isSubmittingTimeOff, setIsSubmittingTimeOff] = useState(false);

  // Load employee data
  useEffect(() => {
    loadEmployee();
    loadAvailableRooms();
  }, [userId]);

  const loadEmployee = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await EmployeeService.getEmployee(CURRENT_SITE_ID, userId);
      setEmployee(data);
      populateForm(data);
    } catch (err) {
      console.error('Failed to load employee:', err);
      setError('Failed to load employee profile. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  const loadAvailableRooms = async () => {
    try {
      const rooms = await EmployeeService.getAvailableRooms(CURRENT_SITE_ID);
      setAvailableRooms(rooms);
    } catch (err) {
      console.error('Failed to load rooms:', err);
    }
  };

  const populateForm = (data: EmployeeProfile) => {
    setFirstName(data.firstName);
    setLastName(data.lastName);
    setEmail(data.email || '');
    setRole(data.role);
    setPayType(data.payType);
    setRate(data.rate.toString());
    setStatus(data.status);
    setSelectedRooms(data.preferredRooms || []);
    setAvailabilityNotes(data.availabilityNotes || '');
    setBirthday(data.birthday || '');
  };

  const handleSave = async () => {
    if (!firstName.trim() || !lastName.trim() || !role.trim()) {
      setError('Please fill in all required fields.');
      return;
    }

    setIsSaving(true);
    setError(null);
    setSuccessMessage(null);
    try {
      const updated = await EmployeeService.updateEmployee(CURRENT_SITE_ID, userId, {
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        email: email.trim() || undefined,
        role: role.trim(),
        payType,
        rate: parseFloat(rate) || 0,
        status,
        preferredRooms: selectedRooms,
        availabilityNotes: availabilityNotes.trim() || undefined,
        birthday: birthday || undefined,
      });
      setEmployee(updated);
      setSuccessMessage('Profile saved successfully!');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      console.error('Failed to save employee:', err);
      setError('Failed to save changes. Please try again.');
    } finally {
      setIsSaving(false);
    }
  };

  const handleAddSkill = async () => {
    if (!newSkill.trim()) return;
    
    try {
      await EmployeeService.addSkill(CURRENT_SITE_ID, userId, newSkill.trim());
      await loadEmployee();
      setNewSkill('');
      setIsSkillModalOpen(false);
    } catch (err) {
      console.error('Failed to add skill:', err);
      setError('Failed to add skill. Please try again.');
    }
  };

  const handleRemoveSkill = async (skill: string) => {
    try {
      await EmployeeService.removeSkill(CURRENT_SITE_ID, userId, skill);
      await loadEmployee();
    } catch (err) {
      console.error('Failed to remove skill:', err);
      setError('Failed to remove skill. Please try again.');
    }
  };

  const handleAddCertification = async () => {
    if (!newCertName.trim()) return;
    
    try {
      await EmployeeService.addCertification(
        CURRENT_SITE_ID,
        userId,
        newCertName.trim(),
        newCertExpiry || undefined
      );
      await loadEmployee();
      setNewCertName('');
      setNewCertExpiry('');
      setIsCertModalOpen(false);
    } catch (err) {
      console.error('Failed to add certification:', err);
      setError('Failed to add certification. Please try again.');
    }
  };

  const handleRemoveCertification = async (name: string) => {
    try {
      await EmployeeService.removeCertification(CURRENT_SITE_ID, userId, name);
      await loadEmployee();
    } catch (err) {
      console.error('Failed to remove certification:', err);
      setError('Failed to remove certification. Please try again.');
    }
  };

  const handleSubmitTimeOff = async () => {
    if (!timeOffStartDate || !timeOffEndDate) return;
    
    setIsSubmittingTimeOff(true);
    try {
      await EmployeeService.submitTimeOffRequest(CURRENT_SITE_ID, userId, {
        type: timeOffType,
        startDate: timeOffStartDate,
        endDate: timeOffEndDate,
        notes: timeOffNotes.trim() || undefined,
      });
      await loadEmployee();
      setIsTimeOffModalOpen(false);
      setTimeOffType('pto');
      setTimeOffStartDate('');
      setTimeOffEndDate('');
      setTimeOffNotes('');
      setSuccessMessage('Time off request submitted successfully!');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      console.error('Failed to submit time off request:', err);
      setError('Failed to submit time off request. Please try again.');
    } finally {
      setIsSubmittingTimeOff(false);
    }
  };

  const handleCancelTimeOffRequest = async (requestId: string) => {
    try {
      await EmployeeService.cancelTimeOffRequest(CURRENT_SITE_ID, userId, requestId);
      await loadEmployee();
      setSuccessMessage('Time off request cancelled.');
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      console.error('Failed to cancel time off request:', err);
      setError('Failed to cancel request. Please try again.');
    }
  };

  const toggleRoom = (room: string) => {
    setSelectedRooms(prev => 
      prev.includes(room) 
        ? prev.filter(r => r !== room)
        : [...prev, room]
    );
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return 'No expiration';
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
    });
  };

  const formatShortDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
    });
  };

  const formatBirthday = (dateString?: string) => {
    if (!dateString) return 'Not set';
    return new Date(dateString + 'T00:00:00').toLocaleDateString('en-US', {
      month: 'long',
      day: 'numeric',
    });
  };

  const getTimeOffStatusBadge = (status: string) => {
    switch (status) {
      case 'approved': return 'active';
      case 'pending': return 'warning';
      case 'denied': return 'error';
      default: return 'inactive';
    }
  };

  const getTimeOffTypeLabel = (type: string) => {
    switch (type) {
      case 'pto': return 'PTO';
      case 'sick': return 'Sick';
      case 'unpaid': return 'Unpaid';
      default: return 'Other';
    }
  };

  const isExpiringSoon = (dateString?: string) => {
    if (!dateString) return false;
    const expiryDate = new Date(dateString);
    const thirtyDaysFromNow = new Date();
    thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30);
    return expiryDate <= thirtyDaysFromNow;
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-12">
        <div className="animate-spin w-8 h-8 border-2 border-violet-500 border-t-transparent rounded-full" />
      </div>
    );
  }

  if (!employee) {
    return (
      <div className="text-center py-12">
        <User className="w-12 h-12 text-muted-foreground mx-auto mb-3" />
        <p className="text-muted-foreground">Employee not found</p>
        <Button variant="ghost" className="mt-4" onClick={() => router.back()}>
          <ChevronLeft className="w-4 h-4" />
          Go Back
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <button
        onClick={() => router.back()}
        className="flex items-center gap-2 text-sm text-muted-foreground hover:text-foreground transition-colors"
      >
        <ChevronLeft className="w-4 h-4" />
        Back
      </button>

      {/* Error/Success Banners */}
      {error && (
        <div className="flex items-center justify-between gap-3 px-4 py-3 bg-rose-500/10 border border-rose-500/30 rounded-lg">
          <div className="flex items-center gap-3">
            <AlertCircle className="w-5 h-5 text-rose-400 flex-shrink-0" />
            <span className="text-sm text-rose-300">{error}</span>
          </div>
          <button onClick={() => setError(null)} className="p-1 hover:bg-rose-500/20 rounded transition-colors" aria-label="Dismiss error">
            <X className="w-4 h-4 text-rose-400" />
          </button>
        </div>
      )}

      {successMessage && (
        <div className="flex items-center gap-3 px-4 py-3 bg-emerald-500/10 border border-emerald-500/30 rounded-lg">
          <span className="text-sm text-emerald-300">{successMessage}</span>
        </div>
      )}

      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <div className="w-16 h-16 rounded-full bg-gradient-to-br from-violet-400 to-purple-600 flex items-center justify-center text-white text-xl font-bold">
            {employee.firstName[0]}{employee.lastName[0]}
          </div>
          <div>
            <h1 className="text-xl font-semibold text-foreground">
              {employee.firstName} {employee.lastName}
            </h1>
            <p className="text-sm text-muted-foreground">{employee.role}</p>
          </div>
        </div>
        <Button onClick={handleSave} disabled={isSaving}>
          <Save className="w-4 h-4" />
          {isSaving ? 'Saving...' : 'Save Changes'}
        </Button>
      </div>

      <AdminGrid columns={2}>
        {/* Basic Information */}
        <AdminSection title="Basic Information">
          <AdminCard title="Personal Details" icon={User}>
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <FormField label="First Name" required>
                  <Input value={firstName} onChange={(e) => setFirstName(e.target.value)} />
                </FormField>
                <FormField label="Last Name" required>
                  <Input value={lastName} onChange={(e) => setLastName(e.target.value)} />
                </FormField>
              </div>
              <FormField label="Email">
                <Input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
              </FormField>
              <FormField label="Role" required>
                <Input value={role} onChange={(e) => setRole(e.target.value)} placeholder="e.g., Grower, Technician" />
              </FormField>
              <div className="grid grid-cols-2 gap-4">
                <FormField label="Birthday">
                  <Input type="date" value={birthday} onChange={(e) => setBirthday(e.target.value)} />
                </FormField>
                <FormField label="Status">
                  <Select
                    value={status}
                    onChange={(e) => setStatus(e.target.value as EmployeeStatus)}
                    options={STATUS_OPTIONS}
                  />
                </FormField>
              </div>
              {employee.hireDate && (
                <div className="pt-2 border-t border-white/[0.06]">
                  <p className="text-xs text-muted-foreground">
                    <CalendarDays className="w-3 h-3 inline mr-1" />
                    Hired: {formatDate(employee.hireDate)}
                  </p>
                </div>
              )}
            </div>
          </AdminCard>
        </AdminSection>

        {/* Labor Information & PTO */}
        <AdminSection title="Labor Information">
          <AdminCard title="Compensation" icon={DollarSign}>
            <div className="space-y-4">
              <FormField label="Pay Type">
                <Select
                  value={payType}
                  onChange={(e) => setPayType(e.target.value as PayType)}
                  options={PAY_TYPE_OPTIONS}
                />
              </FormField>
              <FormField label={payType === 'hourly' ? 'Hourly Rate ($)' : 'Annual Salary ($)'}>
                <Input
                  type="number"
                  step={payType === 'hourly' ? '0.01' : '1000'}
                  value={rate}
                  onChange={(e) => setRate(e.target.value)}
                  placeholder={payType === 'hourly' ? '0.00' : '0'}
                />
              </FormField>
            </div>
          </AdminCard>
          
          {/* PTO Balance */}
          <AdminCard title="PTO Balance" icon={CalendarClock} className="mt-4">
            <div className="grid grid-cols-4 gap-3">
              <div className="text-center p-3 bg-emerald-500/10 rounded-lg">
                <p className="text-2xl font-bold text-emerald-400">{employee.ptoBalance.available}</p>
                <p className="text-xs text-muted-foreground">Available</p>
              </div>
              <div className="text-center p-3 bg-white/[0.04] rounded-lg">
                <p className="text-2xl font-bold text-foreground">{employee.ptoBalance.used}</p>
                <p className="text-xs text-muted-foreground">Used</p>
              </div>
              <div className="text-center p-3 bg-white/[0.04] rounded-lg">
                <p className="text-2xl font-bold text-foreground">{employee.ptoBalance.accrued}</p>
                <p className="text-xs text-muted-foreground">Accrued</p>
              </div>
              <div className="text-center p-3 bg-amber-500/10 rounded-lg">
                <p className="text-2xl font-bold text-amber-400">{employee.ptoBalance.pending}</p>
                <p className="text-xs text-muted-foreground">Pending</p>
              </div>
            </div>
            <p className="text-xs text-muted-foreground mt-3 text-center">Hours (8 hours = 1 day)</p>
          </AdminCard>
        </AdminSection>
      </AdminGrid>

      {/* Upcoming Schedule */}
      <AdminSection title="Upcoming Schedule">
        <AdminCard
          title="Next 7 Days"
          icon={Clock}
          actions={
            <Link
              href="/dashboard/planner/shift-board"
              className="flex items-center gap-1.5 text-xs text-violet-400 hover:text-violet-300 transition-colors"
            >
              View Full Schedule
              <ExternalLink className="w-3 h-3" />
            </Link>
          }
        >
          {employee.upcomingShifts.length === 0 ? (
            <p className="text-sm text-muted-foreground text-center py-4">No upcoming shifts scheduled</p>
          ) : (
            <div className="grid gap-2 md:grid-cols-2 lg:grid-cols-3">
              {employee.upcomingShifts.map((shift) => (
                <div
                  key={shift.id}
                  className="flex items-center justify-between p-3 bg-white/[0.02] border border-white/[0.06] rounded-lg"
                >
                  <div>
                    <div className="font-medium text-foreground">{formatShortDate(shift.date)}</div>
                    <div className="text-xs text-muted-foreground">
                      {shift.startTime} - {shift.endTime}
                    </div>
                    {shift.location && (
                      <div className="text-xs text-cyan-400 mt-1">
                        <MapPin className="w-3 h-3 inline mr-1" />
                        {shift.location}
                      </div>
                    )}
                  </div>
                  <StatusBadge
                    status={shift.status === 'scheduled' ? 'active' : shift.status === 'completed' ? 'inactive' : 'error'}
                    label={shift.status}
                  />
                </div>
              ))}
            </div>
          )}
        </AdminCard>
      </AdminSection>

      <AdminGrid columns={2}>
        {/* Skills */}
        <AdminSection title="Skills & Expertise">
          <AdminCard
            title="Skills"
            icon={Briefcase}
            actions={
              <Button size="sm" variant="secondary" onClick={() => setIsSkillModalOpen(true)}>
                <Plus className="w-4 h-4" />
                Add Skill
              </Button>
            }
          >
            {employee.skills.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-4">No skills added yet</p>
            ) : (
              <div className="flex flex-wrap gap-2">
                {employee.skills.map((skill) => (
                  <span
                    key={skill}
                    className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-violet-500/10 text-violet-300 rounded-full text-sm"
                  >
                    {skill}
                    <button
                      onClick={() => handleRemoveSkill(skill)}
                      className="p-0.5 hover:bg-violet-500/20 rounded-full transition-colors"
                      aria-label={`Remove ${skill} skill`}
                    >
                      <X className="w-3 h-3" />
                    </button>
                  </span>
                ))}
              </div>
            )}
          </AdminCard>
        </AdminSection>

        {/* Certifications */}
        <AdminSection title="Certifications">
          <AdminCard
            title="Certifications"
            icon={Award}
            actions={
              <Button size="sm" variant="secondary" onClick={() => setIsCertModalOpen(true)}>
                <Plus className="w-4 h-4" />
                Add Certification
              </Button>
            }
          >
            {employee.certifications.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-4">No certifications added yet</p>
            ) : (
              <div className="space-y-2">
                {employee.certifications.map((cert) => (
                  <div
                    key={cert.name}
                    className="flex items-center justify-between p-3 bg-white/[0.02] border border-white/[0.06] rounded-lg"
                  >
                    <div>
                      <div className="font-medium text-foreground">{cert.name}</div>
                      <div className={`text-xs ${isExpiringSoon(cert.expiresOn) ? 'text-amber-400' : 'text-muted-foreground'}`}>
                        <Calendar className="w-3 h-3 inline mr-1" />
                        {formatDate(cert.expiresOn)}
                        {isExpiringSoon(cert.expiresOn) && ' (Expiring Soon)'}
                      </div>
                    </div>
                    <button
                      onClick={() => handleRemoveCertification(cert.name)}
                      className="p-1.5 text-muted-foreground hover:text-rose-400 hover:bg-rose-500/10 rounded-lg transition-colors"
                      aria-label={`Remove ${cert.name} certification`}
                    >
                      <Trash2 className="w-4 h-4" />
                    </button>
                  </div>
                ))}
              </div>
            )}
          </AdminCard>
        </AdminSection>
      </AdminGrid>

      <AdminGrid columns={2}>
        {/* Preferred Rooms */}
        <AdminSection title="Work Preferences">
          <AdminCard title="Preferred Rooms" icon={MapPin}>
            <div className="space-y-2">
              {availableRooms.map((room) => (
                <Checkbox
                  key={room}
                  checked={selectedRooms.includes(room)}
                  onChange={() => toggleRoom(room)}
                  label={room}
                />
              ))}
            </div>
          </AdminCard>
        </AdminSection>

        {/* Time Off Requests */}
        <AdminSection title="Time Off">
          <AdminCard
            title="Time Off Requests"
            icon={CalendarDays}
            actions={
              <Button size="sm" variant="secondary" onClick={() => setIsTimeOffModalOpen(true)}>
                <Plus className="w-4 h-4" />
                Request Time Off
              </Button>
            }
          >
            {employee.timeOffRequests.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-4">No time off requests</p>
            ) : (
              <div className="space-y-2">
                {employee.timeOffRequests.map((request) => (
                  <div
                    key={request.id}
                    className="flex items-center justify-between p-3 bg-white/[0.02] border border-white/[0.06] rounded-lg"
                  >
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="font-medium text-foreground">
                          {formatShortDate(request.startDate)}
                          {request.startDate !== request.endDate && ` - ${formatShortDate(request.endDate)}`}
                        </span>
                        <span className="text-xs text-muted-foreground bg-white/[0.04] px-2 py-0.5 rounded">
                          {getTimeOffTypeLabel(request.type)}
                        </span>
                      </div>
                      {request.notes && (
                        <p className="text-xs text-muted-foreground mt-1">{request.notes}</p>
                      )}
                    </div>
                    <div className="flex items-center gap-2">
                      <StatusBadge status={getTimeOffStatusBadge(request.status)} label={request.status} />
                      {request.status === 'pending' && (
                        <button
                          onClick={() => handleCancelTimeOffRequest(request.id)}
                          className="p-1 text-muted-foreground hover:text-rose-400 hover:bg-rose-500/10 rounded transition-colors"
                          aria-label="Cancel request"
                        >
                          <X className="w-4 h-4" />
                        </button>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
            <div className="pt-3 mt-3 border-t border-white/[0.06]">
              <Link
                href="/dashboard/planner/time-approvals"
                className="flex items-center gap-1.5 text-xs text-violet-400 hover:text-violet-300 transition-colors"
              >
                Manage Time in Planner
                <ExternalLink className="w-3 h-3" />
              </Link>
            </div>
          </AdminCard>
        </AdminSection>
      </AdminGrid>

      <AdminGrid columns={2}>
        {/* Availability */}
        <AdminSection title="Availability">
          <AdminCard title="Availability Notes" icon={Calendar}>
            <FormField label="Notes">
              <Textarea
                rows={3}
                value={availabilityNotes}
                onChange={(e) => setAvailabilityNotes(e.target.value)}
                placeholder="e.g., Prefers morning shifts, available weekends"
              />
            </FormField>
          </AdminCard>
        </AdminSection>

        {/* Teams */}
        <AdminSection title="Team Memberships">
          <AdminCard title="Teams" icon={Users}>
            {employee.teams.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-4">Not assigned to any teams</p>
            ) : (
              <div className="space-y-2">
                {employee.teams.map((team) => (
                  <div
                    key={team.id}
                    className="flex items-center justify-between p-3 bg-white/[0.02] border border-white/[0.06] rounded-lg"
                  >
                    <span className="font-medium text-foreground">{team.name}</span>
                    {team.isTeamLead && (
                      <span className="text-xs text-amber-400 bg-amber-500/10 px-2 py-0.5 rounded-full">
                        Team Lead
                      </span>
                    )}
                  </div>
                ))}
              </div>
            )}
          </AdminCard>
        </AdminSection>
      </AdminGrid>

      {/* Add Skill Modal */}
      <AdminModal
        isOpen={isSkillModalOpen}
        onClose={() => {
          setIsSkillModalOpen(false);
          setNewSkill('');
        }}
        title="Add Skill"
        description="Add a new skill to this employee's profile"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsSkillModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleAddSkill} disabled={!newSkill.trim()}>
              Add Skill
            </Button>
          </>
        }
      >
        <FormField label="Skill Name" required>
          <Input
            value={newSkill}
            onChange={(e) => setNewSkill(e.target.value)}
            placeholder="e.g., Irrigation Management, IPM, Nutrient Mixing"
          />
        </FormField>
      </AdminModal>

      {/* Add Certification Modal */}
      <AdminModal
        isOpen={isCertModalOpen}
        onClose={() => {
          setIsCertModalOpen(false);
          setNewCertName('');
          setNewCertExpiry('');
        }}
        title="Add Certification"
        description="Add a new certification to this employee's profile"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsCertModalOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleAddCertification} disabled={!newCertName.trim()}>
              Add Certification
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <FormField label="Certification Name" required>
            <Input
              value={newCertName}
              onChange={(e) => setNewCertName(e.target.value)}
              placeholder="e.g., Pesticide Applicator License"
            />
          </FormField>
          <FormField label="Expiration Date">
            <Input
              type="date"
              value={newCertExpiry}
              onChange={(e) => setNewCertExpiry(e.target.value)}
            />
          </FormField>
        </div>
      </AdminModal>

      {/* Request Time Off Modal */}
      <AdminModal
        isOpen={isTimeOffModalOpen}
        onClose={() => {
          if (!isSubmittingTimeOff) {
            setIsTimeOffModalOpen(false);
            setTimeOffType('pto');
            setTimeOffStartDate('');
            setTimeOffEndDate('');
            setTimeOffNotes('');
          }
        }}
        title="Request Time Off"
        description="Submit a new time off request"
        footer={
          <>
            <Button variant="ghost" onClick={() => setIsTimeOffModalOpen(false)} disabled={isSubmittingTimeOff}>
              Cancel
            </Button>
            <Button
              onClick={handleSubmitTimeOff}
              disabled={!timeOffStartDate || !timeOffEndDate || isSubmittingTimeOff}
            >
              {isSubmittingTimeOff ? 'Submitting...' : 'Submit Request'}
            </Button>
          </>
        }
      >
        <div className="space-y-4">
          <FormField label="Type" required>
            <Select
              value={timeOffType}
              onChange={(e) => setTimeOffType(e.target.value as TimeOffType)}
              options={TIME_OFF_TYPE_OPTIONS}
            />
          </FormField>
          <div className="grid grid-cols-2 gap-4">
            <FormField label="Start Date" required>
              <Input
                type="date"
                value={timeOffStartDate}
                onChange={(e) => setTimeOffStartDate(e.target.value)}
              />
            </FormField>
            <FormField label="End Date" required>
              <Input
                type="date"
                value={timeOffEndDate}
                onChange={(e) => setTimeOffEndDate(e.target.value)}
                min={timeOffStartDate}
              />
            </FormField>
          </div>
          <FormField label="Notes (optional)">
            <Textarea
              rows={2}
              value={timeOffNotes}
              onChange={(e) => setTimeOffNotes(e.target.value)}
              placeholder="Reason for time off request"
            />
          </FormField>
        </div>
      </AdminModal>
    </div>
  );
}
