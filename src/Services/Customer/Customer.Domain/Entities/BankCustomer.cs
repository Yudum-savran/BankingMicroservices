using Customer.Domain.Common;
using Customer.Domain.ValueObjects;

namespace Customer.Domain.Entities;

public class BankCustomer : AggregateRoot
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public Address Address { get; private set; }
    public string IdentityNumber { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public CustomerStatus Status { get; private set; }
    public KYCLevel KYCLevel { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastUpdatedAt { get; private set; }

    private BankCustomer() 
    { 
        Address = new Address("", "", "", "", "");
    }

    public static BankCustomer Create(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        Address address,
        string identityNumber,
        DateTime dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));
        if (string.IsNullOrWhiteSpace(identityNumber))
            throw new ArgumentException("Identity number is required", nameof(identityNumber));
        if (dateOfBirth > DateTime.UtcNow.AddYears(-18))
            throw new ArgumentException("Customer must be at least 18 years old", nameof(dateOfBirth));

        return new BankCustomer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email.ToLowerInvariant(),
            PhoneNumber = phoneNumber,
            Address = address,
            IdentityNumber = identityNumber,
            DateOfBirth = dateOfBirth,
            Status = CustomerStatus.Active,
            KYCLevel = KYCLevel.Basic,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateContactInfo(string email, string phoneNumber, Address address)
    {
        if (!string.IsNullOrWhiteSpace(email) && IsValidEmail(email))
            Email = email.ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(phoneNumber))
            PhoneNumber = phoneNumber;
        if (address != null)
            Address = address;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void UpgradeKYCLevel(KYCLevel newLevel)
    {
        if (newLevel <= KYCLevel)
            throw new InvalidOperationException("Cannot downgrade KYC level");
        KYCLevel = newLevel;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void Suspend() { Status = CustomerStatus.Suspended; LastUpdatedAt = DateTime.UtcNow; }
    public void Activate() { Status = CustomerStatus.Active; LastUpdatedAt = DateTime.UtcNow; }
    public string GetFullName() => $"{FirstName} {LastName}";

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch { return false; }
    }
}

public enum CustomerStatus { Active = 1, Suspended = 2, Closed = 3 }
public enum KYCLevel { Basic = 1, Verified = 2, Premium = 3 }
