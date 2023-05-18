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
                EntityCollection listaConfiguradorEjecutivos = orgService.RetrieveMultiple(queryConfiguradorEjecutivo);

                //Lógica para seleccionar ejecutivo con menor contador
                int contadorMinimo = int.MaxValue;
                Entity configuradorEjecutivoMenor = null;
                foreach (Entity auxEjecutivo in listaConfiguradorEjecutivos.Entities)
                {
                    int contador = int.Parse(auxEjecutivo.Attributes["crbe4_contador"].ToString());

                    if (contador < contadorMinimo)
                    {
                        contadorMinimo = contador;
                        configuradorEjecutivoMenor = auxEjecutivo;
                    }
                }

                //Suma a contador de ejecutivo seleccionado
                configuradorEjecutivoMenor.Attributes["crbe4_contador"] = int.Parse(configuradorEjecutivoMenor.Attributes["crbe4_contador"].ToString()) + 1;
                orgService.Update(configuradorEjecutivoMenor);

                //Asignación de ejecutivo a prospecto
                EntityReference ejecutivo = (EntityReference)configuradorEjecutivoMenor["crbe4_ejecutivo"];
                prospecto.Attributes["crbe4_ejecutivo"] = ejecutivo;
                orgService.Update(prospecto);

            }
            catch (Exception ex)
            {
                tracingService.Trace("Error de plugin: {0}", ex.ToString());
                throw;
            }
        }
    }
}
