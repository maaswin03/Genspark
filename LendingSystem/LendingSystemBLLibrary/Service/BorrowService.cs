using LendingSystemBLLibrary.Interface;
using LendingSystemDALLibrary.Contexts;
using LendingSystemDALLibrary.Interfaces;
using LendingSystemDALLibrary.Repository;
using LendingSystemModelLibrary.Exceptions;
using LendingSystemModelLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace LendingSystemBLLibrary.Service
{
    public class BorrowingService : IBorrowService
    {
        BooklendingsystemContext _context;

        public BorrowingService()
        {
            _context = new BooklendingsystemContext();
        }

        //method for creating borrow transaction
        public bool BorrowBook(int memberid, int bookcopyid)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.Database.ExecuteSqlInterpolated($"CALL borrow_book({memberid},{bookcopyid})");
                transaction.Commit();
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                Console.WriteLine($"\n{e.Message}");
                return false;
            }
        }

        //method for creating return transaction
        public bool ReturnBook(int borrowingid, string status)
        {
            if (!ValidateStatus(status))
            {
                return false;
            }

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                _context.Database.ExecuteSqlInterpolated($"CALL return_book({borrowingid},{status})");
                transaction.Commit();
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                Console.WriteLine($"{e.Message}");
                return false;
            }
        }

        //method for validating the status
        public bool ValidateStatus(string status)
        {
            if (status != "NORMAL" && status != "LOST" && status != "DAMAGED")
            {
                return false;
            }
            return true;
        }
    }
}