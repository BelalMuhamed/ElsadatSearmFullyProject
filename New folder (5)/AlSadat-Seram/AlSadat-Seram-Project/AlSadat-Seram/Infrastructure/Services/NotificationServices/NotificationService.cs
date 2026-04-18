using Application.CommonPagination;
using Application.CommonPagination.Pagination;
using Application.DTOs.Notification;
using Application.Services.contract.CurrentUserService;
using Application.Services.contract.NotificationService;
using AutoMapper;
using Domain.Common;
using Domain.Entities.Users;
using Domain.UnitOfWork.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.NotificationServices;
public class NotificationService(
    IUnitOfWork _unitOfWork,
    IMapper _mapper,
    ICurrentUserService _currentUserService
    )
    :INotificationService
{
    //public async Task<Notification?> CreateBudgetNotificationAsync(Transaction transaction)
    //{
    //    if(transaction.Type != TransactionType.Expense)
    //    {
    //        return null;
    //    }

    //    var userId = transaction.UserId;
    //    var categoryId = transaction.CategoryId;
    //    var transactionMonth = transaction.TransactionDate.Month;
    //    var transactionYear = transaction.TransactionDate.Year;

    //    var budget = await _unitOfWork.Repository<Budget>().FindAsync(b =>
    //        b.UserId == userId && b.CategoryId == categoryId &&
    //        b.Month == transactionMonth && b.Year == transactionYear && !b.IsDeleted);

    //    if(budget is null)
    //    {
    //        return null;
    //    }

    //    var totalSpending = await _unitOfWork.Repository<Transaction>()
    //        .GetQueryable()
    //        .Where(t => t.UserId == userId && t.CategoryId == categoryId &&
    //                    t.TransactionDate.Month == transactionMonth && t.TransactionDate.Year == transactionYear &&
    //                    t.Type == TransactionType.Expense && !t.IsDeleted)
    //        .SumAsync(t => t.Amount);

    //    string? notificationMessage = null;
    //    string? notificationType = null;
    //    decimal warningThreshold = budget.Amount * 0.9m;

    //    var hasExceededNotification = await _unitOfWork.Repository<Notification>().AnyAsync(n => n.RelatedEntityId == budget.Id && n.Type == "BudgetExceeded");
    //    var hasWarningNotification = await _unitOfWork.Repository<Notification>().AnyAsync(n => n.RelatedEntityId == budget.Id && n.Type == "BudgetWarning");

    //    if(totalSpending >= budget.Amount && !hasExceededNotification)
    //    {
    //        notificationMessage = $"لقد تجاوزت ميزانيتك! أنفقت {totalSpending:N2} من أصل {budget.Amount:N2} لفئة '{transaction.Category?.Name}'.";
    //        notificationType = "BudgetExceeded";
    //    }
    //    else if(totalSpending >= warningThreshold && !hasExceededNotification && !hasWarningNotification)
    //    {
    //        notificationMessage = $"تنبيه: أنت على وشك تجاوز ميزانيتك. أنفقت {totalSpending:N2} من أصل {budget.Amount:N2} لفئة '{transaction.Category?.Name}'.";
    //        notificationType = "BudgetWarning";
    //    }

    //    if(notificationMessage != null)
    //    {
    //        var notification = new Notification
    //        {
    //            UserId = userId,
    //            Message = notificationMessage,
    //            Type = notificationType!,
    //            RelatedEntityId = budget.Id,
    //            CreatedAt = DateTime.UtcNow,
    //            IsRead = false
    //        };

    //        await _unitOfWork.Repository<Notification>().AddAsync(notification);
    //        return notification;
    //    }

    //    return null;
    //}

    //public async Task<PagedList<NotificationDto>> GetNotificationsAsync(PaginationParams paginationParams)
    //{
    //    var userId = _currentUserService.UserId;
    //    if(userId is null)
    //        return new PagedList<NotificationDto>(new List<NotificationDto>(),0,paginationParams.PageNumber,paginationParams.PageSize);

    //    var notificationsQuery = _unitOfWork.Repository<Notification>()
    //        .GetQueryable()
    //        .Where(n => n.UserId == userId.Value);

    //    var pagedResult = await notificationsQuery
    //        .OrderByDescending(n => n.CreatedAt)
    //        .ProjectTo<NotificationDto>(_mapper.ConfigurationProvider)
    //        .ToPagedListAsync(paginationParams.PageNumber,paginationParams.PageSize);

    //    return pagedResult;
    //}

    //public async Task<Result<bool>> MarkAsReadAsync(int notificationId)
    //{
    //    var userId = _currentUserService.UserId;
    //    if(userId is null)
    //        return Result<bool>.Failure("User not authenticated.",HttpStatusCode.Unauthorized);

    //    var notification = await _unitOfWork.Repository<Notification>()
    //        .FindAsync(n => n.Id == notificationId && n.UserId == userId.Value);

    //    if(notification is null)
    //        return Result<bool>.Failure("Notification not found.",HttpStatusCode.NotFound);

    //    if(!notification.IsRead)
    //    {
    //        notification.IsRead = true;
    //        await _unitOfWork.Repository<Notification>().UpdateAsync(notification);
    //    }

    //    return Result<bool>.Success(true,"Notification marked as read.");
    //}

    //public async Task<Result<bool>> MarkAllAsReadAsync()
    //{
    //    var userId = _currentUserService.UserId;
    //    if(userId is null)
    //        return Result<bool>.Failure("User not authenticated.",HttpStatusCode.Unauthorized);

    //    var unreadNotifications = await _unitOfWork.Repository<Notification>()
    //        .GetAsync(n => n.UserId == userId.Value && !n.IsRead);

    //    if(!unreadNotifications.Any())
    //    {
    //        return Result<bool>.Success(true,"No unread notifications to mark.");
    //    }

    //    foreach(var notification in unreadNotifications)
    //    {
    //        notification.IsRead = true;
    //        await _unitOfWork.Repository<Notification>().UpdateAsync(notification);
    //    }

    //    return Result<bool>.Success(true,"All notifications marked as read.");
    //}
}

