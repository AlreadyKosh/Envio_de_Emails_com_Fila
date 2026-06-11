using Envio_de_Emails_Com_Fila_Shared.Models;
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
        private readonly MongoEmailReadService _mongoEmailReadService;

        public MessagesController(RabbitMQService rabbit, MongoEmailReadService mongoEmailReadService)
        {
            _rabbit = rabbit;
            _mongoEmailReadService = mongoEmailReadService;
        }

        /// <summary>
        /// Lista os emails persistidos no MongoDB.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int limit = 50,
            [FromQuery] string? cep = null,
            CancellationToken cancellationToken = default)
        {
            var emails = await _mongoEmailReadService.GetEmailsAsync(limit, cep, cancellationToken);
            return Ok(emails);
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
