namespace MyShopAPI.Services
{
    /// <summary>
    /// Service for extracting the current user's ID from JWT claims.
    /// Used by AppDbContext for global query filters.
    /// </summary>
    public interface IUserContextService
    {
        /// <summary>
        /// Gets the current authenticated user's ID from the JWT claims.
        /// Returns null if not authenticated.
        /// </summary>
        string? GetUserId();

        /// <summary>
        /// Gets the current authenticated user's role.
        /// </summary>
        string? GetUserRole();
    }
}
