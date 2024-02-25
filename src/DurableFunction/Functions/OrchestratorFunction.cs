using DurableFunction.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;

namespace DurableFunction.Functions
{
    public class OrchestratorFunction
    {
        [Function("OrchestratorFunction")]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var @event = await context.WaitForExternalEvent<InventoryEvent>("InventoryEventReceived");
            if (@event.TransactionNumber == context.InstanceId)
            {
                var entityInstanceId = new EntityInstanceId("IsLastMessageReceivedEntityFunction", context.InstanceId);

                if (await context.Entities.CallEntityAsync<bool>(entityInstanceId, operationName: "IsLastMessageReceivedActivity",
                    input: new IsLastMessageReceivedActivityFunctionInput()
                    {
                        Id = @event.TransactionNumber,
                    }))
                {
                    Console.WriteLine($"Last Message Arrived, lets consolidate data and upload csv file.");
                }
            }
        }

        [Function("IsLastMessageReceivedEntityFunction")]
        public Task DispatchAsync([EntityTrigger] TaskEntityDispatcher dispatcher)
        {
            return dispatcher.DispatchAsync(operation =>
            {
                // for the first time no state been set, so set it to false
                if (operation.State.GetState(typeof(bool)) is null)
                {
                    operation.State.SetState(false);
                }

                switch (operation.Name)
                {
                    case "IsLastMessageReceivedActivity":
                        bool state = operation.State.GetState<bool>();
                        if (state) return new(false);
                        operation.State.SetState(true);
                        return new(true);
                }

                return default;
            });
        }

        public class IsLastMessageReceivedActivityFunctionInput
        {
            public string Id { get; set; } = default!;
        }

        [Function("UploadToFtpActivityFunction")]
        public void UploadToFtpActivityFunction([ActivityTrigger] ILogger log)
        {
            //upload the file content into Sftp
        }
    }
}
