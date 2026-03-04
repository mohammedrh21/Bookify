using AutoMapper;
using Bookify.Application.Common;
using Bookify.Application.DTO.FAQ;
using Bookify.Application.Interfaces;
using Bookify.Application.Interfaces.FAQ;
using Bookify.Domain.Contracts.FAQ;
using Bookify.Domain.Entities;
using Bookify.Domain.Exceptions;

namespace Bookify.Application.Services
{
    public sealed class FAQService : IFAQService
    {
        private readonly IFAQRepository _repo;
        private readonly IMapper _mapper;
        private readonly IAppLogger<FAQService> _logger;

        public FAQService(
            IFAQRepository repo,
            IMapper mapper,
            IAppLogger<FAQService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<IEnumerable<FAQResponse>>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all FAQs");
            var faqs = await _repo.GetAllAsync();
            return ServiceResponse<IEnumerable<FAQResponse>>.Ok(
                data: _mapper.Map<IEnumerable<FAQResponse>>(faqs));
        }

        public async Task<ServiceResponse<FAQResponse>> GetByIdAsync(Guid id)
        {
            _logger.LogInformation($"Fetching FAQ with ID {id}");
            var faqs = await _repo.GetAllAsync(); // For simplicity, though GetByIdAsync on repo is better
            var faq = faqs.SingleOrDefault(x => x.Id == id)
                ?? throw new NotFoundException(nameof(FAQ), id);

            return ServiceResponse<FAQResponse>.Ok(
                data: _mapper.Map<FAQResponse>(faq));
        }

        public async Task<ServiceResponse<Guid>> CreateAsync(CreateFAQRequest request)
        {
            _logger.LogInformation("Creating FAQ");

            var faq = new FAQ
            {
                Id = Guid.NewGuid(),
                Question = request.Question.Trim(),
                Answer = request.Answer.Trim()
            };

            await _repo.AddAsync(faq);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"FAQ created: {faq.Id}");

            return ServiceResponse<Guid>.Ok(
                id: faq.Id,
                data: faq.Id,
                message: "FAQ created successfully.");
        }

        public async Task<ServiceResponse<Guid>> UpdateAsync(UpdateFAQRequest request)
        {
            _logger.LogInformation($"Updating FAQ: {request.Id}");

            var faqs = await _repo.GetAllAsync();
            var faq = faqs.SingleOrDefault(x => x.Id == request.Id)
                ?? throw new NotFoundException(nameof(FAQ), request.Id);

            faq.Question = request.Question.Trim();
            faq.Answer = request.Answer.Trim();

            await _repo.UpdateAsync(faq);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"FAQ updated: {faq.Id}");

            return ServiceResponse<Guid>.Ok(
                id: faq.Id,
                data: faq.Id,
                message: "FAQ updated successfully.");
        }

        public async Task<ServiceResponse<Guid>> DeleteAsync(Guid id)
        {
            _logger.LogInformation($"Deleting FAQ: {id}");

            var faq = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException(nameof(FAQ), id);

            _repo.Delete(faq);
            await _repo.SaveChangesAsync();

            _logger.LogInformation($"FAQ deleted: {faq.Id}");

            return ServiceResponse<Guid>.Ok(
                id: faq.Id,
                data: faq.Id,
                message: "FAQ deleted successfully.");
        }
    }
}
