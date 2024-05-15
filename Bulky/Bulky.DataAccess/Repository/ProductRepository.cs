using Bulky.Book.DataAccess.Repository.IRepository;

using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Book.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }



        public void Update(Product product)
        {
            var objFromDb=_db.Products.FirstOrDefault(u=>u.Id==product.Id);
            if(objFromDb != null)
            {
                objFromDb.ISBN = product.ISBN;
                objFromDb.ListPrice = product.ListPrice;
                objFromDb.Price = product.Price;
                objFromDb.Price100 = product.Price100;
                objFromDb.Price50 = product.Price50;
                objFromDb.Author = product.Author;
                objFromDb.Description = product.Description;
                objFromDb.Title = product.Title;
                objFromDb.CategoryId = product.CategoryId;
                if(objFromDb.ImageUrl==null)
                    objFromDb.ImageUrl=product.ImageUrl;
            }
           
        }
    }
}
