namespace Customer.Domain.ValueObjects;

public class Address : IEquatable<Address>
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }

    private Address() { 
        Street = string.Empty;
        City = string.Empty;
        State = string.Empty;
        PostalCode = string.Empty;
        Country = string.Empty;
    }

    public Address(string street, string city, string state, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street is required", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required", nameof(country));

        Street = street;
        City = city;
        State = state ?? string.Empty;
        PostalCode = postalCode ?? string.Empty;
        Country = country;
    }

    public bool Equals(Address? other)
    {
        if (other is null) return false;
        return Street == other.Street &&
               City == other.City &&
               State == other.State &&
               PostalCode == other.PostalCode &&
               Country == other.Country;
    }

    public override bool Equals(object? obj) => Equals(obj as Address);
    public override int GetHashCode() => HashCode.Combine(Street, City, State, PostalCode, Country);
    public override string ToString() => $"{Street}, {City}, {State} {PostalCode}, {Country}";
}
