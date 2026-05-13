using Npgsql;
using WordGuessingModelLibrary;
using WordGuessingDALLibrary.Interfaces;

namespace WordGuessingDALLibrary.Repository
{
    public class UserRepository : IUserRepository
    {
        //Avoid using hardcoded values instead use .env 
        //connection url
        string ConnectionUrl = "Host=localhost;Port=5432;Database=game;Username=aswin;Password=Nopassword123";

        NpgsqlConnection connection;

        //constructor for initializing connection
        public UserRepository()
        {
            connection = new NpgsqlConnection(ConnectionUrl);
        }

        //Better to use parameterized queries instead of string interpolation to avoid sql injection issue
        //method for creating a new user
        public bool Create(Users user)
        {
            //query for inserting into the table
            string InsertQuery = "INSERT INTO users(username , password , createdat) VALUES(@username,@password,@createdat)";
            NpgsqlCommand command = new NpgsqlCommand(InsertQuery, connection);

            command.Parameters.AddWithValue("@username", user.UserName);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@createdat", user.CreatedAt);

            try
            {
                connection.Open(); //opening a connection
                int result = command.ExecuteNonQuery();

                if (result > 0)
                {
                    return true;
                }
                return false;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            finally
            {
                connection?.Close(); //closing the connection
            }
        }


        //method for checking the user is already present
        public bool GetData(Users user)
        {
            //query to execute select operation
            string SelectQuery = "SELECT * FROM users WHERE username=@username AND password=@password";
            NpgsqlCommand command = new NpgsqlCommand(SelectQuery, connection);

            command.Parameters.AddWithValue("@username", user.UserName);
            command.Parameters.AddWithValue("@password", user.Password);

            try
            {
                connection.Open(); //open a connection
                NpgsqlDataReader reader = command.ExecuteReader(); //read the values

                if (reader.Read()) //check value is present
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            finally
            {
                connection?.Close(); //close connection
            }
        }

        //check the user already exists
        public bool CheckUser(string username)
        {
            //query to execute select operation
            string SelectQuery = "SELECT * FROM users WHERE username=@username";
            NpgsqlCommand command = new NpgsqlCommand(SelectQuery, connection);

            command.Parameters.AddWithValue("@username", username);

            try
            {
                connection.Open(); //open a connection
                NpgsqlDataReader reader = command.ExecuteReader(); //read the values

                if (reader.Read()) //check value is present
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            finally
            {
                connection?.Close(); //close connection
            }
        }
    }
}