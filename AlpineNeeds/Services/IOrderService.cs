using System.Threading.Tasks;
using AlpineNeeds.Models;

namespace AlpineNeeds.Services
{
    public interface IOrderService
    {
        Task<int> PlaceOrderAsync(string userId, CheckoutAddressViewModel shipping, CheckoutAddressViewModel billing);
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
    }
}
