using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Data.Interfaces;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data.Repositories
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetPhoto(Guid id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<User> GetUser(Guid id)
        {
            var user = await _context.Users.Include(u => u.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<Photo> GetUserMainPhoto(Guid userId)
        {
            var mainPhoto = await _context.Photos.Where(p => p.UserId == userId)
                .FirstOrDefaultAsync(p => p.IsMain);

            return mainPhoto;
        }

        public async Task<PagedList<User>> GetUsers(UserParam userParam)
        {
            var users = _context.Users.Include(u => u.Photos).OrderByDescending(u => u.LastActive)
                .Where(u => u.Id != userParam.UserId && u.Gender.ToLower().Equals(userParam.Gender));

            if(userParam.MinAge != 18 || userParam.MaxAge != 99)
            {
                var minDayOfBirth = DateTime.Today.AddYears(-userParam.MaxAge - 1);
                var maxDayOfBirth = DateTime.Today.AddYears(-userParam.MinAge);

                users = users.Where(u => u.DateOfBirth >= minDayOfBirth && u.DateOfBirth <= maxDayOfBirth);

            }

            if(!string.IsNullOrEmpty(userParam.OrderBy))
            {
                switch (userParam.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParam.PageNumber, userParam.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}