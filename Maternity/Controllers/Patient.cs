using BL;
using DTO.Models;
using Microsoft.AspNetCore.Mvc;

namespace Maternity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _service;

        public PatientsController(IPatientService service)
        {
            _service = service;
        }

        /// <summary>
        /// Получает список всех пациентов родильного дома.
        /// </summary>
        /// <param name="birthdate">
        /// Пример: ge2010-01-01
        /// Необязательный фильтр по дате рождения. Можно указать несколько параметров birthdate (например, 
        /// <c>?birthdate=ge2010-01-01&amp;birthdate=le2011-12-31</c>), они объединяются по логическому И.
        /// Поддерживаются операторы: 
        /// <list type="bullet">
        /// <item><c>eq</c> – равно (например, <c>eq2020-05-15</c>)</item>
        /// <item><c>ne</c> – не равно</item>
        /// <item><c>lt</c> – меньше (раньше)</item>
        /// <item><c>gt</c> – больше (позже)</item>
        /// <item><c>le</c> – меньше или равно</item>
        /// <item><c>ge</c> – больше или равно</item>
        /// <item><c>sa</c> – начинается после указанной даты (starts after)</item>
        /// <item><c>eb</c> – заканчивается до указанной даты (ends before)</item>
        /// <item><c>ap</c> – приблизительно (approximately): интервал даты расширяется на ±1 единицу измерения 
        /// (год, месяц, день, минута, секунда, миллисекунда) в зависимости от точности переданного значения, 
        /// и проверяется пересечение с датой рождения пациента.</item>
        /// </list>
        /// Пример с одним параметром: <c>?birthdate=ge2010-01-01</c> возвращает пациентов, родившихся 2010-01-01 или позже.
        /// Пример с двумя параметрами: <c>?birthdate=ge2010-01-01&amp;birthdate=le2011-12-31</c> возвращает пациентов, родившихся в период с 2010-01-01 по 2011-12-31 включительно.
        /// Если параметр опущен, возвращаются все пациенты.
        /// </param>
        /// <returns>Список пациентов</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDTO>>> GetAll([FromQuery] string[]? birthdate = null)
        {
            var list = await _service.GetAllAsync(birthdate);
            return Ok(list);
        }


        /// <summary>
        /// Получает конкретного пациента по его уникальному идентификатору.
        /// </summary>
        /// <param name="id" example="3fa85f64-5717-4562-b3fc-2c963f66afa6">GUID пациента</param>
        /// <returns>Пациент с указанным ID</returns>
        /// <response code="200">Пациент найден</response>
        /// <response code="404">Пациент не найден</response>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PatientDTO>> GetById(Guid id)
        {
            var person = await _service.GetByIdAsync(id);
            if (person == null) return NotFound();
            return Ok(person);
        }

        /// <summary>
        /// Создает новую запись пациента.
        /// </summary>
        /// <remarks>
        /// Пример тела запроса (JSON):
        /// <code>
        /// {
        ///   "id": null,
        ///   "name": {
        ///     "id": null,
        ///     "patientId": null,
        ///     "use": "official",
        ///     "family": "Ivanov",
        ///     "given": ["Ivan", "Ivanovich"]
        ///   },
        ///   "birthDate": "1990-05-21T00:00:00Z",
        ///   "gender": "male",
        ///   "active": "true"
        /// }
        /// </code>
        /// </remarks>
        /// <param name="input">Данные пациента (Id должен быть пустым, новый GUID будет сгенерирован)</param>
        /// <returns>Созданный пациент</returns>
        /// <response code="201">Пациент успешно создан</response>
        /// <response code="400">Неверные входные данные</response>
        [HttpPost]
        public async Task<ActionResult<PatientDTO>> Create([FromBody] PatientDTO input)
        {
            if (input == null) return BadRequest();
            var created = await _service.CreateAsync(input);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }


        /// <summary>
        /// Обновляет существующую запись пациента.
        /// </summary>
        /// <param name="id" example="3fa85f64-5717-4562-b3fc-2c963f66afa6">ID обновляемого пациента</param>
        /// <param name="input">Обновленные данные пациента (ID должен совпадать с ID в маршруте)</param>
        /// <returns>Нет содержимого при успешном обновлении</returns>
        /// <response code="204">Обновление успешно</response>
        /// <response code="400">Несовпадение ID или неверные данные</response>
        /// <response code="404">Пациент не найден</response>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PatientDTO input)
        {
            if (input == null || id != input.Id) return BadRequest();
            var ok = await _service.UpdateAsync(id, input);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Удаляет запись пациента.
        /// </summary>
        /// <param name="id" example="3fa85f64-5717-4562-b3fc-2c963f66afa6">ID удаляемого пациента</param>
        /// <returns>Нет содержимого при успешном удалении</returns>
        /// <response code="204">Удаление успешно</response>
        /// <response code="404">Пациент не найден</response>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var ok = await _service.DeleteAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}