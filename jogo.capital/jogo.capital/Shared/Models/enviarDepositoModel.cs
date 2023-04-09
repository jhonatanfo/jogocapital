using System;
using System.ComponentModel.DataAnnotations;

namespace jogo.capital.Shared.Models
{
    public class enviarDepositoModel
    {
        public int Pessoa { get; set; }
        public string Tipo { get; set; }
        public decimal Valor { get; set; }
        public string Pix { get; set; }
        public string Banco { get; set; }
        public string Agencia { get; set; }
        public string Conta { get; set; }
    }
}

