﻿using System;
using ActiveCruzer.Models;
using ActiveCruzer.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace ActiveCruzer.BLL
{
    public class RequestBll : IDisposable
    {
        public int CreateRequest(Request request)
        {
            return 1;
        }

        public bool Exists(in int id)
        {
            return true;
        }

        public void Delete(in int id)
        {
            

        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        public void UpdateStatus(Request.RequestStatus status)
        {
            throw new NotImplementedException();
        }

        public Request GetRequest(in int id)
        {
            throw new NotImplementedException();
        }
    }
}