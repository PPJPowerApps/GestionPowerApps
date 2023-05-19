using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace GestionPowerApps
{
    public class ReasignarEjecutivo : IPlugin
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
                if (context.Depth > 1)
                {
                    return;
                }
                //Obtención de nueva información prospecto
                Entity prospectoNuevo = (Entity)context.InputParameters["Target"];
                EntityReference productoNuevo = (EntityReference)prospectoNuevo.Attributes["crbe4_productoaofrecer"];

                //Obtención de información antigua de prospecto
                Entity prospectoAntiguo = context.PreEntityImages["prospectoAntiguo"];
                EntityReference productoAntiguo = (EntityReference)prospectoAntiguo.Attributes["crbe4_productoaofrecer"];
                EntityReference ejecutivoAntiguo = (EntityReference)prospectoAntiguo.Attributes["crbe4_ejecutivo"];

                //Query para obtener información de Configurador de Producto
                QueryExpression queryConfiguradorEjecutivoAntiguo = new QueryExpression
                {
                    EntityName = "crbe4_configuradordeproducto",
                    ColumnSet = new ColumnSet("crbe4_ejecutivo", "crbe4_contador")
                };

                //Filtros para obtener ejecutivo asignado a prospecto
                queryConfiguradorEjecutivoAntiguo.Criteria.AddCondition("crbe4_productoaofrecer", ConditionOperator.Equal, productoAntiguo.Id);
                queryConfiguradorEjecutivoAntiguo.Criteria.AddCondition("crbe4_ejecutivo", ConditionOperator.Equal, ejecutivoAntiguo.Id);
                EntityCollection configuradorEjecutivoAntiguo = orgService.RetrieveMultiple(queryConfiguradorEjecutivoAntiguo);

                //Resta de contador a ejecutivo antiguo
                configuradorEjecutivoAntiguo.Entities[0].Attributes["crbe4_contador"] = int.Parse(configuradorEjecutivoAntiguo.Entities[0].Attributes["crbe4_contador"].ToString()) - 1;
                orgService.Update(configuradorEjecutivoAntiguo.Entities[0]);

                //Query para obtener información de Configurador de Producto
                QueryExpression queryConfiguradorEjecutivoNuevo = new QueryExpression
                {
                    EntityName = "crbe4_configuradordeproducto",
                    ColumnSet = new ColumnSet("crbe4_ejecutivo", "crbe4_contador")
                };

                //Filtros para obtener ejecutivos asignados a producto nuevo
                queryConfiguradorEjecutivoNuevo.Criteria.AddCondition("crbe4_productoaofrecer", ConditionOperator.Equal, productoNuevo.Id);
                EntityCollection listaConfiguradorEjecutivos = orgService.RetrieveMultiple(queryConfiguradorEjecutivoNuevo);

                //Lógica para seleccionar ejecutivo con menor contador
                int contadorMinimo = int.MaxValue;
                Entity configuradorEjecutivoMenor = null;
                foreach(Entity auxEjecutivo in listaConfiguradorEjecutivos.Entities)
                {
                    int contador = int.Parse(auxEjecutivo.Attributes["crbe4_contador"].ToString());

                    if (contador < contadorMinimo)
                    {
                        contadorMinimo = contador;
                        configuradorEjecutivoMenor = auxEjecutivo;
                    }
                }

                //Suma a contador ejecutivo seleccionado
                configuradorEjecutivoMenor.Attributes["crbe4_contador"] = int.Parse(configuradorEjecutivoMenor.Attributes["crbe4_contador"].ToString()) + 1;
                orgService.Update(configuradorEjecutivoMenor);

                //Asignacion de ejecutivo a prospecto
                EntityReference ejecutivoNuevo = (EntityReference)configuradorEjecutivoMenor["crbe4_ejecutivo"];
                prospectoNuevo.Attributes["crbe4_ejecutivo"] = ejecutivoNuevo;
                orgService.Update(prospectoNuevo);

            }
            catch (Exception ex)
            {
                tracingService.Trace("Error de plugin: {0}", ex.ToString());
                throw;
            }
        }
    }
}
