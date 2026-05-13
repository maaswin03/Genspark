using NotificationDALLibrary.Context;
using NotificationDALLibrary.Interface;
using NotificationModelLibrary;
using Npgsql;

namespace NotificationDALLibrary.Repository
{
    public class NotificationRepository : IRepository<int, Notification>
    {
        NotificationSystemContext context;

        //initializing the context 
        public NotificationRepository()
        {
            context = new NotificationSystemContext();
        }

        //method for inserting values into table
        public Notification Create(Notification item)
        {
            context.notifications.Add(item); //adding notification to the table
            context.SaveChanges(); //saving changes in the database
            return item;
        }

        //method for deleting a user
        public Notification? Delete(int key)
        {
            var notification = context.notifications.FirstOrDefault(n => n.MessageId == key); //getting the notification details

            if (notification == null) //check the notification exists 
            {
                return null;
            }

            notification.NotificationSent = false; //set send status false
            context.SaveChanges(); //save changes in table
            return notification;
        }

        //method for getting a particular notification
        public Notification? GetData(int key)
        {
            return context.notifications.FirstOrDefault(n => n.MessageId == key); //check if the notification present in the table 
        }

        //method for getting all the notification
        public List<Notification>? GetAllData()
        {
            return context.notifications.ToList(); //return all the list values
        }

        //method for updating the notification in the table
        public Notification? Update(int key, Notification item)
        {
            var existing_notification = context.notifications.FirstOrDefault(n => n.MessageId == key);

            if (existing_notification == null)
            {
                return null;
            }
            //update message for the notification
            existing_notification.Message = item.Message;
            context.SaveChanges(); //save the changes in database
            return existing_notification;
        }

    }
}