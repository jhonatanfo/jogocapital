using System;
using System.ComponentModel.DataAnnotations;

namespace jogo.capital.Shared.Models
{
	public class lancarSorteioModel
    {
        public string Tipo { get; set; } = "";
        public DateTime DataHora { get; set; } = DateTime.Now;
        public int Numero01 { get; set; } = 0;
        public int Numero02 { get; set; } = 0;
        public int Numero03 { get; set; } = 0;
        public int Numero04 { get; set; } = 0;
        public int Numero05 { get; set; } = 0;
        public int Numero06 { get; set; } = 0;
        public int Numero07 { get; set; } = 0;
        public int Numero08 { get; set; } = 0;
        public int Numero09 { get; set; } = 0;
        public int Numero10 { get; set; } = 0;
    }
}

