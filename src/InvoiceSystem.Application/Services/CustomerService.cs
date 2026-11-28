using InvoiceSystem.Application.DTOs;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Interfaces;

namespace InvoiceSystem.Application.Services;

public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var existing = await _customerRepository.GetByEmailAsync(dto.Email);
        if (existing != null && !existing.IsDeleted)
        {
            throw new InvalidOperationException($"Customer with email '{dto.Email}' already exists.");
        }

        var customer = new Customer
        {
            FullName = dto.FullName,
            CompanyName = dto.CompanyName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _customerRepository.AddAsync(customer);
        return MapToDto(created);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllAsync(string? search = null)
    {
        var customers = await _customerRepository.GetAllAsync(search);
        return customers.Select(MapToDto);
    }

    public async Task<CustomerDto> GetByIdAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null || customer.IsDeleted)
        {
            throw new KeyNotFoundException($"Customer with ID {id} was not found.");
        }
        return MapToDto(customer);
    }

    public async Task<CustomerDto> UpdateAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null || customer.IsDeleted)
        {
            throw new KeyNotFoundException($"Customer with ID {id} was not found.");
        }

        if (customer.Email != dto.Email)
        {
            var existing = await _customerRepository.GetByEmailAsync(dto.Email);
            if (existing != null && existing.Id != id && !existing.IsDeleted)
            {
                throw new InvalidOperationException($"Customer with email '{dto.Email}' already exists.");
            }
        }

        customer.FullName = dto.FullName;
        customer.CompanyName = dto.CompanyName;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        customer.City = dto.City;

        await _customerRepository.UpdateAsync(customer);
        return MapToDto(customer);
    }

    public async Task DeleteAsync(int id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null || customer.IsDeleted)
        {
            throw new KeyNotFoundException($"Customer with ID {id} was not found.");
        }

        var hasInvoices = await _customerRepository.HasInvoicesAsync(id);
        if (hasInvoices)
        {
            throw new InvalidOperationException("Cannot delete customer who has associated invoices.");
        }

        customer.IsDeleted = true;
        await _customerRepository.DeleteAsync(customer);
    }

    private static CustomerDto MapToDto(Customer c) => new(
        c.Id,
        c.FullName,
        c.CompanyName,
        c.Email,
        c.Phone,
        c.Address,
        c.City,
        c.IsDeleted,
        c.CreatedAt
    );
}
