using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

public delegate ApplicationUser UserFactoryDelegate();
public delegate Task<IdentityResult> PostCreationDelegate(ApplicationUser user);
public delegate Task<ApplicationUser?> UserLookupDelegate(string identifier);