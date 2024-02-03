using DurableFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DurableFunction.Functions
{
    public class QueueTriggerFunction
    {
        private readonly ILogger<QueueTriggerFunction> _logger;

        public QueueTriggerFunction(ILogger<QueueTriggerFunction> logger)
        {
            _logger = logger;
        }

        [Function("QueueTriggerFunction")]
        public static async Task Run(
                    [ServiceBusTrigger("%QueueName%", Connection = "QueueConnectionString")] string message,
                    [Microsoft.Azure.Functions.Worker.DurableClient] IDurableOrchestrationClient starter,
                    ILogger log)
        {
            var @event = JsonSerializer.Deserialize<InventoryEvent>(message);
            if (@event == null) return;

            var correlationId = @event.TransactionNumber;

            var existingInstance = await starter.GetStatusAsync(correlationId);
            if (existingInstance == null)
            {
                //persist the data in azure table storage 
                //and then signal to 'OrchestratorFunction'
                var instanceId = await starter.StartNewAsync("OrchestratorFunction", correlationId);
                log.LogInformation($"Started new orchestration with ID = '{instanceId}'.");
            }
            else
            {
                log.LogInformation($"Using existing orchestration with ID = '{existingInstance.InstanceId}'.");
                await starter.RaiseEventAsync(existingInstance.InstanceId, "NewInventoryMessageReceivedEvent", @event.TransactionNumber);
            }
        }
    }
}
