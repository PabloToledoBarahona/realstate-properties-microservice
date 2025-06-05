using Cassandra;
using Microsoft.AspNetCore.Mvc;
using PropertiesService.Services;

namespace PropertiesService.Controllers
{
    [ApiController]
    [Route("api/test-cassandra")]
    public class TestCassandraController : ControllerBase
    {
        private readonly CassandraSessionFactory _factory;

        public TestCassandraController(CassandraSessionFactory factory)
        {
            _factory = factory;
        }

        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var session = _factory.GetSession();
                var rs = await session.ExecuteAsync(new SimpleStatement("SELECT release_version FROM system.local"));
                var version = rs.FirstOrDefault()?["release_version"]?.ToString();
                return Ok(new { CassandraVersion = version });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}