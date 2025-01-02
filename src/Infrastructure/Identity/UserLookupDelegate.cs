namespace Infrastructure.Identity;

public delegate Task<ApplicationUser?> UserLookupDelegate(string identifier);