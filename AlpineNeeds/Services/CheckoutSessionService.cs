using System.Text.Json;
using System.Threading.Tasks;
using AlpineNeeds.Models;
using Microsoft.AspNetCore.Http;

namespace AlpineNeeds.Services
{
    public class CheckoutSessionService : ICheckoutSessionService
    {
        private readonly ISession _session;
        private const string ShippingKey = "Checkout_ShippingAddress";
        private const string BillingKey = "Checkout_BillingAddress";

        public CheckoutSessionService(IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext!.Session;
        }

        public Task SaveShippingAddressAsync(CheckoutAddressViewModel address)
        {
            _session.SetString(ShippingKey, JsonSerializer.Serialize(address));
            return Task.CompletedTask;
        }

        public Task<CheckoutAddressViewModel?> GetShippingAddressAsync()
        {
            var json = _session.GetString(ShippingKey);
            return Task.FromResult(json == null ? null : JsonSerializer.Deserialize<CheckoutAddressViewModel>(json));
        }

        public Task SaveBillingAddressAsync(CheckoutAddressViewModel address)
        {
            _session.SetString(BillingKey, JsonSerializer.Serialize(address));
            return Task.CompletedTask;
        }

        public Task<CheckoutAddressViewModel?> GetBillingAddressAsync()
        {
            var json = _session.GetString(BillingKey);
            return Task.FromResult(json == null ? null : JsonSerializer.Deserialize<CheckoutAddressViewModel>(json));
        }
    }
}
