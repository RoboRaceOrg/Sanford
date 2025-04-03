using System;
using System.Collections.Generic;

public static class EnvironmentVariableChecker
{
    public static (string Status, string Message) CheckEnvironmentVariables(List<string> requiredVariables)
    {
        var missingVariables = new List<string>();

        // Check if each variable is set
        foreach (var variable in requiredVariables)
        {
            var value = Environment.GetEnvironmentVariable(variable);
            if (string.IsNullOrEmpty(value))
            {
                missingVariables.Add(variable);
            }
        }

        // Respond with the result
        if (missingVariables.Count > 0)
        {
            return (
                Status: "Failed",
                Message: $"Missing environment variables: {string.Join(", ", missingVariables)}"
            );
        }
        else
        {
            return (
                Status: "Passed",
                Message: "All required environment variables are set!"
            );
        }
    }
}