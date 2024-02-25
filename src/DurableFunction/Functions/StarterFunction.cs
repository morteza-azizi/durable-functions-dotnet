using DurableFunction.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Newtonsoft.Json;

namespace DurableFunction.Functions
{
    public class StarterFunction
    {

        [Function(nameof(StarterFunction))]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client)
        {
            var requestContent = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<SampleRequest>(requestContent);

            var correlationId = Guid.NewGuid().ToString();
            for (int i = 0; i < request.NumberOfMessages; i++)
            {
                var @event = new InventoryEvent()
                {
                    TransactionNumber = correlationId,
                    PageNumber = i,
                    TotalPages = request.NumberOfMessages,
                    Items = new List<string>()
                        {
                            "1", "2", "3", "4", "5",
                        }
                };


                var existingInstance = await client.GetInstanceAsync(correlationId);

                if (existingInstance != null &&
                    (existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Completed ||
                    existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Terminated ||
                    existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Failed))
                {
                    return new OkObjectResult($"An Instance with Id: {existingInstance.InstanceId} with status {existingInstance.RuntimeStatus} exist and this batch must have been processed before.");
                }

                if (existingInstance == null)
                {
                    await client.ScheduleNewOrchestrationInstanceAsync(nameof(OrchestratorFunction),
                        new StartOrchestrationOptions()
                        {
                            InstanceId = correlationId,
                        });
                }

                await client.WaitForInstanceStartAsync(correlationId);
                await client.RaiseEventAsync(correlationId, "InventoryEventReceived", @event);
            }


            return new OkObjectResult("OK");

        }
    }
}
