using ForumProject.Data.Models;
using Microsoft.AspNetCore.Http;

namespace ForumProject.Services.Interfaces
{
    public interface IUserFingerprintService
    {
        Task<UserFingerprint> GetOrCreateFingerprintAsync(HttpContext httpContext);
    }
}