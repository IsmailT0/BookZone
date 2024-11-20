using BookZone.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookZone.DataAccess.Repository.IRepository
{
    public interface IUserRepository : IRepository<User>
    {
        void Update(User obj);

        User GetByEmail(string email);
        User GetByResetToken(string resetToken);
    }
}
