using HasznaltAuto.API.DTOs;
using HasznaltAuto.API.REST.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HasznaltAuto.API.REST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Használt autókon végezhető műveletek")]

    public class HasznaltAutoController(CarRestService carService) : ControllerBase
    {
        // GET: api/<HasznaltAutoController>
        [SwaggerOperation(Summary = "Összes autó lekérése", Description = "Visszaadja az összes elérhető használt autót", OperationId = "GetAllCars", Tags = new[] { "HasznaltAuto" })]
        [ProducesResponseType(typeof(IEnumerable<CarDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IEnumerable<CarDto>> Get(CancellationToken ct)
        {
            return await carService.GetAll(ct);
        }

        // GET api/<HasznaltAutoController>/5
        [SwaggerOperation(Summary = "Autó lekérése azonosító alapján", OperationId = "GetCarById", Tags = new[] { "HasznaltAuto" })]
        [ProducesResponseType(typeof(CarDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<CarDto> Get(int id, CancellationToken ct)
        {
            return await carService.Get(id, ct);
        }

        // POST api/<HasznaltAutoController>
        [SwaggerOperation(Summary = "Autó létrehozása", OperationId = "CreateCar", Tags = new[] { "HasznaltAuto" })]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Consumes("application/json")]
        [HttpPost]
        public async Task<int> Post([FromBody] CarDto carDto)
        {
            throw new NotImplementedException();
        }

        // PUT api/<HasznaltAutoController>/5
        [SwaggerOperation(Summary = "Autó frissítése", OperationId = "UpdateCar", Tags = new[] { "HasznaltAuto" })]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Consumes("application/json")]
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] CarDto carDto)
        {
            throw new NotImplementedException();
        }

        // DELETE api/<HasznaltAutoController>/5
        [SwaggerOperation(Summary = "Autó törlése", OperationId = "DeleteCar", Tags = new[] { "HasznaltAuto" })]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
