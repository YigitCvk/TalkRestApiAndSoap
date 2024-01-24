using System.ComponentModel.DataAnnotations;

namespace ResetService.Models.ViewModels
{
    public class UserRegisterVM
    {
        [Required(ErrorMessage = "Lütfen Adınızı Girin")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Lütfen soyadınızı Girin")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Lütfen Kullanıcı Adını Girin")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Lütfen mail Girin")]
        public string Mail { get; set; }

        [Required(ErrorMessage = "Lütfen şifreyi Girin")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Lütfen mail Girin")]
        public string DefaultCity { get; set; }

        [Required(ErrorMessage = "Lütfen şifreyi tekrar Girin")]
        [Compare("Password", ErrorMessage = "Şifreler uyumlu değil!")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Lütfen TC No Girin")]
        public string TcNo { get; set; }
    }
}
