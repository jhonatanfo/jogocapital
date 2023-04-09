using System;
using jogo.capital.Shared.Models;

namespace jogo.capital.Client.Services
{
	public interface IAuthService
	{
		Task<resultModel> Login(acessarModel loginModel);
		Task Logout();
		Task<resultModel> RegisterCelular(usuariosCadastrarseCelularModel regModel);
        Task<resultModel> ValidarCelular(usuariosCadastrarseCelularModel regModel);
        Task<resultModel> RegisterEmail(usuariosCadastrarseEmailModel regModel);
        Task<resultModel> ValidarEmail(usuariosCadastrarseEmailModel regModel);
    }
}

