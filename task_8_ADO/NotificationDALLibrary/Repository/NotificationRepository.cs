using NotificationDALLibrary.Interface;
using NotificationModelLibrary;
using Npgsql;

namespace NotificationDALLibrary.Repository
{
    public class NotificationRepository : IRepository<int, Notification>
    {
        //hardcoded values are not pushed into git need to use .env instead
        //so removed after testing
        string ConnectionUrl = "";

        NpgsqlConnection connection;

        public NotificationRepository()
        {
            connection = new NpgsqlConnection(ConnectionUrl);
        }

        //Need to use parameterized query to avoid sql injection
        //method for inserting values into table
        public Notification? Create(Notification item)
        {
            //sql query for inserting into the table
            string InsertQuery = $"INSERT INTO notifications(message , sendedat , notificationtype , notificationsent , receiverid) VALUES('{item.Message}','{item.SendedAt.ToString("yyyy-MM-dd HH:mm:ss")}','{item.NotificationType}','{item.NotificationSent}','{item.ReceiverId}')";
            NpgsqlCommand command = new NpgsqlCommand(InsertQuery, connection);
            try
            {
                connection.Open(); //opening connection 
                int result = command.ExecuteNonQuery(); //executing the query
                if (result > 0) //check if the affected rows is greater than 0
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
                connection?.Close(); //closing the connection
            }
        }

        //method for deleting a user
        public Notification? Delete(int key)
        {
            var notification = GetData(key); //check the notification is present

            if (notification != null)
            {
                //delete query
                string UpdateCommand = $"UPDATE notifications SET notificationsent={false} WHERE messageid={key}";
                NpgsqlCommand command = new NpgsqlCommand(UpdateCommand, connection);

                try
                {
                    connection.Open(); //opening a connection
                    int result = command.ExecuteNonQuery();//executing the query
                    if (result > 0) //check the affected rows
                    {
                        return notification;
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
            return null;
        }

        //method for getting a particular notification
        public Notification? GetData(int key)
        {
            //query for getting the notification
            string SelectQuery = $"SELECT n.*, u.name FROM notifications n JOIN users u ON n.receiverid = u.userid WHERE n.messageid = {key}";
            NpgsqlCommand command = new NpgsqlCommand(SelectQuery, connection);

            try
            {
                connection.Open(); //opening a connection
                NpgsqlDataReader reader = command.ExecuteReader(); //executing a query

                while (reader.Read())
                {
                    //creating a new object for notification
                    Notification notification = new Notification();

                    //assigning values
                    notification.MessageId = reader.GetInt32(0);
                    notification.Message = reader.GetString(1);
                    notification.SendedAt = reader.GetDateTime(2);
                    notification.NotificationType = Enum.Parse<NotType>(reader.GetString(3));
                    notification.NotificationSent = reader.GetBoolean(4);
                    notification.ReceiverId = reader.GetInt32(5);
                    notification.UserName = reader.GetString(6);

                    return notification;
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
                connection?.Close(); //closing connection
            }
        }

        //method for getting all the notification
        public List<Notification>? GetAllData()
        {
            //list to store all the notifications
            List<Notification> notificationlist = new List<Notification>();

            string SelectQuery = "SELECT n.*, u.name FROM notifications n JOIN users u ON n.receiverid = u.userid";
            NpgsqlCommand command = new NpgsqlCommand(SelectQuery, connection);

            try
            {
                connection.Open(); //opening a connection
                NpgsqlDataReader reader = command.ExecuteReader(); //executing a query

                while (reader.Read())
                {
                    Notification notification = new Notification();
                    notification.MessageId = reader.GetInt32(0);
                    notification.Message = reader.GetString(1);
                    notification.SendedAt = reader.GetDateTime(2);
                    notification.NotificationType = Enum.Parse<NotType>(reader.GetString(3));
                    notification.NotificationSent = reader.GetBoolean(4);
                    notification.ReceiverId = reader.GetInt32(5);
                    notification.UserName = reader.GetString(6);

                    notificationlist.Add(notification);
                }

                return notificationlist;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            finally
            {
                connection?.Close();
            }

        }

        //method for updating the notification in the table
        public Notification? Update(int key, Notification item)
        {
            //query for updating row 
            string UpdateQuery = $"UPDATE notifications SET message='{item.Message}' WHERE messageid={key}";
            NpgsqlCommand command = new NpgsqlCommand(UpdateQuery, connection);

            try
            {
                connection.Open(); //opening the connection
                int result = command.ExecuteNonQuery(); //executing the query
                if (result > 0)
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
                connection?.Close(); //closing the connection
            }
        }

    }
}