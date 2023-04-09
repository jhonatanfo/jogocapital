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
    public class ApostasController : Controller
    {

        private readonly IConfiguration cfg;
        private readonly UserManager<IdentityUser> um;
        private readonly SignInManager<IdentityUser> sm;
        private readonly dbCtx db;

        public ApostasController(
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

        [HttpPost("verAcumulado")]
        public async Task<IActionResult> visualizarAcumulado(verSaldosModel dados)
        {

            //Calcular a próxima data com base no Tipo
            DateTime dh = DateTime.Now;
            int dias = 0;
            verSaldosModel ret = new verSaldosModel();

            try
            {
                //Tipos quando "B"olão
                if (dados.Tipo.ToUpper().StartsWith("B"))
                {
                    dados.Tipo = "Bolão";
                    dh = Convert.ToDateTime(dados.DataHora.ToString("yyyy-MM-dd") + " 18:00:00");
                    //Calcular quando Bolão a próxima quarta 
                    if (DateTime.Now.DayOfWeek > DayOfWeek.Wednesday)
                    {
                        dias = (7 - ((int)DateTime.Now.DayOfWeek)) + 3;
                    }
                    else
                    {
                        dias = 3 - ((int)DateTime.Now.DayOfWeek);
                    }
                    dh = dh.AddDays(dias);
                }
                else
                {
                    if (dados.Tipo.ToUpper().StartsWith("J"))
                    {
                        dados.Tipo = "Jogo do Bicho";
                        dh = Convert.ToDateTime(dados.DataHora.ToString("yyyy-MM-dd") + " 16:00:00");
                        //Jogo do Bicho calcular a próxima data de srteio
                        if (DateTime.Now.DayOfWeek > DayOfWeek.Sunday)
                        {
                            dias = (7 - ((int)DateTime.Now.DayOfWeek)) + 0;
                        }
                        else
                        {
                            dias = 0 - ((int)DateTime.Now.DayOfWeek);
                        }
                        dh = dh.AddDays(dias);
                    }
                    else
                    {
                        throw new Exception("Tipo Inválido");
                    }
                }

                var busca = db.Tbl02s.Where(x => x.Tipo == dados.Tipo && x.Ativo == true && x.DataHora >= dh && x.Ativo == true).OrderBy(r => r.Id).FirstOrDefault();

                if (busca == null)
                {
                    busca = new Tbl02();
                    busca.Ativo = true;
                    busca.DataHora = dh;
                    busca.Tipo = (dados.Tipo.ToUpper().StartsWith("B") ? "Bolão" : "Jogo do Bicho");
                    busca.Valor = 0;
                    db.Tbl02s.Add(busca);
                    db.SaveChanges();
                }

                ret.DataHora = busca.DataHora;
                ret.Tipo = busca.Tipo;
                ret.Valor = busca.Valor;

                return Ok(ret);

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                ret.DataHora = dh;
                ret.Tipo = "Erro (" + dados.Tipo + "): " + ex.Message;
                ret.Valor = -1;
                return Ok(ret);
            }

        }

        [HttpPost("lancarSorteio")]
        public async Task<IActionResult> lancamentoDeSorteios(lancarSorteioModel dados)
        {

            //Calcular a próxima data com base no Tipo
            verSaldosModel ret = new verSaldosModel();
            DateTime dh = DateTime.Now;
            var tipoSorteio = "";
            int qtds = 0;
            DateTime dataMinima = DateTime.Now;

            //Buscar Tabela de Preços Atual
            var tabPre = db.Tbl07s.Where(x => x.Ativo == true).FirstOrDefault();

            try
            {

                if (tabPre == null) throw new Exception("Tabela de Preços Atual Não Encontrada !");

                if (dados.Tipo.ToUpper().StartsWith("B"))
                {
                    //Voltar Data Mínima como último Sábado às 20hrs 
                    dataMinima = DateTime.Now.AddDays((((int)DateTime.Now.DayOfWeek) + 1) * (-1));
                    dataMinima = Convert.ToDateTime(dataMinima.ToString("yyyy-MM-dd") + " 20:00:00");
                    dados.Tipo = "Bolão";
                    qtds = 10;
                    if (dados.DataHora.DayOfWeek < DayOfWeek.Wednesday || dados.DataHora.DayOfWeek > DayOfWeek.Saturday) throw new Exception("Dia de Semana Inválido !");
                    if (dados.DataHora.Hour >= 21 || dados.DataHora.Hour < 11)
                    {
                        tipoSorteio = "PTM";
                    }
                    else
                    {
                        if (dados.DataHora.Hour >= 11 && dados.DataHora.Hour < 14)
                        {
                            tipoSorteio = "PT";
                        }
                        else
                        {
                            if (dados.DataHora.Hour >= 14 && dados.DataHora.Hour < 16)
                            {
                                tipoSorteio = "PTV";
                            }
                            else
                            {
                                if (dados.DataHora.Hour >= 16 && dados.DataHora.Hour < 18)
                                {
                                    tipoSorteio = "PTN";
                                }
                                else
                                {
                                    if (dados.DataHora.Hour >= 18 && dados.DataHora.Hour < 21)
                                    {
                                        tipoSorteio = "COR";
                                    }
                                    else
                                    {
                                        throw new Exception("Horário Inválido !");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (dados.Tipo.ToUpper().StartsWith("J"))
                    {
                        //Voltar Data Mínima como último Domingo às 16hrs 
                        dataMinima = DateTime.Now.AddDays((((int)DateTime.Now.DayOfWeek)) * (-1));
                        dataMinima = Convert.ToDateTime(dataMinima.ToString("yyyy-MM-dd") + " 16:00:00");
                        dados.Tipo = "Jogo do Bicho";
                        qtds = 7;
                        if (dados.DataHora.DayOfWeek >= DayOfWeek.Wednesday && dados.DataHora.Hour == 19)
                        {
                            tipoSorteio = "QUA|19h";
                        }
                        else
                        {
                            if (dados.DataHora.DayOfWeek >= DayOfWeek.Thursday && dados.DataHora.Hour == 14)
                            {
                                tipoSorteio = "QUI|14h";
                            }
                            else
                            {
                                if (dados.DataHora.DayOfWeek >= DayOfWeek.Thursday && dados.DataHora.Hour == 18)
                                {
                                    tipoSorteio = "QUI|18h";
                                }
                                else
                                {
                                    if (dados.DataHora.DayOfWeek >= DayOfWeek.Friday && dados.DataHora.Hour == 14)
                                    {
                                        tipoSorteio = "SEX|14h";
                                    }
                                    else
                                    {
                                        if (dados.DataHora.DayOfWeek >= DayOfWeek.Friday && dados.DataHora.Hour == 18)
                                        {
                                            tipoSorteio = "SEX|18h";
                                        }
                                        else
                                        {
                                            if (dados.DataHora.DayOfWeek >= DayOfWeek.Saturday && dados.DataHora.Hour == 14)
                                            {
                                                tipoSorteio = "SAB|14h";
                                            }
                                            else
                                            {
                                                if (dados.DataHora.DayOfWeek >= DayOfWeek.Saturday && dados.DataHora.Hour == 19)
                                                {
                                                    tipoSorteio = "SAB|19h";
                                                }
                                                else
                                                {
                                                    throw new Exception("Data/Hora Inválida !");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Tipo Inválido");
                    }
                }

                var busca = db.Tbl03s.Where(x => x.Tipo == dados.Tipo && x.DataHora == dh && x.Ativo == true).OrderBy(r => r.Id).FirstOrDefault();

                if (busca != null)
                {
                    throw new Exception("Sorteio Já Cadastrado Para a Data " + dados.DataHora.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    for (int i = 1; i <= qtds; i++)
                    {
                        //Lançar cada número sorteado
                        int numero =
                            (i == 1 ? dados.Numero01 :
                            (i == 2 ? dados.Numero02 :
                            (i == 3 ? dados.Numero03 :
                            (i == 4 ? dados.Numero04 :
                            (i == 5 ? dados.Numero05 :
                            (i == 6 ? dados.Numero06 :
                            (i == 7 ? dados.Numero07 :
                            (i == 8 ? dados.Numero08 :
                            (i == 9 ? dados.Numero09 :
                            (i == 10 ? dados.Numero10 : 0))))))))));
                        //Controles de JB
                        if (dados.Tipo.ToUpper().StartsWith("J"))
                        {
                            //Caso venha algum número erradamente quando 5 ou maior, deve-se zerá-lo
                            if (i > 5) numero = 0;
                            //Calcular número quando 6
                            if (i == 6)
                            {
                                //O 6o prêmio e a soma dos números sorteados do 1o ao 5o prêmio e o resultado e até a centena (os 3 últimos dígitos)
                                var num6 = dados.Numero01 + dados.Numero02 + dados.Numero03 + dados.Numero04 + dados.Numero05;
                                //Pegar os últimos 3 números
                                numero = Convert.ToInt32(num6.ToString().Substring(num6.ToString().Length - 3));
                            }
                            else if (i == 7) //Calcular quando 7
                            {
                                //O 7o prêmio e a multiplicação do 1o com 2o prêmio
                                var num7 = dados.Numero01 * dados.Numero02;
                                //e o resultado tira os 3 números do lado direito
                                numero = Convert.ToInt32(num7.ToString().Substring(0, num7.ToString().Length - 3));
                                //e os 2 números do lado esquerdo
                                numero = Convert.ToInt32(numero.ToString().Substring(2));
                            }
                        }

                        //Lançar Número em formato "Milhar" (Todas as 4 posições)
                        busca = new Tbl03();
                        busca.Tipo = dados.Tipo;
                        busca.DataHora = dados.DataHora;
                        busca.Situacao = "Lançado";
                        busca.Sorteio = tipoSorteio;
                        busca.Numero = numero;
                        busca.Sequencia = i;
                        busca.Ativo = true;
                        db.Tbl03s.Add(busca);

                        //Criar Números de Centena e de Dezena
                        var numeroC = Convert.ToInt32(numero.ToString("0000").Substring(1));
                        var numeroD = Convert.ToInt32(numero.ToString("0000").Substring(2));

                        if (numero > numeroC)
                        {
                            //Lançar Número em formato "Centena" (As 3 últimas posições)
                            busca = new Tbl03();
                            busca.Tipo = dados.Tipo;
                            busca.DataHora = dados.DataHora;
                            busca.Situacao = "Lançado";
                            busca.Sorteio = tipoSorteio;
                            busca.Numero = numero;
                            busca.Sequencia = i;
                            busca.Ativo = true;
                            db.Tbl03s.Add(busca);
                        }

                        if (numero > numeroD)
                        {
                            //Lançar Número em formato "Dezena" (As 2 últimas posições)
                            busca = new Tbl03();
                            busca.Tipo = dados.Tipo;
                            busca.DataHora = dados.DataHora;
                            busca.Situacao = "Lançado";
                            busca.Sorteio = tipoSorteio;
                            busca.Numero = numero;
                            busca.Sequencia = i;
                            busca.Ativo = true;
                            db.Tbl03s.Add(busca);
                        }

                        //Marcar Apostas "Sorteadas"
                        if (dados.Tipo.ToUpper().StartsWith("B")) //Quando Bolão é sempre um número apostado para um número sorteado
                        {
                            var apostas = db.Tbl08s.Where(x => x.TipoAposta == "Bolão" && x.NumeroTransformado == numero && x.Ativo == true).ToList();
                            foreach (var item in apostas)
                            {
                                item.Sorteado = true;
                                db.Tbl08s.Update(item);
                            }
                        }
                        else
                        {

                            //Fazer as checagens
                            //Grupo        => 4 dezenas    => 1o apenas       => R$ 18
                            //Dezena       => 00 a 99      => Dezena final 1o => R$ 60,00
                            //Centena      => 000 a 999    => 3 últimos do 1o => R$ 600,00
                            //Milhar       => 0000 a 9999  => 1o apenas       => R$ 4.000,00
                            //Duque Grupo  => Dois Animais => 1o ao 5o        => R$ 18,75
                            //Duque Dezena => 00 a 99 (2x) => 1o ao 5o        => R$ 300,00
                            //Terno Dezena => 00 a 99 (3x) => 1o ao 5o        => R$ 3.000,00
                            //Terno Grupo  => Três Animais => 1o ao 5o        => R$ 130,00
                            //Distribuições
                            //1o apenas    => Integral
                            //1o ao 5o     => 1/5 do escolhido
                            //1o ao 7o     => 1/7 do escolhido
                            //Variações
                            //Centena Inversão => Prêmio / inversões (variações)
                            //Milhar  Inversão => Prêmio / inversões (variações)


                            //Quando "JB" identificar de acordo com os "Tipos" (de 1 a 7)
                            if (i == 1) //Se 1o prêmio
                            {
                                var apostas1 = db.Tbl08s.Where(x => x.TipoAposta == "Jogo do Bicho" && x.NumeroTransformado == numero && x.Ativo == true &&
                                (x.TipoEvento == "1o Prêmio" || x.TipoEvento == "Cercado 1o ao 5o" || x.TipoEvento == "Cercado 1o ao 7o")).ToList();
                                foreach (var item in apostas1)
                                {
                                    item.Sorteado = true;
                                    db.Tbl08s.Update(item);
                                }
                                var apostas2 = db.Tbl08s.Where(x => x.TipoAposta == "Jogo do Bicho" && x.NumeroTransformado == numeroC && x.Ativo == true &&
                                (x.TipoEvento == "1o Prêmio" || x.TipoEvento == "Cercado 1o ao 5o" || x.TipoEvento == "Cercado 1o ao 7o")).ToList();
                                foreach (var item in apostas2)
                                {
                                    item.Sorteado = true;
                                    db.Tbl08s.Update(item);
                                }
                                var apostas3 = db.Tbl08s.Where(x => x.TipoAposta == "Jogo do Bicho" && x.NumeroTransformado == numeroD && x.Ativo == true &&
                                (x.TipoEvento == "1o Prêmio" || x.TipoEvento == "Cercado 1o ao 5o" || x.TipoEvento == "Cercado 1o ao 7o")).ToList();
                                foreach (var item in apostas3)
                                {
                                    item.Sorteado = true;
                                    db.Tbl08s.Update(item);
                                }
                            }
                            else if (i == 2) //Se 2o prêmio
                            {
                                var apostas = db.Tbl08s.Where(x => x.TipoAposta == "Jogo do Bicho" && x.NumeroTransformado == numero && x.Ativo == true &&
                                (x.TipoEvento == "Cercado 1o ao 5o" || x.TipoEvento == "Cercado 1o ao 7o")).ToList();
                                foreach (var item in apostas)
                                {
                                    item.Sorteado = true;
                                    db.Tbl08s.Update(item);
                                }
                            }
                            else if (i == 3) //Se 3o prêmio
                            {
                                var apostas = db.Tbl08s.Where(x => x.TipoAposta == "Jogo do Bicho" && x.NumeroTransformado == numero && x.Ativo == true &&
                                (x.TipoEvento == "Cercado 1o ao 5o" || x.TipoEvento == "Cercado 1o ao 7o")).ToList();
                                foreach (var item in apostas)
                                {
                                    item.Sorteado = true;
                                    db.Tbl08s.Update(item);
                                }
                            }
                            else if (i == 4) //Se 4o prêmio
                            {
                                var apostas = db.Tbl08s.Where(x => x.TipoAposta == "Jogo do Bicho" && x.NumeroTransformado == numero && x.Ativo == true &&
                                (x.TipoEvento == "Cercado 1o ao 5o" || x.TipoEvento == "Cercado 1o ao 7o")).ToList();
                                foreach (var item in apostas)
                                {
                                    item.Sorteado = true;
                                    db.Tbl08s.Update(item);
                                }
                            }
                            else if (i == 5) //Se 5o prêmio
                            {
                                var apostas = db.Tbl08s.Where(x => x.TipoAposta == "Jogo do Bicho" && x.NumeroTransformado == numero && x.Ativo == true &&
                                (x.TipoEvento == "Cercado 1o ao 5o" || x.TipoEvento == "Cercado 1o ao 7o")).ToList();
                                foreach (var item in apostas)
                                {
                                    item.Sorteado = true;
                                    db.Tbl08s.Update(item);
                                }
                            }
                            else if (i == 6) //Se 6o prêmio
                            {
                                var apostas = db.Tbl08s.Where(x => x.TipoAposta == "Jogo do Bicho" && x.NumeroTransformado == numero && x.Ativo == true &&
                                (x.TipoEvento == "Cercado 1o ao 7o")).ToList();
                                foreach (var item in apostas)
                                {
                                    item.Sorteado = true;
                                    db.Tbl08s.Update(item);
                                }
                            }
                            else if (i == 7) //Se 7o prêmio
                            {
                                var apostas = db.Tbl08s.Where(x => x.TipoAposta == "Jogo do Bicho" && x.NumeroTransformado == numero && x.Ativo == true &&
                                (x.TipoEvento == "Cercado 1o ao 7o")).ToList();
                                foreach (var item in apostas)
                                {
                                    item.Sorteado = true;
                                    db.Tbl08s.Update(item);
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                }
                return Ok(ret);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                ret.DataHora = dh;
                ret.Tipo = "Erro: " + ex.Message;
                ret.Valor = -1;
                return Ok(ret);
            }

        }

        [HttpPost("lancarAposta")]
        public async Task<IActionResult> lancamentoDeApostas(lancarApostaModel dados)
        {

            //Calcular a próxima data com base no Tipo
            verSaldosModel ret = new verSaldosModel();
            DateTime dh = DateTime.Now;

            try
            {

                if (dados.Tipo.ToUpper().StartsWith("B"))
                {
                    dados.Tipo = "Bolão";
                    if ((dados.DataHora.DayOfWeek == DayOfWeek.Saturday && dados.DataHora.Hour < 20) ||
                        (dados.DataHora.DayOfWeek == DayOfWeek.Wednesday && dados.DataHora.Hour > 18) ||
                        (dados.DataHora.DayOfWeek == DayOfWeek.Thursday) ||
                        (dados.DataHora.DayOfWeek == DayOfWeek.Friday))
                    {
                        throw new Exception("Data/Hora (" + dados.DataHora.ToString("yyyy-MM-dd HH:mm:ss") + ") fora do intervalo permitido");
                    }                    
                }
                else
                {
                    if (dados.Tipo.ToUpper().StartsWith("J"))
                    {
                        dados.Tipo = "Jogo do Bicho";
                        //Aceita qualquer dia e hora
                    }
                    else
                    {
                        throw new Exception("Tipo Inválido");
                    }
                }

                var busca = db.Tbl04s.Where(x => x.Pessoa == dados.Pessoa && x.Tipo == dados.Tipo && x.Ativo == true && x.DataHora == dh && x.Ativo == true).OrderBy(r => r.Id).FirstOrDefault();

                if (busca != null)
                {
                    throw new Exception("Aposta Já Cadastrada Para a Data " + dados.DataHora.ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    //Buscar Cliente para "Baixar Saldo"
                    var cli = db.Tbl01s.Where(x => x.Id == dados.Pessoa && x.Ativo == true).FirstOrDefault();
                    if (cli == null) throw new Exception("Cliente Não Encontrado !");
                    if (cli.Saldo < dados.ValorAposta) throw new Exception("Saldo Insuficiente !");
                    //Lançar Aposta Pai
                    busca = new Tbl04();
                    busca.Pessoa = dados.Pessoa;
                    busca.Tipo = dados.Tipo;
                    busca.DataHora = dados.DataHora;
                    busca.TipoAposta = dados.TipoAposta;
                    busca.TipoGrupo = dados.TipoGrupo;
                    busca.TipoEvento = dados.TipoEvento;
                    busca.ValorAposta = dados.ValorAposta;
                    busca.Lucro = 0;
                    busca.Quantidade = dados.Qtde;
                    busca.Ativo = true;
                    db.Tbl04s.Add(busca);
                    db.SaveChanges();
                    for (int i = 0; i < dados.Itens.Count; i++)
                    {
                        //Lançar Itens da Aposta (Filhos)
                        var item = new Tbl05();
                        item.Aposta = busca.Id;
                        item.DataHora = busca.DataHora;
                        item.Animal = dados.Itens[i].Animal;
                        item.Numero = dados.Itens[i].Numero;
                        item.Inversao = dados.Itens[i].Inversao;
                        item.Preco = dados.Itens[i].Preco;
                        item.Lucro = 0;
                        item.Desconto = 0;
                        item.Sorteado = false;
                        item.Ativo = true;
                        db.Tbl05s.Add(item);
                    }
                    //Baixar Saldo
                    cli.Saldo -= dados.ValorAposta;
                    db.Tbl01s.Update(cli);
                    //Adicionar ao Acumulado
                    var saldo = db.Tbl02s.Where(x => x.Tipo == dados.Tipo && x.Ativo == true && x.DataHora >= dados.DataHora && x.Ativo == true).OrderBy(r => r.Id).FirstOrDefault();
                    if (saldo == null)
                    {
                        saldo = new Tbl02();
                        saldo.Ativo = true;
                        saldo.DataHora = dh;
                        saldo.Tipo = dados.Tipo;
                        saldo.Valor = dados.ValorAposta;
                        db.Tbl02s.Add(saldo);
                    }
                    else
                    {
                        saldo.Valor += dados.ValorAposta;
                        db.Tbl02s.Update(saldo);
                    }
                    //Salvar Tudo
                    db.SaveChanges();
                    //Buscar Apostas Filhas Recém Lançadas
                    var filhas = db.Tbl05s.Where(x => x.Aposta == busca.Id && x.Ativo == true).ToList();
                    //Lançar Números Transformados
                    if (dados.Tipo.ToUpper().StartsWith("B"))
                    {
                        //Quando "B" - Apenas Replicar, pois não há transformações
                        foreach (var item in filhas)
                        {
                            var tbl08 = new Tbl08();
                            tbl08.Aposta = item.Aposta;
                            tbl08.ApostaFilha = item.Id;
                            tbl08.Ativo = true;
                            tbl08.NumeroReal = item.Numero;
                            tbl08.NumeroTransformado = item.Numero;
                            tbl08.Sorteado = false;
                            tbl08.SorteioFilho = 0;
                            tbl08.TipoAposta = busca.TipoAposta;
                            tbl08.TipoGrupo = busca.TipoGrupo;
                            tbl08.TipoEvento = busca.TipoEvento;
                            db.Tbl08s.Add(tbl08);
                        }
                    }
                    else
                    {
                        //Quando "J" - Criar de Acordo com o Tipo
                        foreach (var item in filhas)
                        {
                            //Variável dos Possíveis Números
                            var numeros = "";
                            //GRUPO
                            if (busca.TipoGrupo == "Grupo" || busca.TipoGrupo == "Duque Grupo" || busca.TipoGrupo == "Terno Grupo")
                            {
                                //Grupos são 4 dezenas referentes a um bicho escolhido na tabela
                                if (busca.TipoEvento == "1o Sorteio")
                                {
                                    //Aposta apenas no primeiro número sorteado
                                    numeros = item.Numero.ToString();
                                }
                                else if (busca.TipoEvento == "Cercado 1o ao 5o")
                                {
                                    //Procura do 1o ao 5o número sorteado
                                    numeros = item.Numero.ToString();
                                }
                                else if (busca.TipoEvento == "Cercado 1o ao 7o")
                                {
                                    //Procura do 1o ao 7o número sorteado
                                    numeros = item.Numero.ToString();
                                }
                            }

                            //DEZENA
                            else if (busca.TipoGrupo == "Dezena" || busca.TipoGrupo == "Duque Dezena" || busca.TipoGrupo == "Terno Dezena")
                            {
                                //Escolhe uma dezena entre 00 até 99
                                if (busca.TipoEvento == "1o Sorteio")
                                {
                                    //Aposta apenas no primeiro número sorteado
                                    numeros = item.Numero.ToString();
                                }
                                else if (busca.TipoEvento == "Cercado 1o ao 5o")
                                {
                                    //Procura do 1o ao 5o número sorteado
                                    numeros = item.Numero.ToString();
                                }
                            }

                            //CENTENA
                            else if (busca.TipoGrupo.StartsWith("Centena"))
                            {
                                //Se NÃO FOR INVERSÃO - Aposta apenas no primeiro número sorteado
                                if (dados.Inversao == false)
                                {
                                    numeros = item.Numero.ToString();
                                }
                                else
                                {
                                    //Escolhe uma centena que valerá as inversões dela ( 6 variações )
                                    //Exemplo: 123 => 123,132,213,231,312,321
                                    numeros = "" + item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(2, 1); // 123 => 123
                                    numeros = "," + item.Numero.ToString().Substring(0, 1) +
                                                    item.Numero.ToString().Substring(2, 1) +
                                                    item.Numero.ToString().Substring(1, 1); // 123 => 132
                                    numeros = "," + item.Numero.ToString().Substring(1, 1) +
                                                    item.Numero.ToString().Substring(0, 1) +
                                                    item.Numero.ToString().Substring(2, 1); // 123 => 213
                                    numeros = "," + item.Numero.ToString().Substring(0, 1) +
                                                    item.Numero.ToString().Substring(2, 1) +
                                                    item.Numero.ToString().Substring(1, 1); // 123 => 231
                                    numeros = "," + item.Numero.ToString().Substring(2, 1) +
                                                    item.Numero.ToString().Substring(0, 1) +
                                                    item.Numero.ToString().Substring(1, 1); // 123 => 312
                                    numeros = "," + item.Numero.ToString().Substring(2, 1) +
                                                    item.Numero.ToString().Substring(1, 1) +
                                                    item.Numero.ToString().Substring(0, 1); // 123 => 321
                                }
                            }

                            //MILHAR
                            else if (busca.TipoGrupo.StartsWith("Milhar"))
                            {

                                //Se NÃO FOR INVERSÃO - Aposta apenas no primeiro número sorteado
                                if (dados.Inversao == false)
                                {
                                    numeros = item.Numero.ToString();
                                }
                                else
                                {
                                    //Escolhe uma milhar que valerá as inversões dela ( 24 variações )
                                    //Exemplo: 1234 => 1234,1243,1324,1342,1423,1432
                                    //                 2134,2143,2314,2341,2413,2431
                                    //                 3124,3142,3214,3241,3412,3421
                                    //                 4123,4132,4213,4231,4312,4321
                                    numeros = "" + item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(3, 1); // 1234 => 1234
                                    numeros = "" + item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(2, 1); // 1234 => 1243
                                    numeros = "" + item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(3, 1); // 1234 => 1324
                                    numeros = "" + item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(1, 1); // 1234 => 1342
                                    numeros = "" + item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(2, 1); // 1234 => 1423
                                    numeros = "" + item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(1, 1); // 1234 => 1432
                                    numeros = "" + item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(3, 1); // 1234 => 2134
                                    numeros = "" + item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(2, 1); // 1234 => 2143
                                    numeros = "" + item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(3, 1); // 1234 => 2314
                                    numeros = "" + item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(0, 1); // 1234 => 2341
                                    numeros = "" + item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(2, 1); // 1234 => 2413
                                    numeros = "" + item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(0, 1); // 1234 => 2431
                                    numeros = "" + item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(3, 1); // 1234 => 3124
                                    numeros = "" + item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(1, 1); // 1234 => 3142
                                    numeros = "" + item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(3, 1); // 1234 => 3214
                                    numeros = "" + item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(0, 1); // 1234 => 3241
                                    numeros = "" + item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(1, 1); // 1234 => 3412
                                    numeros = "" + item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(0, 1); // 1234 => 3421
                                    numeros = "" + item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(2, 1); // 1234 => 4123
                                    numeros = "" + item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(1, 1); // 1234 => 4132
                                    numeros = "" + item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(2, 1); // 1234 => 4213
                                    numeros = "" + item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(0, 1); // 1234 => 4231
                                    numeros = "" + item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(0, 1) +
                                                   item.Numero.ToString().Substring(1, 1); // 1234 => 4312
                                    numeros = "" + item.Numero.ToString().Substring(3, 1) +
                                                   item.Numero.ToString().Substring(2, 1) +
                                                   item.Numero.ToString().Substring(1, 1) +
                                                   item.Numero.ToString().Substring(0, 1); // 1234 => 4321
                                }                                    
                            }
                            for (int i = 0; i < numeros.Split(',').Length; i++)
                            {
                                var tbl08 = new Tbl08();
                                tbl08.Aposta = item.Aposta;
                                tbl08.ApostaFilha = item.Id;
                                tbl08.Ativo = true;
                                tbl08.NumeroReal = item.Numero;
                                tbl08.NumeroTransformado = Convert.ToInt32(numeros.Split(',')[i].ToString());
                                tbl08.Sorteado = false;
                                tbl08.SorteioFilho = 0;
                                tbl08.TipoAposta = busca.TipoAposta;
                                tbl08.TipoGrupo = busca.TipoGrupo;
                                tbl08.TipoEvento = busca.TipoEvento;
                                db.Tbl08s.Add(tbl08);
                            }
                        }
                    }
                    //Salvar Tudo (De Novo)
                    db.SaveChanges();
                }
                return Ok(ret);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                ret.DataHora = dh;
                ret.Tipo = "Erro: " + ex.Message;
                ret.Valor = -1;
                return Ok(ret);
            }

        }

        [HttpPost("verApostasCliente")]
        public async Task<IActionResult> visualizarApostasCliente(verApostasModel dados)
        {

            //Calcular a próxima data com base no Tipo
            verApostasModel ret = new verApostasModel();
            DateTime dataMinima = DateTime.Now;

            try
            {
                //Tipos quando "B"olão
                if (dados.Tipo.ToUpper().StartsWith("B"))
                {
                    //Voltar Data Mínima como último Sábado às 20hrs 
                    dataMinima = DateTime.Now.AddDays((((int)DateTime.Now.DayOfWeek) + 1) * (-1));
                    dataMinima = Convert.ToDateTime(dataMinima.ToString("yyyy-MM-dd") + " 20:00:00");
                    dados.Tipo = "Bolão";
                }
                else
                {
                    if (dados.Tipo.ToUpper().StartsWith("J"))
                    {
                        //Voltar Data Mínima como último Domingo às 16hrs 
                        dataMinima = DateTime.Now.AddDays((((int)DateTime.Now.DayOfWeek)) * (-1));
                        dataMinima = Convert.ToDateTime(dataMinima.ToString("yyyy-MM-dd") + " 16:00:00");
                        dados.Tipo = "Jogo do Bicho";
                    }
                    else
                    {
                        throw new Exception("Tipo Inválido");
                    }
                }

                var pais = db.Tbl04s.Where(x => x.Pessoa == dados.Pessoa && x.Tipo == dados.Tipo && x.Ativo == true && x.DataHora >= dataMinima).ToList();
                foreach (var item in pais)
                {
                    ret.Apostas = new List<verApostasPais>();
                    ret.Apostas.Add(new verApostasPais
                    {
                        DataHora = item.DataHora,
                        TipoAposta = item.TipoAposta,
                        TipoGrupo = item.TipoGrupo,
                        TipoEvento = item.TipoEvento
                    });
                    var childs = new List<verApostasItensModel>();
                    var filhos = db.Tbl05s.Where(x => x.Aposta == item.Id && x.Ativo == true).ToList();
                    foreach (var itens in filhos)
                    {
                        childs.Add( new verApostasItensModel
                        {
                            Animal = itens.Animal,
                            Numero = itens.Numero,
                            Inversao = itens.Inversao,
                            Sorteado = itens.Sorteado
                        });
                    }
                    ret.Apostas[ret.Apostas.Count - 1].Itens = new List<verApostasItensModel>();
                    ret.Apostas[ret.Apostas.Count - 1].Itens.AddRange(childs);
                }

                return Ok(ret);

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                ret.Tipo = "Erro: " + ex.Message;
                return Ok(ret);
            }

        }

        [HttpPost("verApostas")]
        public async Task<IActionResult> visualizarApostas(verApostasModel dados)
        {

            //Calcular a próxima data com base no Tipo
            verApostasModel ret = new verApostasModel();
            DateTime dataMinima = DateTime.Now;

            try
            {
                //Tipos quando "B"olão
                if (dados.Tipo.ToUpper().StartsWith("B"))
                {
                    //Voltar Data Mínima como último Sábado às 20hrs 
                    dataMinima = DateTime.Now.AddDays((((int)DateTime.Now.DayOfWeek) + 1) * (-1));
                    dataMinima = Convert.ToDateTime(dataMinima.ToString("yyyy-MM-dd") + " 20:00:00");
                    dados.Tipo = "Bolão";
                }
                else
                {
                    if (dados.Tipo.ToUpper().StartsWith("J"))
                    {
                        //Voltar Data Mínima como último Domingo às 16hrs 
                        dataMinima = DateTime.Now.AddDays((((int)DateTime.Now.DayOfWeek)) * (-1));
                        dataMinima = Convert.ToDateTime(dataMinima.ToString("yyyy-MM-dd") + " 16:00:00");
                        dados.Tipo = "Jogo do Bicho";
                    }
                    else
                    {
                        throw new Exception("Tipo Inválido");
                    }
                }

                var pais = db.Tbl04s.Where(x => x.Tipo == dados.Tipo && x.Ativo == true && x.DataHora >= dataMinima).ToList();
                foreach (var item in pais)
                {
                    var cli = db.Tbl01s.Where(x => x.Id == item.Pessoa && x.Ativo == true).FirstOrDefault();
                    if (cli != null)
                    {
                        ret.Apostas = new List<verApostasPais>();
                        ret.Apostas.Add(new verApostasPais
                        {
                            DataHora = item.DataHora,
                            TipoAposta = (!string.IsNullOrEmpty(cli.Nome) ? cli.Nome : cli.Documento ) + "=>" + item.TipoAposta,
                            TipoGrupo = item.TipoGrupo,
                            TipoEvento = item.TipoEvento
                        });
                        var childs = new List<verApostasItensModel>();
                        var filhos = db.Tbl05s.Where(x => x.Aposta == item.Id && x.Ativo == true).ToList();
                        foreach (var itens in filhos)
                        {
                            childs.Add(new verApostasItensModel
                            {
                                Animal = itens.Animal,
                                Numero = itens.Numero,
                                Inversao = itens.Inversao,
                                Sorteado = itens.Sorteado
                            });
                        }
                        ret.Apostas[ret.Apostas.Count - 1].Itens = new List<verApostasItensModel>();
                        ret.Apostas[ret.Apostas.Count - 1].Itens.AddRange(childs);
                    }
                }

                return Ok(ret);

            }
            catch (Exception ex)
            {
                if (ex.InnerException != null) ex = ex.InnerException;
                ret.Tipo = "Erro: " + ex.Message;
                return Ok(ret);
            }

        }

    }

}

