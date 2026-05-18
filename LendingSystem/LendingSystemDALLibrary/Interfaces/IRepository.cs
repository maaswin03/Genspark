namespace LendingSystemDALLibrary.Interfaces
{
    public interface IRepository<K,T> where T : class
    {
        T Create(T item); //method for inserting a data 

        T? Delete(K key); //method for deleting a data 

        T? Update(T item); //method for updating a data based on key

        T? GetById(K key); //method for getting data based on key

        List<T> GetAll(); //method for getting all data 
    }
}