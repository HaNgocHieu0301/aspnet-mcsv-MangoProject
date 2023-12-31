﻿using Mango.MessageBus;
using Mango.Services.AuthAPI.Models.DTO;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ResponseDTO _responseDTO;
        private readonly IConfiguration _configuration;
        private readonly IMessageBus _messageBus;
        public AuthAPIController(IAuthService authService, IConfiguration configuration, IMessageBus messageBus)
        {
            _authService = authService;
            _responseDTO = new();
            _configuration = configuration;
            _messageBus = messageBus;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO registrationRequestDTO)
        {
            var errorMessage = await _authService.Register(registrationRequestDTO);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = errorMessage;
                return BadRequest(_responseDTO);
            }
            string check = _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue");
            await _messageBus.PublishMessagej(registrationRequestDTO.Email, check);
            return Ok(_responseDTO);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var loginResponseDTO = await _authService.Login(loginRequestDTO);
            if (loginResponseDTO.User == null)
            {
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = "Login Failed!\n Username or password is incorrect! ";
                return BadRequest(_responseDTO);
            }
            _responseDTO.Result = loginResponseDTO;
            return Ok(_responseDTO);
        }

        [HttpPost("assignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDTO model)
        {
            var assignRoleSuccessful = await _authService.AssignRole(model.Email, model.Role.ToUpper());
            if (!assignRoleSuccessful)
            {
                _responseDTO.IsSuccess = false;
                _responseDTO.Message = "Error encountered!";
                return BadRequest(_responseDTO);
            }
            return Ok(_responseDTO);
        }
    }
}
