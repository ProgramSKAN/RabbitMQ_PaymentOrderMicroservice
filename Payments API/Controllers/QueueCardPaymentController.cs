using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payments_API.Models;
using Payments_API.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payments_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueueCardPaymentController : ControllerBase
    {
        [HttpPost]
        public IActionResult MakePayment([FromBody] CardPayment payment)
        {
            try
            {
                RabbitMQClient client = new RabbitMQClient();
                client.SendPayment(payment);
                client.Close();
            }
            catch (Exception ex)
            {
                return BadRequest(new  { message = ex.Message });
            }

            return Ok(payment);
        }
    }
}
