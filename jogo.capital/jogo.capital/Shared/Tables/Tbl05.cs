using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace jogo.capital.Shared.Tables
{
    [Table("Tbl05")]
    [Index("Ativo", "Aposta", "DataHora", "Numero", "Id", Name = "I01_Tbl05")]
    [Index("Aposta", "Ativo", "DataHora", "Numero", "Id", Name = "I02_Tbl05")]
    [Index("Ativo", "Numero", "DataHora", "Id", Name = "I03_Tbl05")]
    [Index("Numero", "Ativo", "DataHora", "Id", Name = "I04_Tbl05")]
    public partial class Tbl05
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        public int Aposta { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DataHora { get; set; }
        public int Animal { get; set; }
        public int Numero { get; set; }
        public bool Inversao { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Preco { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Lucro { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Desconto { get; set; }
        public bool Sorteado { get; set; }
        [Required]
        public bool Ativo { get; set; }
    }
}
