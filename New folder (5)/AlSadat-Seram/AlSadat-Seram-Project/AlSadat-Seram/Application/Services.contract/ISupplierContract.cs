using AlSadatSeram.Services.contract;
using Application.DTOs;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.DTOs.SupplierDtos;

namespace Application.Services.contract
{
    public interface ISupplierContract
    {
        Task<ApiResponse<List<SupplierDto>>> GetAllSuppliers(SupplierFilteration req);
        Task<Result<string>> AddNewSupllier(SupplierDto dto);
        Task<Result<string>> EditSupplier(SupplierDto dto);
        Task<Result<SupplierDto>> GetById(int id);

    }
}
