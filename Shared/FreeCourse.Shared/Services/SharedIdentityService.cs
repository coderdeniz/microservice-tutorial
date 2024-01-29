using Microsoft.AspNetCore.Http;
using System.Linq;

namespace FreeCourse.Shared.Services
{
    public class SharedIdentityService : ISharedIdentityService
    {
        public string GetUserId => _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "sub").Value;

        private IHttpContextAccessor _httpContextAccessor;

        public SharedIdentityService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
    }
}
