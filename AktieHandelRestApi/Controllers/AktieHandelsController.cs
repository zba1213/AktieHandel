    using AktiehandelRepoLib;
using AktieHandelRepoLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AktieHandelsRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AktieHandelsController : ControllerBase
    {
        private static readonly AktieHandelRepo _repository =
            new AktieHandelRepo();

        // GET: api/AktieHandel
        [HttpGet]
        public ActionResult<IEnumerable<AktieHandel>> GetAll(
            [FromQuery] string? substring = null,
            [FromQuery] int? antal = null,
            [FromQuery] string antalFilter = "lig",
            [FromQuery] decimal? kurs = null,
            [FromQuery] string? navn = null)
        {
            try
            {
                return Ok(_repository.Search(substring, antal, antalFilter, kurs, navn));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        // GET: api/AktieHandel/1
        [HttpGet("{id}")]
        public ActionResult<AktieHandel> GetById(int id)
        {
            AktieHandel? handel = _repository.GetById(id);

            if (handel == null)
            {
                return NotFound();
            }
            return Ok(handel);
        }

        // POST: api/AktieHandel
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult<AktieHandel> Add(AktieHandel handel)
        {
            try
            {
                AktieHandel nyHandel = _repository.Add(handel);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = nyHandel.HandelsId },
                    nyHandel);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/AktieHandel/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<AktieHandel> Update(
            int id,
            AktieHandel data)
        {
            try
            {
                AktieHandel? updated =
                    _repository.Update(id, data);

                if (updated == null)
                {
                    return NotFound();
                }

                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/AktieHandel/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public ActionResult<AktieHandel> Delete(int id)
        {
            AktieHandel? deleted =
                _repository.Delete(id);

            if (deleted == null)
            {
                return NotFound();
            }

            return Ok(deleted);
        }
    }
}
