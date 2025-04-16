using System.Threading.Tasks;
using AlpineNeeds.Models;

namespace AlpineNeeds.Services
{
    public interface ICheckoutSessionService
    {
        Task SaveShippingAddressAsync(CheckoutAddressViewModel address);
        Task<CheckoutAddressViewModel?> GetShippingAddressAsync();
        Task SaveBillingAddressAsync(CheckoutAddressViewModel address);
        Task<CheckoutAddressViewModel?> GetBillingAddressAsync();
    }
}
