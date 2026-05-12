using Npgsql;
using WordGuessingModelLibrary;

namespace WordGuessingDALLibrary.Repository
{
    public class ResultRepository : IResultRepository
    {
        //Avoid using hardcoded values instead use .env 
        //connection string
        string ConnectionUrl = "Host=localhost;Port=5432;Database=game;Username=aswin;Password=Nopassword123";

        NpgsqlConnection connection;

        //Constructor for initializing the Npgsql connection
        public ResultRepository()
        {
            connection = new NpgsqlConnection(ConnectionUrl);
        }
        
        //Better to use parameterized queries instead of string interpolation to avoid sql injection issue
        //method for inserting final scores
        public Results? Create(Results result)
        {
            //query for inserting value
            string InsertQuery = $"INSERT INTO results(username,total_attempt,word , score , played_at) VALUES('{result.UserName}' , '{result.Attempt}' ,  '{result.Word}' , '{result.Score}' , '{result.PlayedAt.ToString("yyyy-MM-dd HH:mm:ss")}')";
            NpgsqlCommand command = new NpgsqlCommand(InsertQuery, connection);

            try
            {
                connection.Open(); //opening a new connection
                int res = command.ExecuteNonQuery();

                if (res > 0) //check the effected rows
                {
                    return result;
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

        public List<Leader>? GetData()
        {
            List<Leader> resultlist = new List<Leader>();

            //query for inserting value
            string SelectQuery = $"SELECT username , COUNT(*) AS total_games , SUM(score) AS total_score , MAX(score) AS best_score FROM results GROUP BY username ORDER BY total_score DESC LIMIT 10";
            NpgsqlCommand command = new NpgsqlCommand(SelectQuery, connection);

            try
            {
                connection.Open(); //opening a new connection
                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Leader result = new Leader();
                    result.UserName = reader.GetString(0);
                    result.TotalGames = reader.GetInt32(1);
                    result.TotalScore = reader.GetInt32(2);
                    result.MaxScore = reader.GetInt32(3);

                    resultlist.Add(result);
                }

                if (resultlist.Count > 0)
                {
                    return resultlist;
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

        //method for getting values for replay
        public List<Results>? GetUserData(string username)
        {
            List<Results> resultlist = new List<Results>();

            //query for inserting value
            string SelectQuery = $"SELECT * FROM results Where username='{username}' ORDER BY played_at DESC LIMIT 10";
            NpgsqlCommand command = new NpgsqlCommand(SelectQuery, connection);

            try
            {
                connection.Open(); //opening a new connection
                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Results result = new Results();
                    result.Attempt = reader.GetInt32(2);
                    result.Word = reader.GetString(3);
                    result.Score = reader.GetInt32(4);
                    result.PlayedAt = reader.GetDateTime(5);

                    resultlist.Add(result);
                }

                if (resultlist.Count > 0)
                {
                    return resultlist;
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
    }
}