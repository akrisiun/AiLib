using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ai.Entity
{
    public abstract class DbSet<T> where T : class
    {
        public DbSet()
        {
        }

        Type Type { get { return typeof(T); } }

        public abstract IEnumerable<T> Load();
        public abstract T Get(object id);

    }

    public abstract class DbSetSave<T> : DbSet<T> where T : class
    {
        public ICollection<T> Changed { get; set; }

        public abstract bool Save();
    }

}
