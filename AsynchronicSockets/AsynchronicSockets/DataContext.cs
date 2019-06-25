namespace AsynchronicSockets
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class DataContext : DbContext
    {
        public DataContext()
            : base("name=DataContext1")
        {
        }

        public DbSet<Street> Streets { get; set; }
    }
}