using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TravelAggregator.Application.Interfaces;
using TravelAggregator.Domain.Entities;

namespace TravelAggregator.Infrastructure.Persistence
{
  public class ApplicationDbContext : DbContext, IApplicationDbContext
  {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<CustomerLead> Leads => Set<CustomerLead>();

    public DbSet<TravelPolicy> Policies => Set<TravelPolicy>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<CustomerLead>().HasKey(e => e.Id);
      modelBuilder.Entity<TravelPolicy>().HasKey(e => e.Id);
      base.OnModelCreating(modelBuilder);
    }
  }
}
