using NotificationDALLibrary.Interface;
using NotificationModelLibrary;
using Npgsql;

namespace NotificationDALLibrary.Repository
{
    public class UserRepository : IRepository<int, User>
    {
        //hardcoded values are not pushed into git need to use .env instead
        //so removed after testing
        string ConnectionUrl = "";

        NpgsqlConnection connection;

        //initializing connection
        public UserRepository()
        {
            connection = new NpgsqlConnection(ConnectionUrl);
        }

        //Need to use parameterized query to avoid sql injection
        //method for inserting into user table
        public User? Create(User item)
        {
            //insert query for inserting new user
            string InsertQuery = $"INSERT INTO users(name , email , phonenumber , createdat) VALUES('{item.Name}','{item.Email}','{item.PhoneNumber}','{item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")}')";
            NpgsqlCommand command = new NpgsqlCommand(InsertQuery, connection);

            try
            {
                connection.Open(); //opening a connection
                int result = command.ExecuteNonQuery(); //executing the query
                if (result > 0) //check the return affected rows
                {
                    return item;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.Close(); //close the connection
            }
        }

        //method for deleting a user
        public User? Delete(int key)
        {
            var user = GetData(key); //checking if the user exits

            if (user != null)
            {
                //query for deleting the user
                string DeleteQuery = $"DELETE FROM users WHERE userid={key}";
                NpgsqlCommand command = new NpgsqlCommand(DeleteQuery, connection);

                try
                {
                    connection.Open(); //opening a connection
                    int result = command.ExecuteNonQuery(); //executing query

                    if (result > 0) //checking the affected rows
                    {
                        return user;
                    }
                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return null;
                }
                finally
                {
                    connection?.Close(); //closing the connection
                }
            }
            return null;
        }

        //method for getting particular user details
        public User? GetData(int key)
        {
            //select query for fetching user detail
            string SelectQuery = $"SELECT * FROM users WHERE userid={key}";
            NpgsqlCommand command = new NpgsqlCommand(SelectQuery, connection);

            try
            {
                connection.Open(); //opening a new connection
                NpgsqlDataReader reader = command.ExecuteReader(); //executing query

                if (reader.Read())
                {
                    User user = new User(); //creating a new user object

                    //assigning values to the new object
                    user.UserId = reader.GetInt32(0);
                    user.Name = reader.GetString(1);
                    user.Email = reader.GetString(2);
                    user.PhoneNumber = reader.GetString(3);
                    user.CreatedAt = reader.GetDateTime(4);

                    return user;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.Close(); //closing the connection
            }
        }

        //method for returning all the existing user 
        public List<User>? GetAllData()
        {
            //list for storing user details
            List<User> userlist = new List<User>();

            //query for selecting all the users
            string SelectQuery = "SELECT * FROM users";
            NpgsqlCommand command = new NpgsqlCommand(SelectQuery, connection);

            try
            {
                connection.Open(); //opening a connection
                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    //creating an object for user
                    User user = new User();

                    //assigning values from the table
                    user.UserId = reader.GetInt32(0);
                    user.Name = reader.GetString(1);
                    user.Email = reader.GetString(2);
                    user.PhoneNumber = reader.GetString(3);
                    user.CreatedAt = reader.GetDateTime(4);

                    userlist.Add(user);
                }

                //check the list has any values
                if (userlist.Count > 0)
                {
                    return userlist;
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection.Close(); // close connection
            }
        }

        //method for updating the user details
        public User? Update(int key, User item)
        {
            //update query for updating user detail
            string UpdateQuery = $"UPDATE users SET name='{item.Name}' , email='{item.Email}' , PhoneNumber='{item.PhoneNumber}' WHERE userid={key}";
            NpgsqlCommand command = new NpgsqlCommand(UpdateQuery, connection);

            try
            {
                connection.Open(); //opening a connection
                int result = command.ExecuteNonQuery(); //executing update query

                if (result > 0) //check the affected rows
                {
                    return item;
                }
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.Close(); //close connection
            }
        }
    }
}