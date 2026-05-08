using NotificationDALLibrary.Interface;
using NotificationModelLibrary;

namespace NotificationDALLibrary.Repository
{
    public class UserRepository : IRepository<int, User>
    {
        Dictionary<int, User> _userMap = new Dictionary<int, User>(); //dictionary to store details with key and value
        private int LastUserid = 0; // variable to create unique userid

        //indexer for creating and fetching based on index
        public User this[int index]
        {
            get { return _userMap[index]; }
            set { _userMap[index] = value; }
        }

        //method to create a new user
        public User Create(User item)
        {
            LastUserid++;
            item.UserId = LastUserid; //assigning userid to the object
            _userMap.Add(LastUserid, item);
            return item;
        }

        //method to delete the user based on userid
        public User? Delete(int key)
        {
            var user = _userMap.FirstOrDefault(u => u.Key == key).Value; //fetching user details using LINQ operator
            if (user != null)
            {
                _userMap.Remove(key);
                return user;
            }
            return null;
        }

        //method to get data of a particular user 
        public User? GetData(int key)
        {
            //using LINQ for searching if the key is not present it will return null
            return _userMap.FirstOrDefault(u => u.Key == key).Value;
        }

        //method to get all the user data
        public List<User>? GetAllData()
        {
            if (_userMap.Count > 0)
            {
                var list = _userMap.Values.ToList();
                return list;
            }
            return null;
        }

        //method for updating a user detail based on user id
        public User? Update(int key, User item)
        {
            var user = _userMap.FirstOrDefault(u => u.Key == key).Value; //fetching user details using LINQ operator
            if (user != null)
            {
                _userMap[key] = item;
                return user;
            }
            return null;
        }
    }
}