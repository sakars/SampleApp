using Microsoft.AspNetCore.Identity;

namespace SampleApp.DbItems
{
    public class SampleUser : IdentityUser<Guid>
    {
        public string DisplayName { get; set; }
    }
}
