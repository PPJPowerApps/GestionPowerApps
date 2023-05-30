using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionPowerApps
{
    public class ApiProductosOfertados : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider) 
        {
            //Contexto de ejecucion (Flujo)
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            //Servicio de organizacion (CRUD)
            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService orgService = serviceFactory.CreateOrganizationService(context.UserId);

            //Rastreo de servicio (debug)
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Guid prospecto = (Guid)context.InputParameters["prospecto"];
            Guid producto = (Guid)context.InputParameters["producto"];
            bool ofertado = true;

            tracingService.Trace(prospecto.ToString());
            tracingService.Trace(producto.ToString());

            QueryExpression queryProductosOfertados = new QueryExpression
            {
                EntityName = "crbe4_productoofertado"
            };

            queryProductosOfertados.Criteria.AddCondition("crbe4_prospecto", ConditionOperator.Equal, prospecto);
            queryProductosOfertados.Criteria.AddCondition("crbe4_productoaofrecer", ConditionOperator.Equal, producto);

            EntityCollection productosOfertados = orgService.RetrieveMultiple(queryProductosOfertados);

            if (productosOfertados.Entities.Count.Equals(0))
            {
                ofertado = false;
            }

            context.OutputParameters["respuesta"] = ofertado;
        }
    }
}
