using ShopSphere.Domain.Common;
using ShopSphere.Domain.Exceptions;

namespace ShopSphere.Domain.Entities;

public class Role : BaseEntity
{
    public const string AdminRole = "Admin";
    public const string CustomerRole = "Customer";

    public string Name { get; private set; } = string.Empty;

    // Navigation property
    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    protected Role() { }

    public Role(int id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name cannot be empty.");

        Id = id;
        Name = name.Trim();
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Role name cannot be empty.");

        Name = newName.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
