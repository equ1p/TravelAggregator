using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using TravelAggregator.Domain.Entities;

namespace TravelAggregator.Application.Interfaces
{
  public interface IApplicationDbContext
  {
    DbSet<CustomerLead> Leads { get; }
    DbSet<TravelPolicy> Policies { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
  }
}
