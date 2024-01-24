using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ResetService.Models.Entity
{
    public class AppUser : IdentityUser<Guid>
    {
        [Key]
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string TcNo { get; set; }
        public string Email { get; set; }
        

        public AppUser()
        {
           // Id = Guid.NewGuid();
        }

    }
}
