using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Orders.Models;
using Ordering.Orders.ValueObjects;


namespace Ordering.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.OrderName, on =>
        {
            on.Property(x => x.Value).HasColumnName("OrderName").IsRequired();
        });

        builder.OwnsOne(x => x.ShippingAddress, sa =>
        {
            sa.Property(a => a.FirstName).HasColumnName("ShippingFirstName");
            sa.Property(a => a.LastName).HasColumnName("ShippingLastName");
            sa.Property(a => a.EmailAddress).HasColumnName("ShippingEmail");
            sa.Property(a => a.AddressLine).HasColumnName("ShippingAddressLine");
            sa.Property(a => a.Country).HasColumnName("ShippingCountry");
            sa.Property(a => a.State).HasColumnName("ShippingState");
            sa.Property(a => a.ZipCode).HasColumnName("ShippingZipCode");
        });

        builder.OwnsOne(x => x.BillingAddress, ba =>
        {
            ba.Property(a => a.FirstName).HasColumnName("BillingFirstName");
            ba.Property(a => a.LastName).HasColumnName("BillingLastName");
            ba.Property(a => a.EmailAddress).HasColumnName("BillingEmail");
            ba.Property(a => a.AddressLine).HasColumnName("BillingAddressLine");
            ba.Property(a => a.Country).HasColumnName("BillingCountry");
            ba.Property(a => a.State).HasColumnName("BillingState");
            ba.Property(a => a.ZipCode).HasColumnName("BillingZipCode");
        });

        builder.OwnsOne(x => x.Payment, p =>
        {
            p.Property(x => x.CardName).HasColumnName("CardName");
            p.Property(x => x.CardNumber).HasColumnName("CardNumber");
            p.Property(x => x.Expiration).HasColumnName("Expiration");
            p.Property(x => x.Cvv).HasColumnName("Cvv");
            p.Property(x => x.PaymentMethod).HasColumnName("PaymentMethod");
        });

        builder.Property(x => x.Status).HasConversion<string>();

        builder.OwnsMany(x => x.Items, ib =>
        {
            ib.WithOwner().HasForeignKey("OrderId");
            ib.HasKey(x => x.Id);
        });

        builder.Navigation(x => x.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}