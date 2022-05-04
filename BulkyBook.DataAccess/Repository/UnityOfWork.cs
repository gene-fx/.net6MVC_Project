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
        }

        public ICategoryRepository Category { get; private set; }


        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
