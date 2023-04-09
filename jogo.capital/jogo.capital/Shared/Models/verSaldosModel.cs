using System;
using System.ComponentModel.DataAnnotations;

namespace jogo.capital.Shared.Models
{
	public class verSaldosModel
    {

        public DateTime DataHora { get; set; }
        public string Tipo { get; set; }
        public decimal Valor { get; set; }

    }
}

