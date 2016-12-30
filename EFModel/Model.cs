using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EF.Model {

    public class BloggingContext : DbContext {

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Party> Parties { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlServer(@"Data Source=.\sqlexpress;Database=BlogNR;Trusted_Connection=True;");
        }

        // Test shim really

        public void SeedIfNeeded() {
            Database.EnsureCreated();
            if (!Blogs.Any()) {
                AddBlogs(100, 3);
                AddPosts(101, 5);
                AddPosts(999, 1);
                AddAuthors(100, 3);
                AddReaders(102, 5);
                AddReaders(999, 2);
                SaveChanges();
            }
        }

        private void AddBlogs(int startId, int cnt) {
            Enumerable.Range(0, cnt).ToList().ForEach(i => Blogs.Add(new Blog { NonKeyField = i + startId, Url = i.ToString() }));
        }

        private void AddPosts(int id, int cnt) {
            Enumerable.Range(1, cnt).ToList().ForEach(i => Posts.Add(new Post { NonKeyField = id, Content = "P" + i, Title = "T" + i }));
        }

        private void AddAuthors(int startId, int cnt) {
            Enumerable.Range(0, cnt).ToList().ForEach(i => Parties.Add(new Party { NonKeyField = startId + i, Name = "A" + i, Disposition = "author" }));
        }

        private void AddReaders(int id, int cnt) {
            Enumerable.Range(0, cnt).ToList().ForEach(i => Parties.Add(new Party { NonKeyField = id, Name = "R" + i, Disposition = "reader" }));
        }
    }

    // These 'entities' simulate objects that have been reverse engineered from a legacy database (or poorly normalised one) that lacks the notion of foreign keys

    public class Blog {
        public int BlogId { get; set; }
        public int NonKeyField { get; set; } // Links a blog to a post
        public string Url { get; set; }
    }

    public class Post {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int NonKeyField { get; set; } // Links a post to a blog
    }

    public class Party {
        public int PartyId { get; set; }
        public string Name { get; set; }
        public int NonKeyField { get; set; } // Links a party to an object of some type
        public string Disposition { get; set; }
    }
}


