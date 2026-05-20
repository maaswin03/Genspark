namespace LibraryApi.Models
{
    public class Book
    {
        public int BookId { get; set; } //variable for storing book id

        public string Title { get; set; } = string.Empty; //variable for storing book title

        public string Author { get; set; } = string.Empty; //variable for storing author details

        public string Isbn { get; set; } = string.Empty; //variable for storing Isbn details

        public int PublishedYear { get; set; } //variable for storing published year

        public int AvailableCopies { get; set; } //variable for storing available copies
    }
}