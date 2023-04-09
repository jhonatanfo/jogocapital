using System;
using System.ComponentModel.DataAnnotations;

namespace jogo.capital.Shared.Models
{
	public class verApostasModel
    {

        public int Pessoa { get; set; }
        public string Tipo { get; set; }
        public List<verApostasPais>? Apostas { get; set; }
    }
    public class verApostasPais
    {
        public DateTime? DataHora { get; set; }
        public string? TipoAposta { get; set; }
        public string? TipoGrupo { get; set; }
        public string? TipoEvento { get; set; }
        public List<verApostasItensModel>? Itens { get; set; }
    }
    public class verApostasItensModel
    {
        public int Animal { get; set; }
        public int Numero { get; set; }
        public bool Inversao { get; set; }
        public bool Sorteado { get; set; }
    }
}

