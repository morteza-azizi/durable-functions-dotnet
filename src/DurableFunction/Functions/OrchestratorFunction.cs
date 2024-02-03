using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace DurableFunction.Functions
{
    public static class OrchestratorFunction
    {
        [Function("OrchestratorFunction")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context, ILogger log)
        {
            log.LogInformation($"OrchestratorFunction called with ID = '{context.InstanceId}'.");

            var eventName = context.GetInput<string>();
            if (eventName == "NewInventoryMessageReceivedEvent")
            {
                bool isLastMessageReceived = await context.CallActivityAsync<bool>("IsLastMessageReceivedActivityFunction");

                if (!isLastMessageReceived)
                {
                    await context.CallActivityAsync<bool>("UploadToFtpActivityFunction");
                }
            }
        }

        [FunctionName("IsLastMessageReceivedActivityFunction")]
        public static bool IsLastMessageReceivedActivityFunction([ActivityTrigger] ILogger log)
        {
            // add the logic if all pages arrived
            return true;
        }

        [FunctionName("UploadToFtpActivityFunction")]
        public static void UploadToFtpActivityFunction([ActivityTrigger] ILogger log)
        {
            //upload the file content into Sftp
        }
    }
}
