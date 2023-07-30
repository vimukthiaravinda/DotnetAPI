using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public class UserRepository : IUserRepository
    {
    DataContextEF _enitiyFramework;
    public UserRepository(IConfiguration configuration)
    {
        _enitiyFramework = new DataContextEF(configuration);
    }
    public bool SaveChanges()
    {
        return _enitiyFramework.SaveChanges() > 0;
    }
    public void AddEntity<T>(T entityToAdd)
    {
        if (entityToAdd != null)
        {
            _enitiyFramework.Add(entityToAdd);
        }
    }
    public void RemoveEntity<T>(T entityToAdd)
    {
        if (entityToAdd != null)
        {
            _enitiyFramework.Remove(entityToAdd);
        }
    }
    public IEnumerable<User> GetUsers()
    {
        IEnumerable<User> users = _enitiyFramework.Users.ToList<User>();
        return users;
    }
    public User GetSingleUser(int userId)
    {
        User? singleuser = _enitiyFramework.Users.Where(u => u.UserId.Equals(userId)).FirstOrDefault<User>();
        if (singleuser != null)
        {
             return singleuser; 
        }
        throw new Exception("Falid to Get User");
       
    }
    }
}