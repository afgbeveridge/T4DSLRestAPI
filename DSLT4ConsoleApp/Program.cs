﻿using Complex.Omnibus.Autogenerated.ExpansionHandling.Blogs;
using Complex.Omnibus.Autogenerated.ExpansionHandling.Posts;
using EF.Model;
using System;
using System.Linq.Expressions;

namespace ConsoleApp {

    public class Program {

        public static void Main(string[] args) {
            try {
                SeedThenExecute(ctx => {
                    ExecuteBlogExpandedQuery(ctx);
                    ExecuteBlogExpandedQuery(ctx, null, BlogsQueryHandler.ExpandPosts, BlogsQueryHandler.ExpandAuthor);
                    ExecuteBlogExpandedQuery(ctx, b => b.BlogId > 0, BlogsQueryHandler.ExpandAuthor, BlogsQueryHandler.ExpandPosts, BlogsQueryHandler.ExpandReaders);
                    ExecutePostExpandedQuery(ctx, PostsQueryHandler.ExpandReaders);
                    ExecuteBlogExpandedQuery(ctx, expansions: "X");
                });
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }

        private static void SeedThenExecute(Action<BloggingContext> query) {
            using (BloggingContext ctx = new BloggingContext()) {
                ctx.SeedIfNeeded();
                query(ctx);
            }
        }

        private static void ExecuteBlogExpandedQuery(BloggingContext ctx, Expression<Func<Blog, bool>> filter = null, params string[] expansions) {
            var result = new BlogsQueryHandler().GetBlogsWithExpansion(ctx, filter, expansions: expansions).Result;
            Console.WriteLine();
        }

        private static void ExecutePostExpandedQuery(BloggingContext ctx, params string[] expansions) {
            var result = new PostsQueryHandler().GetPostsWithExpansion(ctx, b => true, expansions: expansions).Result;
            Console.WriteLine();
        }

    }
}
