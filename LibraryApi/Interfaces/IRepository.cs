namespace LibraryApi.Interfaces
{
    public interface IRepository<K, T> where T : class
    {
        T Create(T item); //method for inserting an item

        List<T> GetAll(); //method for getting all items

        T? GetById(K key); //method for getting item by id
    }
}