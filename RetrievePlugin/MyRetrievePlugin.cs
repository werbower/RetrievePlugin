using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetrievePlugin{
    public class MyRetrievePlugin : IPlugin {
        public void Execute(IServiceProvider serviceProvider) {
            // Extract the tracing service for use in debugging sandboxed plug-ins.  
            // If you are not registering the plug-in in the sandbox, then you do  
            // not have to add any tracing service related code.  
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.Stage == 20 //pre-event
                && context.MessageName=="RetrieveMultiple") {
                if (context.InputParameters.Contains("Query")) {
                    if (context.InputParameters["Query"] is QueryExpression qe) {
                        if (qe.EntityName == "new_specialentity") {

                            ConditionExpression condition = new ConditionExpression() {
                                AttributeName = "new_spstatuscode",
                                Operator = ConditionOperator.NotIn,
                                Values = {true}//0 no,1 yes
                            };
                            qe.Criteria.AddCondition(condition);
                        }
                    }
                }
            }


        }
    }
}
