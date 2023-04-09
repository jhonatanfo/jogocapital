using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using jogo.capital.Shared.Tables;

namespace jogo.capital.Server.Context
{
    public partial class dbCtx : DbContext
    {

        public dbCtx(DbContextOptions<dbCtx> options) : base(options)
        {
        }

        public virtual DbSet<Tbl01> Tbl01s { get; set; } = null!;
        public virtual DbSet<Tbl02> Tbl02s { get; set; } = null!;
        public virtual DbSet<Tbl03> Tbl03s { get; set; } = null!;
        public virtual DbSet<Tbl04> Tbl04s { get; set; } = null!;
        public virtual DbSet<Tbl05> Tbl05s { get; set; } = null!;
        public virtual DbSet<Tbl06> Tbl06s { get; set; } = null!;
        public virtual DbSet<Tbl07> Tbl07s { get; set; } = null!;
        public virtual DbSet<Tbl08> Tbl08s { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                //optionsBuilder.UseSqlServer("Server=sala63.qualit.tec.br;Database=jb;User Id=sa;password=C4n4r!0;Trusted_Connection=False;MultipleActiveResultSets=true;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
