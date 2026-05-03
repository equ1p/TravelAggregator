using System;
using System.Collections.Generic;
using System.Text;

namespace TravelAggregator.Application.Interfaces
{
  public interface ICurrencyService
  {
    Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
  }
}
