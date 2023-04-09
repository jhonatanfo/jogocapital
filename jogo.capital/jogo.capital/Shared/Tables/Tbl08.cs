using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace jogo.capital.Shared.Tables
{
    [Table("Tbl08")]
    [Index("Aposta", "ApostaFilha", "Ativo", "Id", Name = "I01_Tbl08")]
    [Index("NumeroTransformado", "Ativo", "Id", Name = "I02_Tbl08")]
    [Index("Sorteado", "SorteioFilho", "Ativo", "Id", Name = "I03_Tbl08")]
    [Index("SorteioFilho", "Sorteado", "Ativo", "Id", Name = "I04_Tbl08")]
    public partial class Tbl08
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        public int Aposta { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string TipoAposta { get; set; } = null!;
        [StringLength(20)]
        [Unicode(false)]
        public string TipoGrupo { get; set; } = null!;
        [StringLength(20)]
        [Unicode(false)]
        public string TipoEvento { get; set; } = null!;
        public int ApostaFilha { get; set; }
        public int NumeroReal { get; set; }
        public int NumeroTransformado { get; set; }
        [Required]
        public bool Sorteado { get; set; }
        public int SorteioFilho { get; set; }
        [Required]
        public bool Ativo { get; set; }
    }
}
