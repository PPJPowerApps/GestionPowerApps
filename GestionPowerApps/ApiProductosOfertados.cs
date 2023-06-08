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

            //Obtención y declaración de variables
            Guid prospecto = (Guid)context.InputParameters["crbe4_prospecto"];
            Guid producto = (Guid)context.InputParameters["crbe4_productoaofrecer"];
            bool ofertado = true;

            //Query para obtener productos ofertados
            QueryExpression queryProductosOfertados = new QueryExpression
            {
                EntityName = "crbe4_productoofertado"
            };

            //Filtros para saber si producto fue ofertado a cliente específico
            queryProductosOfertados.Criteria.AddCondition("crbe4_prospecto", ConditionOperator.Equal, prospecto);
            queryProductosOfertados.Criteria.AddCondition("crbe4_productoaofrecer", ConditionOperator.Equal, producto);
            EntityCollection productosOfertados = orgService.RetrieveMultiple(queryProductosOfertados);

            //Lógica para verificar si producto fue ofertado
            if (productosOfertados.Entities.Count.Equals(0))
            {
                ofertado = false;
            }

            //Envío de respuesta
            context.OutputParameters["respuesta"] = ofertado;
        }
    }
}
