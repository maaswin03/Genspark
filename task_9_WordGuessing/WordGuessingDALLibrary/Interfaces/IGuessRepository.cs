namespace WordGuessingDALLibrary.Interfaces
{
    //interface for repository
    interface IRepository<K,T> where T : class
    {
        public T? Create(T item); //method for inserting into list

        public T? GetData(K key); //method for getting particular guess

        public List<T>? DeleteAllData(); //method for deleting all the values in list
    }
}