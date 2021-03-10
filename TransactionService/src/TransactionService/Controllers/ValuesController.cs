using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using TransactionService.Models;

namespace TransactionService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        private readonly CurrentUserContext _userContext; 

        public ValuesController(CurrentUserContext userContext)
        {
            _userContext = userContext;
        }
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            Console.WriteLine(_userContext.UserId);
            return new string[] {"value1", "value2"};
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}