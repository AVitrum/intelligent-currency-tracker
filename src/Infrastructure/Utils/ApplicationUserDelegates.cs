using Infrastructure.Identity;

namespace Infrastructure.Utils;

public delegate Task<ApplicationUser?> UserLookupDelegate(string identifier);