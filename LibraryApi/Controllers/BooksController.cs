using LibraryApi.Interfaces;
using LibraryApi.Misc;
using LibraryApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        IBookService _service;

        public BooksController(IBookService service)
        {
            _service = service;
        }

        //method for getting all data
        [HttpGet]
        public ActionResult<IEnumerable<Book>> GetAllBook()
        {
            try
            {
                var result = _service.GetAllBooks(); //using book service getting the book values

                if (result.Count == 0) //check any value is present 
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //method for getting data by id
        [HttpGet("{id}")]
        public ActionResult<Book> GetBookById(int id)
        {
            try
            {
                var result = _service.GetBookById(id); //getting values using service

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //method for inserting into database
        [HttpPost]
        public ActionResult<Book> CreateBook(Book book)
        {
            try
            {
                var result = new { message = "BOOK ADDED SUCCESSFULLY!" };
                _service.CreateBook(book);
                return Created("", result);
            }
            catch (InvalidInputException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //method for getting book by tittle
        [HttpGet("search/{title}")]
        public ActionResult<Book> GetBookByTitle(string title)
        {
            try
            {
                var result = _service.GetBookByTitle(title); //getting book by title using service

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}