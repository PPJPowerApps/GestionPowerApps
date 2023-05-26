using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace GestionPowerApps
{
    public class AsignacionEjecutivo : IPlugin
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

            //Verificación context vacio
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {
                    //Obtención de información flujo
                    Entity prospecto = (Entity)context.InputParameters["Target"];
                    EntityReference producto = (EntityReference)prospecto.Attributes["crbe4_productoaofrecer"];

                    //Query para obtener información de Configurador de Producto
                    QueryExpression queryConfiguradorEjecutivo = new QueryExpression
                    {
                        EntityName = "crbe4_configuradordeproducto",
                        ColumnSet = new ColumnSet("crbe4_ejecutivo", "crbe4_contador")
                    };
                
                    //Filtro para obtener ejecutivos asignados a producto ingresado en flujo
                    queryConfiguradorEjecutivo.Criteria.AddCondition("crbe4_productoaofrecer", ConditionOperator.Equal, producto.Id);
                    queryConfiguradorEjecutivo.AddOrder("crbe4_contador", OrderType.Ascending);
                    EntityCollection listaConfiguradorEjecutivos = orgService.RetrieveMultiple(queryConfiguradorEjecutivo);
                    Entity configuradorEjecutivoMenor = listaConfiguradorEjecutivos.Entities[0];

                    //Suma a contador de ejecutivo seleccionado
                    configuradorEjecutivoMenor.Attributes["crbe4_contador"] = int.Parse(configuradorEjecutivoMenor.Attributes["crbe4_contador"].ToString()) + 1;
                    orgService.Update(configuradorEjecutivoMenor);

                    //Asignación de ejecutivo a prospecto
                    EntityReference ejecutivo = (EntityReference)configuradorEjecutivoMenor["crbe4_ejecutivo"];
                    Entity auxProspecto = new Entity("crbe4_prospecto");
                    auxProspecto.Id = prospecto.Id;
                    auxProspecto.Attributes["crbe4_ejecutivo"] = ejecutivo;
                    auxProspecto.Attributes["crbe4_contador"] = 0;
                    orgService.Update(auxProspecto);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("Error de plugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
