using forecast_api.Models.Dto;

namespace forecast_api.Services
{
    /// <summary>
    /// An interface that is responsible for authenticating a given auth request
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Validates whether the user is authenticated
        /// </summary>
        /// <param name="request">The request to authenticate.</param>
        /// <returns>True if the user is authenticated; false otherwise</returns>
        public bool IsAuthenticated(AuthRequest request);
    }

    public class DummyAuthenticationService : IAuthenticationService
    {
        // Don't actually do this with an actual API
        // But for the sake of what we're doing, as long as they provide this password, we'll let them in
        private const string HardcodedPassword = "Sup3rSe_cr3tP@ssw0rd";
        public bool IsAuthenticated(AuthRequest request)
        {
            if (string.Equals(request.Password, HardcodedPassword, StringComparison.Ordinal))
            {
                return true;
            }
            return false;
        }
    }
}