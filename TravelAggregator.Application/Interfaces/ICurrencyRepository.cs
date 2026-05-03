using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TravelAggregator.Application.DTOs;

namespace TravelAggregator.Application.Interfaces
{
  public interface ICurrencyRepository
  {
    Task SaveRatesSnapshotAsync(ExchangeRatesDto ratesDto);
  }
}
