﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Common.WebApi.Handlers
{
    public class InjectAcceptHandler : DelegatingHandler
    {
        readonly MediaTypeWithQualityHeaderValue accept;

        public bool ClearRequestedAccept { get; set; }

        public InjectAcceptHandler(string accept)
        {
            if (!MediaTypeWithQualityHeaderValue.TryParse(accept, out this.accept))
                throw new ArgumentException("invalid contentType in InjectAcceptHandler constructor");
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var acceptHeader = request.Headers.Accept;

            if (acceptHeader != null)
            {
                if (ClearRequestedAccept)
                    acceptHeader.Clear();

                //if (!acceptHeader.Any())
                acceptHeader.Add(accept);
            }

            return base.SendAsync(request, cancellationToken);              
        }
    }
}
