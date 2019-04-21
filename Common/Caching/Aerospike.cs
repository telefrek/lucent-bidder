using Aerospike.Client;

namespace Lucent.Common.Caching
{
    internal static class Aerospike
    {
        public static readonly AsyncClient INSTANCE;

        static Aerospike()
        {
            var policy = new AsyncClientPolicy
            {
                asyncMaxCommands = 512,
                asyncMaxCommandAction = MaxCommandAction.BLOCK,
                readPolicyDefault = new Policy
                {
                    readModeAP = ReadModeAP.ALL,
                    readModeSC = ReadModeSC.ALLOW_REPLICA,
                    replica = Replica.MASTER_PROLES,
                    priority = Priority.MEDIUM,
                },
                writePolicyDefault = new WritePolicy
                {
                    commitLevel = CommitLevel.COMMIT_MASTER,
                    generationPolicy = GenerationPolicy.NONE,
                    priority = Priority.HIGH,
                },
            };

            INSTANCE = new AsyncClient(policy, "aspk-cache.lucent.svc", 3000);
        }

    }
}