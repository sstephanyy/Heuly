using System.ComponentModel.DataAnnotations;

namespace HeulyAPI.Dtos.Account
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O campo Email deve conter um endereço de email válido.")]
        public string Email { get; set; }
    }
}
