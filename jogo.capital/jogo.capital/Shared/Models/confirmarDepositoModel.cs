using System;
using System.ComponentModel.DataAnnotations;

namespace jogo.capital.Shared.Models
{
	public class confirmarDepositoModel
    {
        public string Identificador { get; set; } = "";
        public DateTime DataHoraPagamento { get; set; } = DateTime.Now;
    }
}

