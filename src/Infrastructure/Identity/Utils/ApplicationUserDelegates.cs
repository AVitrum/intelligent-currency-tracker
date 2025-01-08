namespace Infrastructure.Identity.Utils;

public delegate Task<ApplicationUser?> UserLookupDelegate(string identifier);