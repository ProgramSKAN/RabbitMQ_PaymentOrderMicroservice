using Microsoft.AspNetCore.Mvc;
using Payments_API.Models;
using Payments_API.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Payments_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QueuePurchaseOrderController : ControllerBase
    {
        [HttpPost]
        public IActionResult MakePayment([FromBody] PurchaseOrder purchaseOrder)
        {
            try
            {
                RabbitMQClient client = new RabbitMQClient();
                client.SendPurchaseOrder(purchaseOrder);
                client.Close();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            return Ok(purchaseOrder);
        }
    }
}
