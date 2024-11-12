using BookZone.DataAccess.Data;
using BookZone.DataAccess.Repository.IRepository;
using BookZone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookZone.DataAccess.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;
        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }//since we use repository pattern we need to pass db to base class
        //we can use base class methods in this class
        //we can also use _db.Categories.Add() but we are using base class methods


        public void Save()
        {
           _db.SaveChanges();
        }

        public void Update(Category obj)
        {
            _db.Categories.Update(obj);
        }
    }
}
