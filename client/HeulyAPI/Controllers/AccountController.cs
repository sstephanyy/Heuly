using HeulyAPI.Dtos.Account;
using HeulyAPI.Interfaces;
using HeulyAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HeulyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager, IConfiguration configuration , IEmailSender emailSender)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Password confirmation validation
                if (registerDto.Password != registerDto.ConfirmPassword)
                {
                    return BadRequest(new { message = "As senhas não são iguais." });
                }

                var user = new AppUser
                {
                    Email = registerDto.Email,
                    UserName = registerDto.Name
                };

                var createdUser = await _userManager.CreateAsync(user, registerDto.Password);

                if (createdUser.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "User");

                    if (roleResult.Succeeded)
                    {
                        return Ok(
                            new AuthResponseDto
                            {
                                IsSuccess = true,
                                Token = _tokenService.CreateToken(user),
                                Message = "Conta criada com sucesso!"
                            }
                        );
                    }
                    else
                    {
                        return StatusCode(500, roleResult.Errors);
                    }
                }
                else
                {
                    return BadRequest(createdUser.Errors);
                }

            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized("Usuário não encontrado!");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
            {
                return Unauthorized("Senha inválida.");

            }

            return Ok(
                new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Login realizado!"
                }
           );
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

            if (user == null)
            {
                return BadRequest(new { message = "Se um usuário com esse e-mail existir, um e-mail de redefinição será enviado." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "Account", new { token, email = user.Email }, Request.Scheme);

            var subject = "Redefinição de Senha - Heuly";
            var body = $@"
            <p>Olá,</p>
            <p>Recebemos uma solicitação para redefinir a senha da sua conta. Se você não solicitou a redefinição de senha, por favor, ignore este e-mail. Caso contrário, clique no link abaixo para redefinir sua senha:</p>
            <p><a href='{resetLink}'>Redefinir Senha</a></p>
            <p>Se o link acima não funcionar, copie e cole a URL abaixo no seu navegador:</p>
            <p>{resetLink}</p>
            <p>Atenciosamente,</p>
            <p>Heuly</p>";

            await _emailSender.SendEmailAsync(user.Email, subject, body);

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Token = token,
                Message = "Um link para redefinir a senha foi enviado para seu e-mail."
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);

            if (user == null)
            {
                return BadRequest(new { message = "Usuário não encontrado." });
            }

            if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmPassword)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "As senhas não coincidem."
                });
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);


            if (!result.Succeeded)
            {
                return BadRequest(new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Erro ao redefinir a senha.",
                    Token = resetPasswordDto.Token
                });
            }

            return Ok(new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Senha redefinida com sucesso."
            });
        }
    }

}

