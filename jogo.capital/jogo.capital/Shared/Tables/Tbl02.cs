using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace jogo.capital.Shared.Tables
{
    [Table("Tbl02")]
    [Index("Ativo", "Tipo", "DataHora", "Id", Name = "I01_Tbl02")]
    [Index("Tipo", "Ativo", "DataHora", "Id", Name = "I02_Tbl02")]
    public partial class Tbl02
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string Tipo { get; set; } = null!;
        [Column(TypeName = "datetime")]
        public DateTime DataHora { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Valor { get; set; }
        [Required]
        public bool Ativo { get; set; }
    }
}
