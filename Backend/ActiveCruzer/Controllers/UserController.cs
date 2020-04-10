﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ActiveCruzer.BLL;
using ActiveCruzer.DAL.DataContext;
using ActiveCruzer.Models;
using ActiveCruzer.Models.DTO;
using ActiveCruzer.Models.Geo;
using ActiveCruzer.Startup;
using AutoMapper;
using GeoCoordinatePortable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Org.BouncyCastle.Crypto.Tls;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace ActiveCruzer.Controllers
{
    /// <summary>
    /// Controller to check if User is logged in
    /// </summary>
    [Route("api/User")]
    [ApiController]
    public class UserController : BaseController
    {
        private bool disposed = false;

        private UserBLL _userBll;
        private IGeoCodeBll _geoCodeBll;

        private readonly IOptions<Jwt.JwtAuthentication> _jwtAuthentication;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="logger"></param>
        public UserController(IMapper mapper, IOptions<Jwt.JwtAuthentication> jwtAuthentication,
            IConfiguration configuration, UserBLL userBll)
        {
            _geoCodeBll = new GeoCodeBll(mapper, configuration);
            _jwtAuthentication = jwtAuthentication;
            _userBll = userBll;
        }

        /// <summary>
        /// Indicates whether or not a User is logged in
        /// </summary>
        /// <remarks>When the code reachis this function the user was successfully logged in through Windows authentication.</remarks>
        /// <returns>Object with RealName and UserName</returns>
        /// <response code="200"></response>
        /// <response code="401"></response>
        [Authorize()]
        [HttpGet]
        [Route("LoggedIn")]
        public ActionResult LoggedIn()
        {
            //If the execution reaches this code the user is logged in.
            return Ok();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<JwtDto>> Register([FromBody] RegisterUserDTO credentials)
        {
            if (ModelState.IsValid)
            {
                var validatedAddress = _geoCodeBll.ValidateAddress(new GeoQuery
                {
                    City = credentials.City, Country = credentials.Country, Street = credentials.Street,
                    Zip = credentials.Zip
                });

                if (validatedAddress.ConfidenceLevel == ConfidenceLevel.High)
                {
                    var result = await _userBll.Register(credentials, validatedAddress.Coordinates);
                    if (result.Succeeded)
                    {
                        User user = await _userBll.GetUser(credentials.Email);
                        var token = GenerateToken(user.UserName, user.Id, null);
                        return Ok(new JwtDto
                        {
                            Token = new JwtSecurityTokenHandler().WriteToken(token),
                            ValidUntil = token.ValidTo.ToUniversalTime()
                        });
                    }
                    else
                    {
                        return BadRequest(result.Errors);
                    }
                }
                else
                {
                    return new ContentResult
                    {
                        StatusCode = 424,
                        Content = $"Status Code: {424}; FailedDependency; Address is invalid",
                        ContentType = "text/plain",
                    };
                }
                
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<JwtDto>> Login([FromBody] CredentialsDTO credentials)
        {
            User user = await _userBll.Login(credentials); 

            if (user != null)
            {
                var token = GenerateToken(user.UserName, user.Id, credentials.MinutesValid);
                return Ok(new JwtDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    ValidUntil = token.ValidTo.ToUniversalTime()
                });
            }

            return Unauthorized();
        }

        /// <summary>
        /// delete current loggedin user account. If code 200 returns, user and related references were succesful deleted. If 401 returns, user is not logged in.
        /// </summary>
        /// <returns></returns>
        /// <response code="200"></response>
        /// <response code="401"></response>
        [Authorize]
        [HttpDelete]
        [Route("Delete")]
        public async Task<ActionResult> DeleteUser()
        {
            var user = await _userBll.GetUserViaId(GetUserId());
            if(user != null)
            {
                var result = await _userBll.DeleteUser(user);
                return Ok(result);
            }
            return Unauthorized("You are not allowed to perform this action.");
        }

        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        /// <response code="200">User was updated</response>
        /// <response code="401">The logged in user is not updated user</response>
        [Authorize]
        [HttpPut]
        [Route("Update")]
        public ActionResult UpdateUser([FromBody] UpdateUserDto user)
        {
                var _user = _userBll.UpdateUser(user, GetUserId());
                if (_user != null)
                {
                    return Ok(_user);
                }
                return Unauthorized("You are not allowed to perform this action.");
        }

        /// <summary>
        /// get user information from logged in user. If 200 returns all went well, otherwise 401 will be retured cause no user is logged in
        /// </summary>
        /// <returns></returns>
        /// <response code="200"></response>
        /// <response code="401"></response>
        [Authorize]
        [HttpGet]
        [Route("GetUser")]
        public ActionResult GetUser()
        {
            var user = _userBll.GetUserViaId(GetUserId());
            if(user != null)
            {
                return Ok(user);
            }
            return Unauthorized("You are not allowed to perform this action.");
        }


        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!disposed)
                {
                    if (_userBll != null)
                    {
                        _userBll.Dispose();
                    }
                }

                disposed = true;
            }

            base.Dispose(disposing);
        }


        private JwtSecurityToken GenerateToken(string username, string id, int? credentialsMinutesValid)
        {
            return new JwtSecurityToken(
                audience: _jwtAuthentication.Value.ValidAudience,
                issuer: _jwtAuthentication.Value.ValidIssuer,
                claims: new[]
                {
                    new Claim("id", id),
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                },
                expires: DateTime.UtcNow.AddMinutes(credentialsMinutesValid??1),
                notBefore: DateTime.UtcNow,
                signingCredentials: _jwtAuthentication.Value.SigningCredentials);
        }
    }
}