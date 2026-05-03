using System;
using System.Text;
using System.Threading.Tasks;
using TravelAggregator.Application.DTOs;

namespace TravelAggregator.Application.Interfaces
{
  public interface ICurrencyRateProvider
  {
    Task<ExchangeRatesDto> GetLatestRatesAsync();
  }
}
