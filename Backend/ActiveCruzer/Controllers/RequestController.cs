﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using ActiveCruzer.BLL;
using ActiveCruzer.DAL.DataContext;
using ActiveCruzer.Models;
using ActiveCruzer.Models.DTO;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ActiveCruzer.Controllers
{
    /// <summary>
    /// Database controller for transactions related to the database
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class RequestController : BaseController 
    {
        private readonly RequestBll _bll;
        private readonly IMapper _mapper;
        private bool _disposed;

        /// <summary>
        /// Inserts a request to the database
        /// </summary>
        /// <returns></returns>
        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<CreateRequestResponseDto> InsertRequest([FromBody] CreateRequestDto req)
        {
            if (ModelState.IsValid)
            {
                var request = _mapper.Map<Request>(req);
                var id = _bll.CreateRequest(request);
                return CreatedAtAction(nameof(GetById), new {id}, new CreateRequestResponseDto{Id = id});
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// Removes a request from the Database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult RemoveRequest([FromRoute] int id)
        {
            if (_bll.Exists(id))
            {
                _bll.Delete(id);
                return Ok();
            }
            else
            {
                return NotFound(id);
            }
            

        }

        /// <summary>
        /// Updates the status of a request
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult PatchRequest([FromRoute] int id, [FromBody] PatchRequestDto patchRequest)
        {
            if (ModelState.IsValid)
            {
                if (_bll.Exists(id))
                {
                    _bll.UpdateStatus(Models.Request.RequestStatus.CLOSED);
                    return Ok();
                }
                else
                {
                    return NotFound(id);
                }
            }
            else
            {
                return BadRequest(ModelState);
            }

        
        }

        /// <summary>
        /// Get request by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<GetRequestResponse> GetById(int id)
        {
            var request = _bll.GetRequest(id);
            
            return Ok(_mapper.Map<GetRequestResponse>(request));
        }

        /// <summary>
        /// Get all requests in a specific area
        /// </summary>
        /// <param name="longitude">Longitude in degrees</param>
        /// <param name="latitude">Latitude in degrees</param>
        /// <param name="amount">How many requests to retrieve</param>
        /// <param name="metersPerimeter">Which perimeter should be kept in considoration</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<GetAllRequestResponse> GetAll([FromQuery] double longitude, 
            [FromQuery] double latitude, [FromQuery] int amount = 10, [FromQuery] int metersPerimeter = 2000)
        {

            var requests = _bll.GetRequestsViaGps(latitude, longitude,amount, metersPerimeter);
            var dtoRequests = requests.Select(it => _mapper.Map<RequestDto>(it)).ToList();

            return Ok(new GetAllRequestResponse {Requests = dtoRequests});
        }

        /// <summary>
        /// IDisposible for connections
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    _bll?.Dispose();
                }
                _disposed = true;
            }

            base.Dispose(disposing);
        }

    }

}