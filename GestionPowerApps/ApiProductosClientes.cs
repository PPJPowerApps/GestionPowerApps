using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GestionPowerApps
{
    public class ApiProductosClientes : IPlugin
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

            //Obtención de ID Cliente
            Guid cliente = (Guid)context.InputParameters["crbe4_clientes"];

            //Query para obtener lista de productos existentes
            QueryExpression queryProductos = new QueryExpression
            {
                EntityName = "crbe4_productoaofrecer"
            };

            //Obtención de productos
            EntityCollection productos = orgService.RetrieveMultiple(queryProductos);
            List<Entity> listaProductos = productos.Entities.ToList();

            //Query para obtener lista de productos del cliente
            QueryExpression queryProductosCliente = new QueryExpression
            {
                EntityName = "crbe4_productocliente",
                ColumnSet = new ColumnSet("crbe4_productoaofrecer")
            };

            //Filtro para obtener todos los productos del cliente
            queryProductosCliente.Criteria.AddCondition("crbe4_clientes", ConditionOperator.Equal, cliente);

            //Obtención de productos cliente
            EntityCollection productosCliente = orgService.RetrieveMultiple(queryProductosCliente);

            //Filtro de productos asignados a cliente
            foreach (Entity producto in productosCliente.Entities)
            {
                EntityReference auxProducto = (EntityReference)producto.Attributes["crbe4_productoaofrecer"];
                listaProductos.RemoveAll(p => p.Id == auxProducto.Id);
            }

            //Vacío y llenado con productos filtrados
            productos.Entities.Clear();
            productos.Entities.AddRange(listaProductos);

            //Envío de respuesta
            context.OutputParameters["productos"] = productos;

        }
    }
}
