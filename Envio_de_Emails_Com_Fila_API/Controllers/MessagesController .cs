using Envio_de_Emails_Com_Fila.Shared.Models;
using Envio_de_Emails_Com_Fila_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Envio_de_Emails_Com_Fila_API.Controllers
{
    [ApiController]
    [Route("messages")]
    public class MessagesController : ControllerBase
    {
        private readonly RabbitMQService _rabbit;

        public MessagesController(RabbitMQService rabbit)
        {
            _rabbit = rabbit;
        }

        [HttpPost]
        public async Task<IActionResult> Send([FromBody] EmailMessage msg)
        {
            await _rabbit.Publish(msg);
            return Ok("Email enviado para fila");
        }
    }
}
