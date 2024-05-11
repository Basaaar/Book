using Bulky.Book.DataAccess.Repository.IRepository;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Book.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository Category { get; private set; }
        private readonly ApplicationDbContext _db;
      
        public UnitOfWork(ApplicationDbContext _db)
        {
            this._db = _db;
            Category = new CategoryRepository(_db);

        }
        public void Save()
        {
            _db.SaveChanges();  
        }
    }
}
