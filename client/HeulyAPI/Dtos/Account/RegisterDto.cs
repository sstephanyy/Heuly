using System.ComponentModel.DataAnnotations;

namespace HeulyAPI.Dtos.Account
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "O campo Nome é obrigatório.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "O campo Email é obrigatório.")]
        [EmailAddress(ErrorMessage = "O campo Email deve conter um endereço de email válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "O campo Senha é obrigatório.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "O campo Confirmar Senha é obrigatório.")]
        [Compare("Password", ErrorMessage = "As senhas não correspondem.")]
        public string ConfirmPassword { get; set; }


    }
}
