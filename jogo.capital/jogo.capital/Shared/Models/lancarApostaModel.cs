using System;
using System.ComponentModel.DataAnnotations;

namespace jogo.capital.Shared.Models
{
	public class lancarApostaModel
    {
        public string Tipo { get; set; } = "";
        public DateTime DataHora { get; set; } = DateTime.Now;
        public int Pessoa { get; set; } = 0;
        public string TipoAposta { get; set; } = "";
        public string TipoGrupo { get; set; } = "";
        public string TipoEvento { get; set; } = "";
        public decimal ValorAposta { get; set; } = 0;
        public int Qtde { get; set; } = 0;
        public bool Inversao { get; set; } = false;
        public List<lancarApostaItensModel> Itens { get; set; } = new List<lancarApostaItensModel>();
    }
    public class lancarApostaItensModel
    {
        public int Animal { get; set; }
        public int Numero { get; set; }
        public bool Inversao { get; set; }
        public decimal Preco { get; set; }
    }
}

