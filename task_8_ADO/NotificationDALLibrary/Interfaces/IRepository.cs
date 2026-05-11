namespace NotificationDALLibrary.Interface
{
    public interface IRepository<K, T> where T : class
    {
        public T? Create(T item); //inserting into dictionary 

        public T? Delete(K key); //deleting object based on key

        public T? GetData(K key); //getting a particular data based on key

        public List<T>? GetAllData(); //fetching all the details

        public T? Update(K key, T item); //updating the dictionary based on key
    }
}