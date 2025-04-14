using System.Threading.Tasks;
using AlpineNeeds.Models;
using AlpineNeeds.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

namespace AlpineNeeds.Pages.Checkout
{
    public static class CheckoutServiceRegistration
    {
        public static IServiceCollection AddCheckoutServices(this IServiceCollection services)
        {
            services.AddScoped<ICheckoutSessionService, CheckoutSessionService>();
            return services;
        }
    }
}
