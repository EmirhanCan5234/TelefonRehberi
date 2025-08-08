using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TelefonRehberi.Models;

public partial class UygulamaDbContext : DbContext
{
    public UygulamaDbContext()
    {
    }

    public UygulamaDbContext(DbContextOptions<UygulamaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Kisiler> Kisilers { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-CHEOOOO\\SQLEXPRESS;Database=TelefonRehberi;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kisiler>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Kisiler__3214EC0795ABB4EF");

            entity.ToTable("Kisiler");

            entity.Property(e => e.Ad).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Soyad).HasMaxLength(50);
            entity.Property(e => e.Telefon).HasMaxLength(20);

            entity.HasOne(d => d.Kullanici).WithMany(p => p.Kisilers)
                .HasForeignKey(d => d.KullaniciId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Kisiler__Kullani__398D8EEE");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Logs__3214EC0737FC87ED");

            entity.Property(e => e.Islem).HasMaxLength(100);
            entity.Property(e => e.Tarih).HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07FCA9DBF0");

            entity.Property(e => e.Ad).HasMaxLength(50);
            entity.Property(e => e.KullaniciAdi).HasMaxLength(50);
            entity.Property(e => e.Sifre).HasMaxLength(100);
            entity.Property(e => e.Soyad).HasMaxLength(50);
            entity.Property(e => e.Telefon).HasMaxLength(20);
            entity.Property(e => e.Tema)
                .HasMaxLength(20)
                .HasDefaultValue("light");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
