using Envio_de_Emails_Com_Fila.Shared.Models;
using Envio_de_Emails_Com_Fila_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Envio_de_Emails_Com_Fila_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Tags("Rabbit")]
    public class MessagesController : ControllerBase
    {
        private readonly RabbitMQService _rabbit;

        public MessagesController(RabbitMQService rabbit)
        {
            _rabbit = rabbit;
        }

        /// <summary>
        /// Publica uma mensagem de email na fila do RabbitMQ para processamento assíncrono.
        /// </summary>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] EmailMessage msg)
        {
            if (msg == null)
                return BadRequest();

            await _rabbit.Publish(msg);
            return Ok("Email enviado para fila");
        }
    }
}
