using Microsoft.EntityFrameworkCore;
using test7.Models;

namespace test7.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base()
        {
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Produit> Produits { get; set; }
        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.Cart> Cart { get; set; }
        public DbSet<Models.CartItem> CartItem { get; set; }
<<<<<<< HEAD
        public DbSet<Models.Categorie> Categorie { get; set; }
=======
>>>>>>> 9c21152ef3d47e1699b001f031c199b4a35905fa

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "Server=localhost;Database=test1;User=root;Password=;";
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

    }
}
