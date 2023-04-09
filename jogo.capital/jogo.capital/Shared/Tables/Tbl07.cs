using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace jogo.capital.Shared.Tables
{
    [Table("Tbl07")]
    [Index("Ativo", "Id", Name = "I01_Tbl07")]
    public partial class Tbl07
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime DataHora { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Grupo { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Dezena { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Centena { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Milhar { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal DuqueDeGrupo { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal DuqueDeDezena { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal TernoDeDezena { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal TernoDeGrupo { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Cerdado1ao5 { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal Cercado1ao7 { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal CentenaInversao { get; set; }
        [Column(TypeName = "decimal(12, 2)")]
        public decimal MilharInversao { get; set; }
        [Required]
        public bool Ativo { get; set; }
    }
}
