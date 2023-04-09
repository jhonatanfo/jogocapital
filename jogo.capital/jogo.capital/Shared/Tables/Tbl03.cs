using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace jogo.capital.Shared.Tables
{
    [Table("Tbl03")]
    [Index("Ativo", "Tipo", "DataHora", "Situacao", "Sorteio", "Numero", "Id", Name = "I01_Tbl03")]
    [Index("Tipo", "DataHora", "Situacao", "Ativo", "Sorteio", "Numero", "Id", Name = "I02_Tbl03")]
    [Index("Ativo", "Numero", "Situacao", "Id", Name = "I03_Tbl03")]
    [Index("Numero", "Ativo", "Situacao", "Id", Name = "I04_Tbl03")]
    public partial class Tbl03
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string Tipo { get; set; } = null!;
        [Column(TypeName = "datetime")]
        public DateTime DataHora { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string Situacao { get; set; } = null!;
        [StringLength(10)]
        [Unicode(false)]
        public string Sorteio { get; set; } = null!;
        public int Numero { get; set; }
        public int Sequencia { get; set; }
        [Required]
        public bool Ativo { get; set; }
    }
}
