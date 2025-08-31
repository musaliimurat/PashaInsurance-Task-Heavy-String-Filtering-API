using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PashaInsuranceFiltering.Application.Features.CQRS.Commands.UploadCommands;
using PashaInsuranceFiltering.Application.Features.CQRS.Queries.UploadQueries;
using PashaInsuranceFiltering.Application.Features.CQRS.Results.UploadResults;

namespace PashaInsuranceFiltering.WebAPI.Controllers
{
    [Route("api/")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UploadsController(IMediator mediator) => _mediator = mediator;


        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromBody] UploadChunkCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct); 
            if (!result.Success)
            {
                return BadRequest(new { status = "Error", message = result.Message });
            }
            return StatusCode(StatusCodes.Status202Accepted, new { status = "Accepted" });
        }


        [HttpGet("result/{uploadId:guid}")]
        public async Task<IActionResult> GetResult([FromRoute] Guid uploadId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetUploadResultQuery(uploadId), ct);

            if (result.Success && result.Data is GetUploadResultQueryResult dto && !string.IsNullOrWhiteSpace(dto.Data))
            {
                return Ok(new
                {
                    uploadId,
                    status = "Completed",
                    data = dto.Data
                });
            }

            if (!result.Success && result.Message.Contains("process", StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(StatusCodes.Status202Accepted, new
                {
                    uploadId,
                    status = "Processing"
                });
            }

            if (!result.Success && result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new
                {
                    uploadId,
                    status = "NotFound"
                });
            }

            return BadRequest(new
            {
                uploadId,
                status = "Error",
                message = result.Message
            });
        }
    }
}
