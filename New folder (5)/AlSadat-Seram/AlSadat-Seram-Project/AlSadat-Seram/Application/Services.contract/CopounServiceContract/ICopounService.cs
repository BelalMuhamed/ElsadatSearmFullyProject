using AlSadatSeram.Services.contract;
using Application.DTOs.CopounDtos;
using Domain.Entities.copounModel;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.contract.CopounServiceContract
{
    public interface ICopounService
    {
        Task<IReadOnlyList<CopounRespDto>> GetAllCopouns();
        Task AddNewCopoun(CopounReqDto req);
        Task<CopounRespDto?> GetByID(string Id);
        Task EditCopoun(CopounRespDto req);
        Task<IReadOnlyList<Copoun>> CheckExist(string Id);

        Task UpdateAllCopounPoints(int newPoints);
    }
}
