using BookZone.DataAccess.Data;
using BookZone.DataAccess.Repository.IRepository;
using BookZone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookZone.DataAccess.Repository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private ApplicationDbContext _db;
        public UserRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(User obj)
        {
            _db.Users.Update(obj);
        }

        public User GetByEmail(string email)
        {
            return _db.Users.FirstOrDefault(u => u.Email.ToLower().Trim() == email.ToLower().Trim());
        }

        public User GetByResetToken(string resetToken)
        {
            return _db.Users.FirstOrDefault(u => u.ResetToken == resetToken);
        }

    }
}
