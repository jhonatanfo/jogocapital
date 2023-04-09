using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace jogo.capital.Shared.Tables
{
    [Table("Tbl01")]
    [Index("Documento", "Ativo", "Id", Name = "I01_Tbl01")]
    [Index("Ativo", "Documento", "Id", Name = "I02_Tbl01")]
    public partial class Tbl01
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [StringLength(20)]
        [Unicode(false)]
        public string Tipo { get; set; } = null!;
        [StringLength(100)]
        [Unicode(false)]
        public string Documento { get; set; } = null!;
        [StringLength(20)]
        [Unicode(false)]
        public string ContraSenha { get; set; } = null!;
        [Required]
        public DateTime Validade { get; set; }
        [StringLength(100)]
        [Unicode(false)]
        public string Senha { get; set; } = null!;
        [Column(TypeName = "decimal(12, 2)")]
        public decimal? Saldo { get; set; }
        [StringLength(100)]
        [Unicode(false)]
        public string? Nome { get; set; } = null!;
        [Column("CPF")]
        [StringLength(20)]
        [Unicode(false)]
        public string? Cpf { get; set; } = null!;
        [Column(TypeName = "date")]
        public DateTime? Nascimento { get; set; }
        [StringLength(100)]
        [Unicode(false)]
        public string? Banco { get; set; } = null!;
        [StringLength(20)]
        [Unicode(false)]
        public string? Agencia { get; set; } = null!;
        [StringLength(20)]
        [Unicode(false)]
        public string? Conta { get; set; } = null!;
        [StringLength(100)]
        [Unicode(false)]
        public string? Titular { get; set; } = null!;
        [StringLength(20)]
        [Unicode(false)]
        public string? CpfTitular { get; set; } = null!;
        [StringLength(100)]
        [Unicode(false)]
        public string? Pix { get; set; } = null!;
        [Required]
        public bool Ativo { get; set; }
    }
}
