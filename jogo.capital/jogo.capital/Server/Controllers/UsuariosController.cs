using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using jogo.capital.Server.Context;
using jogo.capital.Server.Services;
using jogo.capital.Shared.Models;
using jogo.capital.Shared.Tables;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace jogo.capital.Server.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : Controller
    {

        private readonly IConfiguration cfg;
        private readonly UserManager<IdentityUser> um;
        private readonly SignInManager<IdentityUser> sm;
        private readonly dbCtx db;

        public UsuariosController(
            IConfiguration _cfg,
            UserManager<IdentityUser> _um,
            SignInManager<IdentityUser> _sm,
            dbCtx _db)
        {
            cfg = _cfg;
            um = _um;
            sm = _sm;
            db = _db;
        }

        [HttpPost("cadastrarseCelular")]
        public async Task<IActionResult> cadCel(usuariosCadastrarseCelularModel usr)
        {
            try
            { 
                var ret = db.Tbl01s.Where(x => x.Documento == usr.celular && x.Ativo == true).OrderByDescending(r=>r.Id).FirstOrDefault();
                if (ret != null) throw new Exception("Celular Já Cadastrado !!!");

                var user = new Tbl01();
                user.ContraSenha = usr.confirmarCodigo;
                user.Documento = usr.celular;
                user.Senha = Utils.EncryptPlainTextToCipherText(usr.senha);
                user.Tipo = "Celular";
                user.ContraSenha = new Random().Next(1,9999).ToString("0000");
                user.Validade = DateTime.Now.AddMinutes(5);
                user.Saldo = 0;
                user.Ativo = false; //Cadastra como falso para posterior checagem da Contra Senha
                db.Tbl01s.Add(user);
                db.SaveChanges();

                try
                {
                    envioDeSms(new enviarSmsModel()
                    {
                        Destinatario = usr.celular,
                        Texto = "Código para confirmar seu Cadastro no JB é " + user.ContraSenha
                    });
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                return Ok(new resultModel { Sucesso = true });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("validarCadastrarseCelular")]
        public async Task<IActionResult> valCadCel(usuariosCadastrarseCelularModel usr)
        {
            try
            {
                var ret = new Tbl01();
                ret = db.Tbl01s.Where(x => x.Documento == usr.celular).OrderByDescending(r => r.Id).FirstOrDefault();
                if (ret == null) throw new Exception("Celular Não Cadastrado !!!");
                if (!usr.confirmarCodigo.Contains("-"))
                {
                    usr.confirmarCodigo = usr.confirmarCodigo.Replace("-", "");
                    if (ret.ContraSenha != usr.confirmarCodigo) throw new Exception("Código Inválido !!!");
                    ret.Senha = Utils.EncryptPlainTextToCipherText(usr.confirmarCodigo);
                }
                else
                {
                    if (ret.Validade < DateTime.Now) throw new Exception("Cadastramento Já Expirado. Refaça-o !!!");
                    if (ret.ContraSenha != usr.confirmarCodigo) throw new Exception("Código Inválido !!!");
                }

                ret.Ativo = true;
                db.Tbl01s.Update(ret);
                db.SaveChanges();

                return Ok(new resultModel { Sucesso = true });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("esqueciCelular")]
        public async Task<IActionResult> esqueciCel(usuariosCadastrarseCelularModel usr)
        {
            try
            {
                var ret = new Tbl01();
                ret = db.Tbl01s.Where(x => x.Documento == usr.celular && x.Ativo == true).OrderByDescending(r => r.Id).FirstOrDefault();
                if (ret == null) throw new Exception("Celular Não Cadastrado !!!");

                if (ret.ContraSenha == "12121212")
                {
                    try
                    {
                        ret.ContraSenha = new Random().Next(1, 9999).ToString("0000");
                        ret.Validade = DateTime.Now.AddMinutes(5);
                        db.Tbl01s.Update(ret);
                        db.SaveChanges();
                        envioDeSms(new enviarSmsModel()
                        {
                            Destinatario = usr.celular,
                            Texto = "Seu código para alterar a senha no JB é " + ret.ContraSenha 
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    if (ret.ContraSenha != usr.confirmarCodigo) throw new Exception("Código Inválido !!!");
                    ret.Senha = Utils.EncryptPlainTextToCipherText(usr.senha);
                    ret.Ativo = true;
                    db.Tbl01s.Update(ret);
                    db.SaveChanges();
                }

                return Ok(new resultModel { Sucesso = true });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("cadastrarseEmail")]
        public async Task<IActionResult> cadMail(usuariosCadastrarseEmailModel usr)
        {
            try
            {
                var ret = db.Tbl01s.Where(x => x.Documento == usr.email && x.Ativo == true).OrderByDescending(r => r.Id).FirstOrDefault();
                if (ret != null) throw new Exception("E-mail Já Cadastrado !!!");

                var user = new Tbl01();
                user.ContraSenha = usr.confirmarCodigo;
                user.Documento = usr.email;
                user.Senha = Utils.EncryptPlainTextToCipherText(usr.senha);
                user.Tipo = "E-mail";
                user.ContraSenha = new Random().Next(1, 9999).ToString("0000");
                user.Validade = DateTime.Now.AddMinutes(5);
                user.Saldo = 0;
                user.Ativo = false; //Cadastra como falso para posterior checagem da Contra Senha
                db.Tbl01s.Add(user);
                db.SaveChanges();

                try
                {
                    envioDeEmail(new enviarEmailModel()
                    {
                        Destinatario = usr.email,
						Texto = "Código para confirmar seu Cadastro no JB é " + user.ContraSenha
					});
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }

                return Ok(new resultModel { Sucesso = true });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("validarCadastrarseEmail")]
        public async Task<IActionResult> valCadMail(usuariosCadastrarseEmailModel usr)
        {
            try
            {
                var ret = db.Tbl01s.Where(x => x.Documento == usr.email).OrderByDescending(r => r.Id).FirstOrDefault();
                if (ret == null) throw new Exception("E-mail Não Cadastrado !!!");

                if (!usr.confirmarCodigo.Contains("-"))
                {
                    usr.confirmarCodigo = usr.confirmarCodigo.Replace("-", "");
                    if (ret.ContraSenha != usr.confirmarCodigo) throw new Exception("Código Inválido !!!");
                    ret.Senha = Utils.EncryptPlainTextToCipherText(usr.confirmarCodigo);
                }
                else
                {
                    if (ret.Validade < DateTime.Now) throw new Exception("Cadastramento Já Expirado. Refaça-o !!!");
                    if (ret.ContraSenha != usr.confirmarCodigo) throw new Exception("Código Inválido !!!");
                }

                //if (ret.Validade < DateTime.Now && usr.confirmarCodigo.Contains("-")) throw new Exception("Cadastramento Já Expirado. Refaça-o !!!");
                //if (ret.ContraSenha != usr.confirmarCodigo) throw new Exception("Código Inválido !!!");

                ret.Ativo = true;
                db.Tbl01s.Update(ret);
                db.SaveChanges();

                return Ok(new resultModel { Sucesso = true });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("esqueciEmail")]
        public async Task<IActionResult> esqueciMail(usuariosCadastrarseEmailModel usr)
        {
            try
            {
                var ret = new Tbl01();
                ret = db.Tbl01s.Where(x => x.Documento == usr.email && x.Ativo == true).OrderByDescending(r => r.Id).FirstOrDefault();
                if (ret == null) throw new Exception("E-mail Não Cadastrado !!!");

                if (ret.ContraSenha == "12121212")
                {

                    try
                    {
                        ret.ContraSenha = new Random().Next(1, 9999).ToString("0000");
                        ret.Validade = DateTime.Now.AddMinutes(5);
                        db.Tbl01s.Update(ret);
                        db.SaveChanges();
                        envioDeEmail(new enviarEmailModel()
                        {
                            Destinatario = usr.email,
							Texto = "Seu código para alterar a senha no JB é " + ret.ContraSenha
						});
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }
                else
                {
                    if (ret.ContraSenha != usr.confirmarCodigo) throw new Exception("Código Inválido !!!");
                    ret.Senha = Utils.EncryptPlainTextToCipherText(usr.senha);
                    ret.Ativo = true;
                    db.Tbl01s.Update(ret);
                    db.SaveChanges();
                }

                return Ok(new resultModel { Sucesso = true });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("validarAcesso")]
        public async Task<IActionResult> validarLogin(acessarModel usr)
        {

            try
            {

                var ret = db.Tbl01s.Where(x => x.Documento == usr.Documento && x.Ativo == true).FirstOrDefault();

                if (ret == null) throw new Exception("Acesso Não Encontrado !!!");

                if (Utils.DecryptCipherTextToPlainText(ret.Senha) != usr.Senha) throw new Exception("Credencial Inválida !!!");

                var claims = new[] { new Claim(ClaimTypes.Name, usr.Documento) };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["JwtSecurityKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiry = DateTime.Now.AddDays(Convert.ToInt32(cfg["JwtExpiryInDays"]));
                var token = new JwtSecurityToken(
                       cfg["JwtIssuer"],
                       cfg["JwtAudience"],
                       claims,
                       expires: expiry,
                       signingCredentials: creds
                    );
                return Ok(new resultModel { Sucesso = true, Token = new JwtSecurityTokenHandler().WriteToken(token) });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("listarCliente")]
        public async Task<IActionResult> listagemDeClientes(listarClienteModel dados)
        {
            try
            {

                var novo = new List<Tbl01>();
                if (dados.Codigo == 0)
                {
                    novo = db.Tbl01s.Where(x => x.Ativo == true).ToList();
                }
                else
                {
                    novo = db.Tbl01s.Where(x => x.Id == dados.Codigo).ToList();
                }

                return Ok(new resultModel { Sucesso = true, Token = JsonSerializer.Serialize(novo) });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("enviarSMS")]
        public async Task<IActionResult> envioDeSms(enviarSmsModel dados)
        {
            try
            {

                dados.Destinatario = dados.Destinatario.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
				var key = cfg.GetSection("SmsSettings").Get<SmsSettings>().Key;
                var url = cfg.GetSection("SmsSettings").Get<SmsSettings>().Url;
                var typ = cfg.GetSection("SmsSettings").Get<SmsSettings>().Type;
                var usr = cfg.GetSection("SmsSettings").Get<SmsSettings>().Usr;
                var str = url + "?key=" + key + "&type=" + typ.ToString() + "&number=" + dados.Destinatario + "&msg=" + dados.Texto + "&refer=" + usr; 
                var htp = new HttpClient();

                var ret = await htp.GetAsync(str);

                return Ok(new resultModel { Sucesso = true });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("enviarEmail")]
        public async Task<IActionResult> envioDeEmail(enviarEmailModel dados)
        {
            try
            {

                var message = new MailMessage();
                message.From = new MailAddress(cfg.GetSection("MailSettings").Get<MailSettings>().Mail, cfg.GetSection("MailSettings").Get<MailSettings>().DisplayName);
                message.To.Add(new MailAddress(dados.Destinatario, dados.Destinatario));
                message.Subject = "Confirmação";
                message.Body = dados.Texto;

                var client = new SmtpClient();
                client.Host = cfg.GetSection("MailSettings").Get<MailSettings>().Host;
                client.Port = cfg.GetSection("MailSettings").Get<MailSettings>().Port;
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(cfg.GetSection("MailSettings").Get<MailSettings>().Mail, cfg.GetSection("MailSettings").Get<MailSettings>().Password);
                client.Send(message);

                return Ok(new resultModel { Sucesso = true });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("listarFinanceiros")]
        public async Task<IActionResult> listagemFinanceira()
        {
            try
            {

                var novo = db.Tbl06s.Where(x => x.Ativo == false).ToList();

                return Ok(new resultModel { Sucesso = true, Token = JsonSerializer.Serialize(novo) });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("fazerDeposito")]
        public async Task<IActionResult> depositar(enviarDepositoModel dados)
        {
            try
            {

                var novo = new Tbl06();
                novo.Ativo = false;
                novo.DataHora = DateTime.Now;
                novo.Identificador = Guid.NewGuid().ToString().Replace("-", "");
                novo.Pessoa = dados.Pessoa;
                novo.Tipo = dados.Tipo;
                novo.Valor = dados.Valor;
                novo.Pix = dados.Pix;
                novo.Banco = dados.Banco;
                novo.Agencia = dados.Agencia;
                novo.Conta = dados.Conta;
                db.Tbl06s.Add(novo);
                db.SaveChanges();

                return Ok(new resultModel { Sucesso = true, Token = novo.Identificador });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("confirmarDeposito")]
        public async Task<IActionResult> confirmacaoDeDeposito(confirmarDepositoModel dados)
        {
            try
            {

                var novo = db.Tbl06s.Where(x => x.Identificador == dados.Identificador && x.Ativo == false).OrderByDescending(x => x.Id).FirstOrDefault();

                if (novo == null) throw new Exception("Identificador Não Encontrado !");

                var cliente = db.Tbl01s.Where(x => x.Id == novo.Pessoa && x.Ativo == true).FirstOrDefault();

                if (cliente == null) throw new Exception("Cliente Não Encontrado !");

                novo.Ativo = true;
                novo.DataHora = dados.DataHoraPagamento;
                db.Tbl06s.Update(novo);

                cliente.Saldo += novo.Valor;
                db.Tbl01s.Update(cliente);

                db.SaveChanges();

                return Ok(new resultModel { Sucesso = true });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("fazerSaque")]
        public async Task<IActionResult> sacar(enviarDepositoModel dados)
        {
            try
            {

                var novo = new Tbl06();
                novo.Ativo = false;
                novo.DataHora = DateTime.Now;
                novo.Identificador = Guid.NewGuid().ToString().Replace("-", "");
                novo.Pessoa = dados.Pessoa;
                novo.Tipo = dados.Tipo;
                novo.Valor = dados.Valor;
                novo.Pix = dados.Pix;
                novo.Banco = dados.Banco;
                novo.Agencia = dados.Agencia;
                novo.Conta = dados.Conta;
                db.Tbl06s.Add(novo);
                db.SaveChanges();

                return Ok(new resultModel { Sucesso = true, Token = novo.Identificador });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

        [HttpPost("confirmarSaque")]
        public async Task<IActionResult> confirmacaoDeSaque(confirmarDepositoModel dados)
        {
            try
            {

                var novo = db.Tbl06s.Where(x => x.Identificador == dados.Identificador && x.Ativo == false).OrderByDescending(x => x.Id).FirstOrDefault();

                if (novo == null) throw new Exception("Identificador Não Encontrado !");

                var cliente = db.Tbl01s.Where(x => x.Id == novo.Pessoa && x.Ativo == true).FirstOrDefault();

                if (cliente == null) throw new Exception("Cliente Não Encontrado !");

                novo.Ativo = true;
                novo.DataHora = dados.DataHoraPagamento;
                db.Tbl06s.Update(novo);

                cliente.Saldo -= novo.Valor;
                db.Tbl01s.Update(cliente);

                db.SaveChanges();

                return Ok(new resultModel { Sucesso = true });

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                var erros = new[] { ex.Message };
                return Ok(new resultModel { Erros = erros, Sucesso = false });
            }
        }

    }

    public class Utils
    {
        private const string SecurityKey = "$enh4_Vem_Kaki_Komplecsa";
        public static string EncryptPlainTextToCipherText(string PlainText)
        {
            byte[] toEncryptedArray = UTF8Encoding.UTF8.GetBytes(PlainText);
            MD5CryptoServiceProvider objMD5CryptoService = new MD5CryptoServiceProvider();
            byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
            objMD5CryptoService.Clear();
            var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
            objTripleDESCryptoService.Key = securityKeyArray;
            objTripleDESCryptoService.Mode = CipherMode.ECB;
            objTripleDESCryptoService.Padding = PaddingMode.PKCS7;
            var objCrytpoTransform = objTripleDESCryptoService.CreateEncryptor();
            byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptedArray, 0, toEncryptedArray.Length);
            objTripleDESCryptoService.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        public static string DecryptCipherTextToPlainText(string CipherText)
        {
            byte[] toEncryptArray = Convert.FromBase64String(CipherText);
            MD5CryptoServiceProvider objMD5CryptoService = new MD5CryptoServiceProvider();
            byte[] securityKeyArray = objMD5CryptoService.ComputeHash(UTF8Encoding.UTF8.GetBytes(SecurityKey));
            objMD5CryptoService.Clear();
            var objTripleDESCryptoService = new TripleDESCryptoServiceProvider();
            objTripleDESCryptoService.Key = securityKeyArray;
            objTripleDESCryptoService.Mode = CipherMode.ECB;
            objTripleDESCryptoService.Padding = PaddingMode.PKCS7;
            var objCrytpoTransform = objTripleDESCryptoService.CreateDecryptor();
            byte[] resultArray = objCrytpoTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            objTripleDESCryptoService.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

    }

    public class SmsSettings
    {
        public string Key { get; set; }
        public string Url { get; set; }
        public int Type { get; set; }
        public string Usr { get; set; }
    }

    public class MailSettings
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }

}

