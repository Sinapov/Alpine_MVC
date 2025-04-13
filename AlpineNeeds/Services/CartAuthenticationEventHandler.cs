using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AlpineNeeds.Models;

namespace AlpineNeeds.Services
{
    public class CartAuthenticationEventHandler : IAuthenticationHandler
    {
        private readonly ICartService _cartService;

        public CartAuthenticationEventHandler(ICartService cartService)
        {
            _cartService = cartService;
        }

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            return Task.CompletedTask;
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            return Task.FromResult(AuthenticateResult.Fail("Not implemented"));
        }

        public Task ChallengeAsync(AuthenticationProperties? properties)
        {
            return Task.CompletedTask;
        }

        public Task ForbidAsync(AuthenticationProperties? properties)
        {
            return Task.CompletedTask;
        }
    }

    public static class CartIdentityExtensions
    {
        public static IdentityBuilder AddCartMergeOnLogin(this IdentityBuilder builder)
        {
            builder.Services.Configure<SecurityStampValidatorOptions>(options =>
            {
                // Enable the validation of the security stamp in the cookie when the user logs in
                options.ValidationInterval = TimeSpan.Zero;
            });

            builder.AddSignInManager<CartSignInManager<ApplicationUser>>();
            return builder;
        }
    }

    public class CartSignInManager<TUser> : SignInManager<TUser> where TUser : class
    {
        private readonly ICartService _cartService;

        public CartSignInManager(
            UserManager<TUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<TUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<TUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<TUser> confirmation,
            ICartService cartService)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
        {
            _cartService = cartService;
        }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var result = await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
            
            if (result.Succeeded)
            {
                // After successful login, merge the session cart with the database cart
                await _cartService.MergeCartAsync();
            }
            
            return result;
        }

        public override async Task<SignInResult> PasswordSignInAsync(TUser user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var result = await base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
            
            if (result.Succeeded)
            {
                // After successful login, merge the session cart with the database cart
                await _cartService.MergeCartAsync();
            }
            
            return result;
        }
    }
}