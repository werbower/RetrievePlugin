using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetrievePlugin {
    public class MyRetrievePlugin : IPlugin {
        public void Execute(IServiceProvider serviceProvider) {
            // Extract the tracing service for use in debugging sandboxed plug-ins.  
            // If you are not registering the plug-in in the sandbox, then you do  
            // not have to add any tracing service related code.  
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            Guid authorId = new Guid(context.InitiatingUserId.ToString());

            if (context.Stage == 20 //pre-event
                && context.MessageName == "RetrieveMultiple") {
                if (context.InputParameters.Contains("Query")) {
                    QueryExpression qe = null;
                    if (context.InputParameters["Query"] is FetchExpression fe) { //forward convert

                        var conversionRequest = new FetchXmlToQueryExpressionRequest {
                            FetchXml = fe.Query };
                        var conversionResponse = (FetchXmlToQueryExpressionResponse)service.Execute(conversionRequest);
                        qe = conversionResponse.Query;
                    } else if (context.InputParameters["Query"] is QueryExpression) {
                        qe = context.InputParameters["Query"] as QueryExpression;
                    }

                    if (qe != null) {
                        if (qe.EntityName == "new_specialentity"
                            && !hasRole(service, authorId)) {

                            ConditionExpression condition = new ConditionExpression() {
                                AttributeName = "new_spstatuscode",
                                Operator = ConditionOperator.NotIn,
                                Values = { true }
                            };
                            qe.Criteria.AddCondition(condition);

                            if (context.InputParameters["Query"] is FetchExpression ffe) { //back convert
                                var conversionRequest = new QueryExpressionToFetchXmlRequest {
                                    Query = qe };
                                var conversionResponse = (QueryExpressionToFetchXmlResponse)service.Execute(conversionRequest);
                                ffe.Query = conversionResponse.FetchXml;
                            }
                        }
                    }




                }
            }


        }
        private static bool hasRole(IOrganizationService service, Guid userId) {
            string roleName = "Special Role";
            var query = new QueryExpression("role");
            query.Criteria.AddCondition("name", ConditionOperator.Equal, roleName);
            var link = query.AddLink("systemuserroles", "roleid", "roleid");
            link.LinkCriteria.AddCondition("systemuserid", ConditionOperator.Equal, userId);

            return service.RetrieveMultiple(query).Entities.Count > 0;

        }
    }
}
