using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        readonly IConfiguration configuration;

        public IndexModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }

        public string LoginUrl
        {
            get
            {
                var authorizeEndpoint = configuration["authorizeEndpoint"];
                var clientId = configuration["clientId"];
                var callbackUrl = configuration["callbackURL"];
                var state = "state 123";
                var scope = "openid offline_access";

                var result = $"{authorizeEndpoint}?client_id={clientId}&redirect_uri={callbackUrl}&response_type=code&state={state}&scope={scope}";

                return result;
            }
        }

        public void OnGet()
        {

        }
    }
}
