using System.Data.Entity;
using CatalogoRopaCalzado.Models;

namespace CatalogoRopaCalzado.Data
{
    public class CatalogoContext : DbContext
    {
        static CatalogoContext()
        {
            // El esquema se instala mediante los scripts SQL de la entrega.
            Database.SetInitializer<CatalogoContext>(null);
        }

        public CatalogoContext() : base("name=Catalogo")
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallePedidos { get; set; }
        public DbSet<Factura> Facturas { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Producto>().Property(p => p.Precio).HasPrecision(12, 2);
            modelBuilder.Entity<DetallePedido>().Property(p => p.Precio).HasPrecision(12, 2);
            modelBuilder.Entity<Pedido>().Property(p => p.Total).HasPrecision(18, 2);
            modelBuilder.Entity<Pedido>().HasMany(p => p.Detalles).WithRequired()
                .HasForeignKey(d => d.PedidoId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Factura>().HasRequired(f => f.Pedido).WithMany()
                .HasForeignKey(f => f.PedidoId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Usuario>().Property(u => u.BloqueadoHasta).HasColumnType("datetime2");
            modelBuilder.Entity<Pedido>().Property(p => p.Fecha).HasColumnType("datetime2");
            modelBuilder.Entity<Pedido>().Property(p => p.FechaPago).HasColumnType("datetime2");
            modelBuilder.Entity<Factura>().Property(f => f.Fecha).HasColumnType("datetime2");
            modelBuilder.Entity<Producto>().HasRequired(p => p.Categoria)
                .WithMany().HasForeignKey(p => p.CategoriaId).WillCascadeOnDelete(false);
            base.OnModelCreating(modelBuilder);
        }
    }
}
