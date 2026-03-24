using Bookify.Application.DTO.ContactInfo;
using Bookify.Application.Interfaces.ContactInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Bookify.API.Controllers
{
    [Route("api/contact-info")]
    [Produces("application/json")]
    public class ContactInfoController : BaseController
    {
        private readonly IContactInfoService _infoService;
        public ContactInfoController(IContactInfoService infoService)
        {
            _infoService = infoService;
        }

        /// <summary>Get the only one contact info.</summary>
        /// <response code="200">Contact Info found.</response>
        /// <response code="404">Contact Info not found.</response>
        [HttpGet()]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetInfoAsync()
        {
            var result = await _infoService.GetAsync();
            return HandleResult(result);
        }

        /// <summary>Create a new Contact info (Admin only).</summary>
        /// <response code="201">Contact info created.</response>
        /// <response code="400">Validation error.</response>
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateContactInfoRequest request)
        {
            var result = await _infoService.CreateAsync(request);
            if (result.Success)
            {
                return CreatedAtAction(
                    nameof(GetInfoAsync),
                    new { id = result.Data },
                    result);
            }
            return HandleResult(result);
        }

        /// <summary>Update an existing Contact info (Admin only).</summary>
        /// <response code="200">Contact info updated.</response>
        /// <response code="400">Validation error.</response>
        /// <response code="404">Contact info not found.</response>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactInfoRequest request)
        {
            request.Id = id;
            var result = await _infoService.UpdateAsync(request);
            return HandleResult(result);
        }
    }
}
