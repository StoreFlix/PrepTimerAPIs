using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PrepTimerAPIs.Models;

public partial class StoreLynkDbProd01Context : DbContext
{
    public StoreLynkDbProd01Context()
    {
    }

   
    public StoreLynkDbProd01Context(DbContextOptions<StoreLynkDbProd01Context> options)
        : base(options)
    {
    }

    public virtual DbSet<PtactiveDevice> PtactiveDevices { get; set; }

    public virtual DbSet<Ptcategory> Ptcategories { get; set; }

    public virtual DbSet<PtcategoryStoreMap> PtcategoryStoreMaps { get; set; }

    public virtual DbSet<Ptcompany> Ptcompanies { get; set; }

    public virtual DbSet<PtcustomerSubscription> PtcustomerSubscriptions { get; set; }

    public virtual DbSet<Ptlanguage> Ptlanguages { get; set; }

    public virtual DbSet<Ptstore> Ptstores { get; set; }

    public virtual DbSet<PTUser> PTUsers { get; set; }

    public virtual DbSet<SlFuseBillKeyValue> SlFuseBillKeyValues { get; set; }

    public virtual DbSet<SlFusebillCouponDetail> SlFusebillCouponDetails { get; set; }

    public virtual DbSet<SlFusebillFranchise> SlFusebillFranchises { get; set; }

    public virtual DbSet<SlFusebillPayment> SlFusebillPayments { get; set; }

    public virtual DbSet<SlFusebillProductMapping> SlFusebillProductMappings { get; set; }

    public virtual DbSet<SlFusebillSubscription> SlFusebillSubscriptions { get; set; }

    public virtual DbSet<SlFusebillSubscriptionDetail> SlFusebillSubscriptionDetails { get; set; }

    public virtual DbSet<PTItem> PTItems { get; set; }
    public virtual DbSet<PTItemTranslationMapping> PTItemTranslationMapping { get; set; }
    public virtual DbSet<PTItemCategoryMapping> PTItemCategoryMapping { get; set; }



    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
       
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

      

        modelBuilder.Entity<PTItem>(entity =>
        {
            entity.HasKey(e => e.ItemId);
            entity.ToTable("PTItem");
        } );

        modelBuilder.Entity<PTItemTranslationMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("PTItemTranslationMapping");
        });

        modelBuilder.Entity<PTItemCategoryMapping>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("PTItemCategoryMapping");
        });
        modelBuilder.Entity<PtactiveDevice>(entity =>
        {
            entity.HasKey(e => e.DeviceId).HasName("PK__PTActive__49E12311BFF09F3D");

            entity.ToTable("PTActiveDevices");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeviceImei)
                .HasMaxLength(500)
                .HasColumnName("DeviceIMEI");
            entity.Property(e => e.DeviceOs)
                .HasMaxLength(500)
                .HasColumnName("DeviceOS");
            entity.Property(e => e.DeviceType).HasMaxLength(500);
            entity.Property(e => e.DeviceUniqueId).HasMaxLength(500);

            entity.HasOne(d => d.Company).WithMany(p => p.PtactiveDevices)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PTActiveD__Compa__7E89C91F");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PtactiveDevices)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__PTActiveD__Creat__00721191");

            entity.HasOne(d => d.Subscription).WithMany(p => p.PtactiveDevices)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK__PTActiveD__Subsc__7F7DED58");
        });

        modelBuilder.Entity<Ptcategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__PTCatego__19093A0B8EA2363B");

            entity.ToTable("PTCategory");

            entity.Property(e => e.CategoryName).HasMaxLength(200);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Company).WithMany(p => p.Ptcategories)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_PTCategory_Company");
        });

        modelBuilder.Entity<PtcategoryStoreMap>(entity =>
        {
            entity.HasKey(e => e.CategoryStoreMapId).HasName("PK__PTCatego__7CBE13A957C6A10F");

            entity.ToTable("PTCategoryStoreMap");

            entity.HasOne(d => d.Category).WithMany(p => p.PtcategoryStoreMaps)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PTCategoryStoreMap_Category");

            entity.HasOne(d => d.Store).WithMany(p => p.PtcategoryStoreMaps)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PTCategoryStoreMap_Store");
        });

        modelBuilder.Entity<Ptcompany>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__PTCompan__2D971CACCCCCE3C2");

            entity.ToTable("PTCompany");

            entity.Property(e => e.Address1).HasMaxLength(500);
            entity.Property(e => e.Address2).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(500);
            entity.Property(e => e.CompanyName).HasMaxLength(500);
            entity.Property(e => e.Country).HasMaxLength(500);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FranchiseCode).HasMaxLength(200);
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Phonenumber).HasMaxLength(20);
            entity.Property(e => e.State).HasMaxLength(500);
            entity.Property(e => e.Zipcode).HasMaxLength(20);
        });

        modelBuilder.Entity<PtcustomerSubscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__PTCustom__9A2B249D6DFC912A");

            entity.ToTable("PTCustomerSubscriptions");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.FbsubscriptionId).HasColumnName("FBSubscriptionId");

            entity.HasOne(d => d.Company).WithMany(p => p.PtcustomerSubscriptions)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PTCustome__Compa__7223F23A");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PtcustomerSubscriptions)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK__PTCustome__Creat__73181673");
        });

        modelBuilder.Entity<Ptlanguage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PTLangua__3214EC07EFC5F49D");

            entity.ToTable("PTLanguages");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.Locale).HasMaxLength(10);
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(500);
        });

        modelBuilder.Entity<Ptstore>(entity =>
        {
            entity.HasKey(e => e.StoreId).HasName("PK__PTStore__3B82F1018D2F9D8A");

            entity.ToTable("PTStore");

            entity.Property(e => e.Address1).HasMaxLength(500);
            entity.Property(e => e.Address2).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(500);
            entity.Property(e => e.Country).HasMaxLength(500);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Phonenumber).HasMaxLength(20);
            entity.Property(e => e.State).HasMaxLength(500);
            entity.Property(e => e.StoreName).HasMaxLength(500);
            entity.Property(e => e.Zipcode).HasMaxLength(20);

            entity.HasOne(d => d.Company).WithMany(p => p.Ptstores)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK__PTStore__Company__64C9F71C");
        });

        modelBuilder.Entity<PTUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__PTUser__1788CC4CB70BBBCA");

            entity.ToTable("PTUser");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(500);
            entity.Property(e => e.FirstName).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(500);
            entity.Property(e => e.LoginName).HasMaxLength(1000);
            entity.Property(e => e.ModifiedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.Phonenumber).HasMaxLength(20);

            entity.HasOne(d => d.Company).WithMany(p => p.PTUsers)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK__PTUser__CompanyI__6A82D072");
        });

        modelBuilder.Entity<SlFuseBillKeyValue>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("SL_FuseBillKeyValue");
        });

        modelBuilder.Entity<SlFusebillCouponDetail>(entity =>
        {
            entity.ToTable("SL_FusebillCouponDetail");

            entity.Property(e => e.CouponCode).HasMaxLength(255);
            entity.Property(e => e.CouponDetailDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.EligibilityEndDate).HasColumnType("datetime");
            entity.Property(e => e.EligibilityStartDate).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(255);
        });

        modelBuilder.Entity<SlFusebillFranchise>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SL_Franchise");

            entity.ToTable("SL_FusebillFranchise");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<SlFusebillPayment>(entity =>
        {
            entity.ToTable("SL_FusebillPayment");

            entity.Property(e => e.EventSource).HasMaxLength(200);
            entity.Property(e => e.EventType).HasMaxLength(200);
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.Result).HasMaxLength(200);
        });

        modelBuilder.Entity<SlFusebillProductMapping>(entity =>
        {
            entity.ToTable("SL_FusebillProductMapping");

            entity.Property(e => e.FranchiseCode).HasMaxLength(50);
            entity.Property(e => e.FusebillProductName).HasMaxLength(255);
            entity.Property(e => e.SubscriptionType).HasMaxLength(255);
        });

        modelBuilder.Entity<SlFusebillSubscription>(entity =>
        {
            entity.ToTable("SL_FusebillSubscription");

            entity.Property(e => e.EventSource).HasMaxLength(200);
            entity.Property(e => e.EventType).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(200);
            entity.Property(e => e.SubscriptionDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<SlFusebillSubscriptionDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_SL_FusebillSubscriptionProduct");

            entity.ToTable("SL_FusebillSubscriptionDetail");

            entity.Property(e => e.Currency).HasMaxLength(50);
            entity.Property(e => e.LastPurchaseDate).HasColumnType("datetime");
            entity.Property(e => e.NextRechargeDate).HasColumnType("datetime");
            entity.Property(e => e.PlanName).HasMaxLength(200);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.SubscriptionDetailDate).HasColumnType("datetime");
        });

      
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
