using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace jogo.capital.Shared.Tables
{
    [Table("Tbl06")]
    [Index("Ativo", "Pessoa", "Tipo", "DataHora", "Id", Name = "I01_Tbl06")]
    [Index("Ativo", "Identificador", "Id", Name = "I02_Tbl06")]
    [Index("Identificador", "Ativo", "Id", Name = "I03_Tbl06")]
    public partial class Tbl06
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
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Valor { get; set; }
        [StringLength(100)]
        [Unicode(false)]
        public string Pix { get; set; } = null!;
        [StringLength(100)]
        [Unicode(false)]
        public string Banco { get; set; } = null!;
        [StringLength(20)]
        [Unicode(false)]
        public string Agencia { get; set; } = null!;
        [StringLength(20)]
        [Unicode(false)]
        public string Conta { get; set; } = null!;
        [StringLength(100)]
        [Unicode(false)]
        public string Identificador { get; set; } = null!;
        [Column(TypeName = "datetime")]
        public DateTime? Pagamento { get; set; }
        [Required]
        public bool Ativo { get; set; }
    }
}
