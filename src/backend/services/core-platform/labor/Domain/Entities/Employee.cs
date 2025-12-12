using Harvestry.Labor.Domain.Enums;
using Harvestry.Shared.Kernel.Domain;

namespace Harvestry.Labor.Domain.Entities;

public sealed class Employee : AggregateRoot<Guid>
{
    private readonly List<Certification> _certifications = new();
    private readonly List<Skill> _skills = new();

    private Employee(Guid id) : base(id) { }

    private Employee(
        Guid id,
        Guid siteId,
        string fullName,
        string role,
        PayType payType,
        decimal rate,
        EmploymentStatus status) : base(id)
    {
        SiteId = siteId;
        FullName = fullName.Trim();
        Role = role.Trim();
        PayType = payType;
        Rate = rate;
        Status = status;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid SiteId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public PayType PayType { get; private set; }
    public decimal Rate { get; private set; }
    public EmploymentStatus Status { get; private set; }
    public string? PreferredRooms { get; private set; }
    public string? AvailabilityNotes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<Certification> Certifications => _certifications;
    public IReadOnlyCollection<Skill> Skills => _skills;

    public static Employee Create(
        Guid siteId,
        string fullName,
        string role,
        PayType payType,
        decimal rate,
        EmploymentStatus status)
    {
        return new Employee(Guid.NewGuid(), siteId, fullName, role, payType, rate, status);
    }

    public void AddSkill(string name)
    {
        if (_skills.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _skills.Add(new Skill(name));
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddCertification(string name, DateOnly? expiresOn = null)
    {
        if (_certifications.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _certifications.Add(new Certification(name, expiresOn));
        UpdatedAt = DateTime.UtcNow;
    }
}

public sealed record Skill(string Name);

public sealed record Certification(string Name, DateOnly? ExpiresOn);



