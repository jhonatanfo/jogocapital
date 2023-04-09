using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace jogo.capital.Shared.Tables
{
    [Table("Tbl04")]
    [Index("Ativo", "Pessoa", "Tipo", "DataHora", "Id", Name = "I01_Tbl04")]
    [Index("Pessoa", "Tipo", "Ativo", "DataHora", "Id", Name = "I02_Tbl04")]
    public partial class Tbl04
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        public int Pessoa { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string Tipo { get; set; } = null!;
        [Column(TypeName = "datetime")]
        public DateTime DataHora { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string TipoAposta { get; set; } = null!;
        [StringLength(20)]
        [Unicode(false)]
        public string TipoGrupo { get; set; } = null!;
        [StringLength(20)]
        [Unicode(false)]
        public string TipoEvento { get; set; } = null!;
        [Column(TypeName = "decimal(12, 2)")]
        public decimal ValorAposta { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Lucro { get; set; }
        public int Quantidade { get; set; }
        [Required]
        public bool Ativo { get; set; }
    }
}
