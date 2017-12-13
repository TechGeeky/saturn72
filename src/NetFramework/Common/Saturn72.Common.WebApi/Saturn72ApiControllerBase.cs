﻿#region

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Saturn72.Common.WebApi.Models;
using Saturn72.Common.WebApi.MultistreamProviders;
using Saturn72.Common.WebApi.Utils;
using Saturn72.Core.Services.File;
using Saturn72.Extensions;

#endregion

namespace Saturn72.Common.WebApi
{
    public abstract class Saturn72ApiControllerBase : ApiController
    {
        protected NameValueCollection FormData { get; private set; }

        protected ClaimsIdentity Identity
        {
            get { return User.Identity as ClaimsIdentity; }
        }

        protected IHttpActionResult BadRequestResult(object data)
        {
            return BadRequest(JsonUtil.Serialize(data));
        }

        protected Task<IHttpActionResult> ValidateModelStateAndRunActionAsync(Action action)
        {
            return ValidateModelStateAndRunActionAsync(() => ModelState.IsValid, action);
        }

        protected Task<IHttpActionResult> ValidateModelStateAndRunActionAsync(Func<bool> validationFunc,
            Action action)
        {
            Exception exception = null;
            return Task.Run<IHttpActionResult>(() =>
            {
                if (!validationFunc())
                    return BadRequest(ConvertModelStateErrorsToKeyValuePair());

                try
                {
                    action();
                    return Ok();
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return BadRequest();
                }
                finally
                {
                    if (exception.NotNull())
                        throw exception;
                }
            });
        }

        protected Task<IHttpActionResult> ValidateModelStateAndRunActionAsync<TResult>(Func<TResult> func)
        {
            return ValidateModelStateAndRunActionAsync(() => ModelState.IsValid, func);
        }

        protected Task<IHttpActionResult> ValidateModelStateAndRunActionAsync<TResult>(Func<bool> validationFunc,
            Func<TResult> func)
        {
            Exception exception = null;
            return Task.Run<IHttpActionResult>(() =>
            {
                if (!validationFunc())
                    return BadRequest(ConvertModelStateErrorsToKeyValuePair());

                try
                {
                    return Ok(func());
                }
                catch (Exception ex)
                {
                    exception = ex;
                    return BadRequest();
                }
                finally
                {
                    if (exception.NotNull())
                        throw exception;
                }
            });
        }

        protected virtual string ConvertModelStateErrorsToKeyValuePair()
        {
            var modelStateErrorsList = ModelState.Select(x =>
                string.Format("{0} : {1}", x.Key, string.Join("\n\t", x.Value.Errors.Select(e => e.ErrorMessage))))
                .ToArray();

            return string.Join("\n", modelStateErrorsList);
        }

        protected IEnumerable<Claim> GetClaims()
        {
            return Identity.Claims;
        }

        protected Claim GetIdClaim()
        {
            return Identity.FindFirst(ClaimTypes.NameIdentifier);
        }

        protected virtual async Task<bool> TryExtractDomainModelFromMultipartRequestAsync<TApiModel>
            (ICollection<FileUploadRequest> attachtments, TApiModel result)
            where TApiModel : ApiModelBase, new()
        {
            try
            {
                result = await ExtractDomainModelFromMultipartRequestAsync<TApiModel>(attachtments);
                return true;
            }
            catch (HttpResponseException )
            {
                return false;
            }
        }

        protected virtual async Task<TApiModel> ExtractDomainModelFromMultipartRequestAsync<TApiModel>
            (ICollection<FileUploadRequest> attachtments)
            where TApiModel : ApiModelBase, new()
        {
            return await ExtractDomainModelFromMultipartRequestAsync<TApiModel>(Request, attachtments);
        }

        protected virtual async Task<TApiModel> ExtractDomainModelFromMultipartRequestAsync<TApiModel>
            (HttpRequestMessage request, ICollection<FileUploadRequest> attachtments)
            where TApiModel : ApiModelBase, new()
        {
            TApiModel model = null;

            return await ExtractDomainModelFromMultipartRequestAsync(request, model, attachtments);
        }

        protected virtual async Task<TApiModel> ExtractDomainModelFromMultipartRequestAsync<TApiModel>
            (HttpRequestMessage request, TApiModel model, ICollection<FileUploadRequest> attachtments)
            where TApiModel : ApiModelBase, new()
        {
            Guard.NotNull(request);

            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotAcceptable,
                    "This request does not contain multipart data"));
            }

            var streamProvider = new InMemoryMultipartFormDataStreamProvider();
            await request.Content.ReadAsMultipartAsync(streamProvider);
            FormData = streamProvider.FormData;

            var allHttpContentTypes = HttpContentType.AllHttpContentTypes;

            foreach (var httpContent in streamProvider.Contents)
            {
                var ct = allHttpContentTypes.FirstOrDefault(c => c.Match(httpContent));

                if (ct == HttpContentType.Model)
                {
                    var stream = await httpContent.ReadAsStreamAsync();
                    model = JsonUtil.Deserialize<TApiModel>(stream);
                    continue;
                }

                if (ct == HttpContentType.File)
                {
                    var getBytesTask = httpContent.ReadAsStreamAsync().Result.ToByteArray();
                    var fileName = httpContent.GetContentDispositionFileName();
                    attachtments.Add(new FileUploadRequest
                    {
                        Bytes = getBytesTask,
                        FileName = fileName
                    });
                }
            }

            return model;
        }
    }
}