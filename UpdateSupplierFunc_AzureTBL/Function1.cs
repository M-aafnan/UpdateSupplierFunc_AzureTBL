using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Supplier_AzureTBLRModel.ViewModels;
using Microsoft.Azure.Cosmos.Table;
using Supplier_AzureTBLRModel.Entities;

namespace UpdateSupplierFunc_AzureTBL
{
    public static class Function1
    {
        [FunctionName("EditSupplier")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {

                CloudStorageAccount storageAcc = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=tablestorage1;AccountKey=mvdDaDEswE16qPIWefkIjcpiNjpvC8GbXEglDBjrMKItK0QsFXFxr0SwNSjIdzdKeDrShIZ6abHw+AStfRWs5A==;EndpointSuffix=core.windows.net");

                //// create table client
                CloudTableClient tblclient = storageAcc.CreateCloudTableClient(new TableClientConfiguration());

                // get customer table
                CloudTable cloudTable = tblclient.GetTableReference("Supplier");


                string requestBody;
                using (StreamReader streamReader = new StreamReader(req.Body))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                SupplierEditVM data = JsonConvert.DeserializeObject<SupplierEditVM>(requestBody);

                Supplier model = new Supplier()
                {
                    Timestamp = DateTime.UtcNow,
                    RowKey = data.Id,
                    PartitionKey = data.Id,
                    Id = data.Id,
                    Address = data.Address,
                    Contact = data.Contact,
                    EmailID = data.EmailID,
                    SupplierName = data.SupplierName,

                };


                //// get single record
                TableOperation retrieveOperation = TableOperation.Retrieve<Supplier>(model.Id, model.Id);
                TableResult retrievedResult = cloudTable.Execute(retrieveOperation);

                Supplier supplier = (Supplier)retrievedResult.Result;

                /// update 
                
                supplier.Address = data.Address;
                supplier.Contact = data.Contact;
                supplier.EmailID = data.EmailID;
                supplier.SupplierName = data.SupplierName;



                TableOperation updateTableOperation = TableOperation.InsertOrMerge(supplier);
                TableResult updateOrMergeTableResult = await cloudTable.ExecuteAsync(updateTableOperation);

                return new OkObjectResult(new ResponseModel() { Data = supplier, Success = true });
            }
            catch (Exception e)
            {
                return new OkObjectResult(new ResponseModel() { Data = e.Message, Success = true });
            }
           
        }
    }
}
