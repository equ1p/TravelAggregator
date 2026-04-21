using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace TravelAggregator.Domain.Entities
{
  public class CustomerLead : BaseEntity
  {
    public string FullName { get; private set; }
    public string Email { get; private set; }
    public LeadStatus Status {  get; private set; }
    public int Score { get; private set; }

    public CustomerLead(string fullName, string email)
    {
      if (string.IsNullOrWhiteSpace(fullName))
        throw new ArgumentException("Name cannot be empty.");

      FullName = fullName;
      Email = email;
      Status = LeadStatus.New;
      Score = 0;
    }

    public void AddScore(int points)
    {
      Score += points;
      if (Score >= 50 && Status == LeadStatus.New)
      {
        Status = LeadStatus.MQL;
      }
      else if (Score >= 100 && Status == LeadStatus.MQL)
      {
        Status = LeadStatus.SQL;
      }
    }
  }

  public enum LeadStatus
  {
    New,
    MQL,
    SQL,
    Converted,
    Lost
  }
}
