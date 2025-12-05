using System;
using System.Collections.Generic;
using System.Text.Json;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Harvestry.Identity.Infrastructure.External;

/// <summary>
/// Bootstraps Slack credentials from AWS Secrets Manager into the configuration system.
/// </summary>
public static class SlackSecretsBootstrapper
{
    public static void TryAddSlackSecrets(ConfigurationManager configuration, ILogger logger)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (logger is null)
        {
            throw new ArgumentNullException(nameof(logger));
        }

        var secretsSection = configuration.GetSection("Slack:SecretsManager");
        var enabled = secretsSection.GetValue<bool?>("Enabled");

        if (enabled.HasValue && enabled.Value == false)
        {
            return;
        }

        var secretName = secretsSection.GetValue<string?>("SecretName")
            ?? configuration["TASKS_SLACK_SECRET_NAME"]
            ?? Environment.GetEnvironmentVariable("TASKS_SLACK_SECRET_NAME")
            ?? "slack_tasks_dev";

        var region = secretsSection.GetValue<string?>("Region")
            ?? configuration["AWS:Region"]
            ?? Environment.GetEnvironmentVariable("AWS_REGION")
            ?? Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION");

        if (string.IsNullOrWhiteSpace(region))
        {
            logger.LogInformation("Skipping Slack secret bootstrap because AWS region is not configured");
            return;
        }

        try
        {
            using var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));
            var response = client.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = secretName
            }).GetAwaiter().GetResult();

            if (string.IsNullOrWhiteSpace(response.SecretString))
            {
                logger.LogWarning("Slack secret '{SecretName}' returned an empty payload", secretName);
                return;
            }

            var mappedValues = MapSecretPayload(response.SecretString);

            if (mappedValues.Count == 0)
            {
                logger.LogWarning("Slack secret '{SecretName}' did not contain recognized keys", secretName);
                return;
            }

            configuration.AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("Slack:SecretsManager:SecretName", secretName),
                new KeyValuePair<string, string?>("Slack:SecretsManager:Region", region)
            });

            configuration.AddInMemoryCollection(mappedValues);
            logger.LogInformation("Loaded Slack credentials from AWS Secrets Manager secret '{SecretName}'", secretName);
        }
        catch (AmazonSecretsManagerException ex)
        {
            logger.LogError(ex, "Unable to load Slack secret '{SecretName}' from AWS Secrets Manager", secretName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while loading Slack secret '{SecretName}'", secretName);
        }
    }

    private static Dictionary<string, string?> MapSecretPayload(string secretPayload)
    {
        using var json = JsonDocument.Parse(secretPayload);

        if (json.RootElement.ValueKind != JsonValueKind.Object)
        {
            return new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }

        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in json.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var value = property.Value.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            values[property.Name] = value;

            switch (property.Name)
            {
                case "TASKS_SLACK_BOT_TOKEN":
                    values["Slack:Credentials:BotToken"] = value;
                    break;
                case "TASKS_SLACK_REFRESH_TOKEN":
                    values["Slack:Credentials:RefreshToken"] = value;
                    break;
                case "TASKS_SLACK_WORKSPACE_ID":
                    values["Slack:Credentials:WorkspaceId"] = value;
                    break;
                case "SLACK_CLIENT_ID":
                    values["Slack:OAuth:ClientId"] = value;
                    break;
                case "SLACK_CLIENT_SECRET":
                    values["Slack:OAuth:ClientSecret"] = value;
                    break;
            }
        }

        return values;
    }
}
