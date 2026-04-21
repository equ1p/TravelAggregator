using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAggregator.Domain.Entities
{
  public class TravelPolicy : BaseEntity
  {
    public Guid LeadId { get; private set; }
    public string Destination { get; private set; }
    public decimal Premium { get; private set; }

    public TravelPolicy(Guid leadId, string destination, decimal premium)
    {
      LeadId = leadId;
      Destination = destination;
      Premium = premium;
    }
  }
}
