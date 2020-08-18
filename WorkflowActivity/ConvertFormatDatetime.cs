using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;

namespace WorkflowActivity
{
    public class ConvertFormatDatetime : CodeActivity
    {
        [RequiredArgument]
        [Input("DateTime input")]
        public InArgument<DateTime> DateToEvaluate { get; set; }
        [Output("Formatted DateTime output as string")]
        public OutArgument<string> FormattedDateTimeOutput { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);
            
            DateTime utcDateTime = this.DateToEvaluate.Get(context);
            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = utcDateTime.ToUniversalTime();
            }

            var settings = service.Retrieve("usersettings", workflowContext.UserId, new ColumnSet("timezonecode"));
            LocalTimeFromUtcTimeRequest timeZoneChangeRequest = new LocalTimeFromUtcTimeRequest()
            {
                UtcTime = utcDateTime,
                TimeZoneCode = int.Parse(settings["timezonecode"].ToString())
            };

            LocalTimeFromUtcTimeResponse timeZoneResponse = service.Execute(timeZoneChangeRequest) as LocalTimeFromUtcTimeResponse;
            this.FormattedDateTimeOutput.Set(context, string.Format("{0:f}", timeZoneResponse.LocalTime));
        }
    }
}
