using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BK.WeChat.Controllers.ApiControllers
{
    public class BaseApiController : ApiController
    {
        // GET api/<controller>
        public virtual IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public virtual string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public virtual void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public virtual void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public virtual void Delete(int id)
        {
        }
    }
}