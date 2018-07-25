using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Handler;
using DistillNET;
using Guitarify.Wpf.Properties;
using Guitarify.Wpf.Util;

namespace Guitarify.Wpf.Handlers
{
    public class AdBlockRequestHandler : DefaultRequestHandler
    {

        public override bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request,
                                                IResponse   response)
        {
            if (AdBlock.IsAdUrl(new Uri(request.Url), response.ResponseHeaders, request.ReferrerUrl))
            {
                return false;
            }

            return base.OnResourceResponse(browserControl, browser, frame, request, response);
        }

        public override CefReturnValue OnBeforeResourceLoad(IWebBrowser      browserControl, IBrowser browser, IFrame frame, IRequest request,
                                                            IRequestCallback callback)
        {
            if (AdBlock.IsAdUrl(new Uri(request.Url), request.Headers, request.ReferrerUrl))
            {
                return CefReturnValue.Cancel;
            }

            return base.OnBeforeResourceLoad(browserControl, browser, frame, request, callback);
        }
    }
}
