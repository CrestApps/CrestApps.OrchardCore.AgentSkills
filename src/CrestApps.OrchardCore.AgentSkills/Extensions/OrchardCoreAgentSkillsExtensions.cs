using System;
using System.IO;

namespace CrestApps.OrchardCore.AgentSkills.Extensions
{
    public static class OrchardCoreAgentSkillsExtensions
    {
        /// <summary>
        /// Adds Orchard Core agent skills from the auto-mounted .agents/skills directory.
        /// </summary>
        /// <param name="builder">The agent builder instance.</param>
        /// <returns>The agent builder for chaining.</returns>
        public static IAgentBuilder AddOrchardCoreSkills(
            this IAgentBuilder builder)
        {
            var path = Path.Combine(
                AppContext.BaseDirectory,
                ".agents",
                "skills"
            );

            builder.AddSkillsFromPath(path);

            return builder;
        }
    }
}
