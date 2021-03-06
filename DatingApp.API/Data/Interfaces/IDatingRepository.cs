using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;

namespace DatingApp.API.Data.Interfaces
{
    public interface IDatingRepository
    {
         void Add<T>(T entity) where T: class;
         void Delete<T>(T entity) where T: class;
         Task<bool> SaveAll();
         Task<PagedList<User>> GetUsers(UserParam userParam);
         Task<User> GetUser(Guid id);
        Task<Photo> GetPhoto(Guid id);
        Task<Photo> GetUserMainPhoto(Guid userId);
        Task<Like> GetLike(Guid userId, Guid recipientId);
        Task<Message> GetMessage(Guid id);
        Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams);
        Task<IEnumerable<Message>> GetMessageThread(Guid userId, Guid recipientId);
    }
}