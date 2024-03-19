using FreeCourse.Web.Models;

namespace FreeCourse.Web.Services.Abstract
{
    public interface IUserService
    {
        Task<UserViewModel> GetUser();
    }
}
