using System;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Blazored.LocalStorage;
using jogo.capital.Client.Providers;
using jogo.capital.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace jogo.capital.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient httpClient;
        private readonly ILocalStorageService localStorage;
        private readonly AuthenticationStateProvider authenticationStateProvider;

        public AuthService(HttpClient _httpClient, ILocalStorageService _localStorage, AuthenticationStateProvider _authenticationStateProvider)
        {
            httpClient = _httpClient;
            localStorage = _localStorage;
            authenticationStateProvider = _authenticationStateProvider;
        }

        public async Task<resultModel> Login(acessarModel loginModel)
        {
            var loginJson = JsonSerializer.Serialize(loginModel);
            var response = await httpClient.PostAsync("api/Usuarios/validarAcesso", new StringContent(loginJson, Encoding.UTF8, "application/json"));
            var loginResult = JsonSerializer.Deserialize<resultModel>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (!response.IsSuccessStatusCode) return loginResult;
            await localStorage.SetItemAsync("authToken", loginResult.Token);
            ((ApiAuthenticationStateProvider)authenticationStateProvider).MarkUserAuthenticated(loginModel.Documento);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", loginResult.Token);
            return loginResult;
        }

        public async Task Logout()
        {
            await localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)authenticationStateProvider).MarkUserAsLoggedOut();
            httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<resultModel> RegisterCelular(usuariosCadastrarseCelularModel regModel)
        {
            var loginJson = JsonSerializer.Serialize(regModel);
            var result = await httpClient.PostAsync("api/Usuarios/cadastrarseCelular", new StringContent(loginJson, Encoding.UTF8, "application/json"));
            var loginResult = JsonSerializer.Deserialize<resultModel>(await result.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return loginResult;
        }

        public async Task<resultModel> ValidarCelular(usuariosCadastrarseCelularModel regModel)
        {
            var loginJson = JsonSerializer.Serialize(regModel);
            var result = await httpClient.PostAsync("api/Usuarios/validarCadastrarseCelular", new StringContent(loginJson, Encoding.UTF8, "application/json"));
            var loginResult = JsonSerializer.Deserialize<resultModel>(await result.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return loginResult;
        }

        public async Task<resultModel> RegisterEmail(usuariosCadastrarseEmailModel regModel)
        {
            var loginJson = JsonSerializer.Serialize(regModel);
            var result = await httpClient.PostAsync("api/Usuarios/cadastrarseEmail", new StringContent(loginJson, Encoding.UTF8, "application/json"));
            var loginResult = JsonSerializer.Deserialize<resultModel>(await result.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return loginResult;
        }

        public async Task<resultModel> ValidarEmail(usuariosCadastrarseEmailModel regModel)
        {
            var loginJson = JsonSerializer.Serialize(regModel);
            var result = await httpClient.PostAsync("api/Usuarios/validarCadastrarseEmail", new StringContent(loginJson, Encoding.UTF8, "application/json"));
            var loginResult = JsonSerializer.Deserialize<resultModel>(await result.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return loginResult;
        }

    }
}

