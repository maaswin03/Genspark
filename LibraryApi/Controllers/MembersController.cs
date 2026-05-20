using LibraryApi.Interfaces;
using LibraryApi.Misc;
using LibraryApi.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        IMemberService _service;

        public MembersController(IMemberService service)
        {
            _service = service;
        }

        //method for getting all the member details
        [HttpGet]
        public ActionResult<IEnumerable<Member>> GetAllMembers()
        {
            try
            {
                var result = _service.GetAllMembers(); //service used for fetching all the member details

                if (result.Count == 0)
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

        //method for getting member by id
        [HttpGet("{id}")]
        public ActionResult<Member> GetMemberById(int id)
        {
            try
            {
                var result = _service.GetMemberById(id); //service used for fetching member by id

                if (result == null) //checking is the result is empty
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

        //method for creating a new member
        [HttpPost]
        public ActionResult<Member> CreateMember(Member member)
        {
            try
            {
                var result = new { message = "MEMBER CREATED SUCCESSFULLY" };
                _service.CreateMember(member);
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
    }
}