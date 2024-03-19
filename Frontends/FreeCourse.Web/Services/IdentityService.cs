using FreeCourse.Shared.Dtos;
using FreeCourse.Web.Models;
using FreeCourse.Web.Services.Abstract;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace FreeCourse.Web.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClientSettings _clientSettings;
        private readonly ServiceApiSettings _serviceApiSettings;

        public IdentityService(HttpClient client, IHttpContextAccessor httpContextAccessor, IOptions<ClientSettings> clientSettings, IOptions<ServiceApiSettings> serviceApiSettings)
        {
            _httpClient = client;
            _httpContextAccessor = httpContextAccessor;
            _clientSettings = clientSettings.Value;
            _serviceApiSettings = serviceApiSettings.Value;
        }

        public async Task<TokenResponse> GetAccessTokenByRefreshToken()
        {
            // identityserver extension method
            var disco = await _httpClient.GetDiscoveryDocumentAsync(
                new DiscoveryDocumentRequest
                {
                    Address = _serviceApiSettings.IdentityBaseUri,
                    Policy = new DiscoveryPolicy
                    {
                        RequireHttps = false
                    }
                });

            if (disco.IsError)
            {
                throw disco.Exception;
            }

            string refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            RefreshTokenRequest refreshTokenRequest = new()
            {
                ClientId = _clientSettings.WebMvcClientForUser.ClientId,
                ClientSecret = _clientSettings.WebMvcClientForUser.ClientSecret,
                Address = disco.TokenEndpoint,
                RefreshToken = refreshToken
            };

            var token = await _httpClient.RequestRefreshTokenAsync(refreshTokenRequest);

            if (token.IsError)
            {
                return null;
            }

            var authenticationTokens = new List<AuthenticationToken> { new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.AccessToken,
                Value = token.AccessToken
            },
            new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.RefreshToken,
                Value = token.RefreshToken
            },
            new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.ExpiresIn,
                Value = DateTime.Now.AddSeconds(token.ExpiresIn).ToString("o", CultureInfo.InvariantCulture)
            },
            };

            var authenticationResult = await _httpContextAccessor.HttpContext.AuthenticateAsync();

            var props = authenticationResult.Properties;
            
            props.StoreTokens(authenticationTokens);

            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                authenticationResult.Principal,props);

            return token;
        }

        public async Task RevokeRefreshToken()
        {
            // identityserver extension method
            var disco = await _httpClient.GetDiscoveryDocumentAsync(
                new DiscoveryDocumentRequest
                {
                    Address = _serviceApiSettings.IdentityBaseUri,
                    Policy = new DiscoveryPolicy
                    {
                        RequireHttps = false
                    }
                });

            if (disco.IsError)
            {
                throw disco.Exception;
            }

            var refreshToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.RefreshToken);

            TokenRevocationRequest tokenRevocationRequest = new()
            {
               ClientId = _clientSettings.WebMvcClientForUser.ClientId,
               ClientSecret = _clientSettings.WebMvcClientForUser.ClientSecret,
               Address = disco.RevocationEndpoint,
               Token = refreshToken,
               TokenTypeHint = "refresh_token"
            };

            await _httpClient.RevokeTokenAsync(tokenRevocationRequest);
        }

        public async Task<Response<bool>> SignIn(SigninInput signinInput)
        {
            // identityserver extension method
            var disco = await _httpClient.GetDiscoveryDocumentAsync(
                new DiscoveryDocumentRequest
                {
                    Address = _serviceApiSettings.IdentityBaseUri,
                    Policy = new DiscoveryPolicy
                    {
                        RequireHttps = false
                    }
                });

            if (disco.IsError)
            {
                throw disco.Exception;
            }

            var passwordTokenRequest = new PasswordTokenRequest
            {
                ClientId = _clientSettings.WebMvcClientForUser.ClientId,
                ClientSecret = _clientSettings.WebMvcClientForUser.ClientSecret,
                UserName = signinInput.Email,
                Password = signinInput.Password,
                Address = disco.TokenEndpoint
            };

            var token = await _httpClient.RequestPasswordTokenAsync(passwordTokenRequest);

            if (token.IsError)
            {
                var responsContent = await token.HttpResponse.Content.ReadAsStringAsync();

                var errorDto = JsonSerializer.Deserialize<ErrorDto>(responsContent,new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return Response<bool>.Fail(errorDto.Errors, 400);
            }

            var userInfoRequest = new UserInfoRequest
            {
                Token = token.AccessToken,
                Address = disco.UserInfoEndpoint
            };

            var userInfo = await _httpClient.GetUserInfoAsync(userInfoRequest);

            if (userInfo.IsError)
            {
                throw userInfo.Exception;
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(userInfo.Claims, CookieAuthenticationDefaults.AuthenticationScheme, "name", "role");

            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            var authenticationProperties = new AuthenticationProperties();

            authenticationProperties.StoreTokens(new List<AuthenticationToken> { new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.AccessToken,
                Value = token.AccessToken
            },
            new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.RefreshToken,
                Value = token.RefreshToken
            },
            new AuthenticationToken
            {
                Name = OpenIdConnectParameterNames.ExpiresIn,
                Value = DateTime.Now.AddSeconds(token.ExpiresIn).ToString("o", CultureInfo.InvariantCulture)
            },
            });
            
            authenticationProperties.IsPersistent = signinInput.IsRemember;
            
            await _httpContextAccessor.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal, authenticationProperties);

            return Response<bool>.Success(true,200);
        }
    }
}
