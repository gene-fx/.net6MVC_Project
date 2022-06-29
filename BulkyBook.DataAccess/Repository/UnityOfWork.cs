using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.DataAccess;

namespace BulkyBook.DataAccess.Repository
{
    public class UnityOfWork : IUnityOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnityOfWork(ApplicationDbContext db)
        {
            _db = db;
            Category = new CategoryRepository(_db);
            CoverType = new CoverTyperRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
        }

        public ICategoryRepository Category { get; private set; }

        public ICoverTypeRepository CoverType { get; set; }

        public IProductRepository Product { get; private set; }

        public ICompanyRepository Company { get; set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
