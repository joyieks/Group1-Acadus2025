using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public LoginResult AuthenticateUser(string email, string password, ref User user)
        {
            // Supabase handles password authentication
            // This method should only retrieve the user after Supabase auth succeeds
            user = new User();
            user = _repository.GetUsers().Where(x => x.email == email).FirstOrDefault();

            return user != null ? LoginResult.Success : LoginResult.Failed;
        }

        public void AddUser(UserViewModel model)
        {
            var user = new User();
            if (!_repository.UserExists(model.UserId))
            {
                _mapper.Map(model, user);
                // New schema doesn't include Password field or audit fields
                // Password handling should be moved to a dedicated password/auth service
                _repository.AddUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }
    }
}
