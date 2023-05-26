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

            //Verificación contexto vacío
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {
                    if (context.Depth > 1)
                    {
                        return;
                    }
                    //Obtención de nueva información prospecto
                    Entity prospectoNuevo = (Entity)context.InputParameters["Target"];
                    EntityReference productoNuevo = (EntityReference)prospectoNuevo.Attributes["crbe4_productoaofrecer"];

                    tracingService.Trace(productoNuevo.Id.ToString());

                    //Obtención de información antigua de prospecto
                    ColumnSet csProspectoAntiguo = new ColumnSet("crbe4_productoaofrecer", "crbe4_ejecutivo");
                    Entity prospectoAntiguo = orgService.Retrieve("crbe4_prospecto", prospectoNuevo.Id, csProspectoAntiguo);
                    EntityReference productoAntiguo = (EntityReference)prospectoAntiguo.Attributes["crbe4_productoaofrecer"];
                    EntityReference ejecutivoAntiguo = (EntityReference)prospectoAntiguo.Attributes["crbe4_ejecutivo"];

                    tracingService.Trace(productoAntiguo.Id.ToString());

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
                    queryConfiguradorEjecutivoNuevo.AddOrder("crbe4_contador", OrderType.Ascending);
                    EntityCollection listaConfiguradorEjecutivos = orgService.RetrieveMultiple(queryConfiguradorEjecutivoNuevo);
                    Entity configuradorEjecutivoMenor = listaConfiguradorEjecutivos.Entities[0];

                    //Suma a contador ejecutivo seleccionado
                    configuradorEjecutivoMenor.Attributes["crbe4_contador"] = int.Parse(configuradorEjecutivoMenor.Attributes["crbe4_contador"].ToString()) + 1;
                    orgService.Update(configuradorEjecutivoMenor);
                    
                    //Asignacion de ejecutivo a prospecto
                    EntityReference ejecutivoNuevo = (EntityReference)configuradorEjecutivoMenor["crbe4_ejecutivo"];
                    prospectoNuevo.Attributes["crbe4_ejecutivo"] = ejecutivoNuevo;
                    Entity prospectoAux = new Entity();
                    prospectoAux.Id = prospectoNuevo.Id;
                    prospectoAux.Attributes["crbe4_ejecutivo"] = ejecutivoNuevo;
                    orgService.Update(prospectoAux);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("Error de plugin: {0}", ex.ToString());
                }
            }
        }
    }
}
