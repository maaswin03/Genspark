using NotificationSystem.Models;
using NotificationSystem.Interface;

namespace NotificationSystem.Repository
{
    internal class NotificationRepository : IRepository<int, Notification>
    {
        Dictionary<int, Notification> _notificationMap = new Dictionary<int, Notification>(); // creating a new dictionary to store notification and its key

        int LastMessageid = 0; // variable for storing unique message id


        //indexer for creating and fetching based on index
        public Notification this[int index]
        {
            get { return _notificationMap[index]; }
            set { _notificationMap[index] = value; }
        }

        //method for creating a  new notification - send 
        public Notification Create(Notification item)
        {
            LastMessageid++;
            item.MessageId = LastMessageid;
            item.NotificationSent = true;
            _notificationMap.Add(LastMessageid, item);
            return item;
        }

        //method for deleting a notification - unsent
        public Notification? Delete(int key)
        {
            var notification = GetData(key); //checking the message is present
            if (notification != null)
            {
                notification.NotificationSent = false; //updating the sent status as false 
                _notificationMap[key] = notification;
                return notification;
            }
            return null;
        }

        //method for getting data of a particular notification based on message id
        public Notification? GetData(int key)
        {
            if (_notificationMap.ContainsKey(key)) //checking the dictionary contains the message id
            {
                return _notificationMap[key];
            }
            return null;
        }

        //method for getting all the notification data
        public List<Notification>? GetAllData()
        {
            if (_notificationMap.Count > 0) //checking the dictionary is not empty
            {
                var notification = _notificationMap.Values.ToList();
                return notification;
            }
            return null;
        }

        //method for updating existing message;
        public Notification? Update(int key, Notification item)
        {
            var notification = GetData(key); //calling the method to check the notification is present
            if (notification != null)
            {
                _notificationMap[key] = item;
                return notification;
            }
            return null;
        }
    }
}