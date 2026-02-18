using DigitalTwinPlatform.Domain.Models;

namespace DigitalTwinPlatform.API.Services.Simulation;

/// <summary>
/// Interface for evaluating transfer functions used in sensor data generation.
/// </summary>
public interface ITransferFunctionEvaluator
{
    /// <summary>
    /// Evaluates a transfer function expression with given parameters and degradation state.
    /// </summary>
    /// <param name="transferFunction">The transfer function expression string.</param>
    /// <param name="parameters">Dictionary of parameter names and values.</param>
    /// <param name="degradationState">Current degradation state value.</param>
    /// <param name="random">Random number generator for noise injection.</param>
    /// <returns>The evaluated transfer function result.</returns>
    double Evaluate(
        string transferFunction,
        Dictionary<string, double> parameters,
        double degradationState,
        Random random);
}

/// <summary>
/// Evaluates mathematical transfer functions for sensor data generation based on degradation models.
/// Supports basic arithmetic operations, power functions, and trigonometric operations.
/// </summary>
public class TransferFunctionEvaluator : ITransferFunctionEvaluator
{
    /// <summary>
    /// Evaluates a transfer function expression with given parameters and degradation state.
    /// </summary>
    /// <param name="transferFunction">The transfer function expression string.</param>
    /// <param name="parameters">Dictionary of parameter names and values.</param>
    /// <param name="degradationState">Current degradation state value.</param>
    /// <param name="random">Random number generator for noise injection.</param>
    /// <returns>The evaluated transfer function result.</returns>
    /// <exception cref="ArgumentException">Thrown when transfer function is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when expression evaluation fails.</exception>
    public double Evaluate(
        string transferFunction,
        Dictionary<string, double> parameters,
        double degradationState,
        Random random)
    {
        if (string.IsNullOrWhiteSpace(transferFunction))
        {
            throw new ArgumentException("Transfer function cannot be null or empty", nameof(transferFunction));
        }

        // Replace degradationState variable
        var expression = transferFunction.Replace("degradationState", degradationState.ToString("F6"), StringComparison.OrdinalIgnoreCase);
        
        // Replace all parameters
        foreach (var param in parameters)
        {
            expression = expression.Replace(param.Key, param.Value.ToString("F6"), StringComparison.OrdinalIgnoreCase);
        }

        // Evaluate mathematical expression
        // Support: +, -, *, /, ^, exp, log, sin, cos, sqrt
        try
        {
            var result = EvaluateExpression(expression);
            
            // Add noise if noiseLevel is specified
            if (parameters.TryGetValue("noiseLevel", out var noiseLevel) && noiseLevel > 0)
            {
                var noise = (random.NextDouble() - 0.5) * 2 * noiseLevel; // Uniform noise in [-noiseLevel, noiseLevel]
                result += noise;
            }
            
            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to evaluate transfer function: {transferFunction}", ex);
        }
    }

    private double EvaluateExpression(string expression)
    {
        // Use DataTable.Compute for basic arithmetic expressions
        // It supports: +, -, *, /, and parentheses
        try
        {
            // Replace power operator (^) with multiplication for simple cases
            // For complex expressions with exp/log/sin/cos, we'll handle them separately
            var processedExpression = expression;
            
            // Handle power operator - convert x^y to x*x for simple cases, or use Math.Pow
            if (processedExpression.Contains('^'))
            {
                // Simple approach: replace x^2 with x*x, x^3 with x*x*x, etc.
                // For non-integer powers, we'll need a different approach
                // For now, handle simple cases
                var parts = processedExpression.Split('^');
                if (parts.Length == 2)
                {
                    var baseExpr = parts[0].Trim();
                    var powerStr = parts[1].Trim();
                    if (double.TryParse(powerStr, out var power) && power == (int)power)
                    {
                        // Integer power - use multiplication
                        var powerResult = baseExpr;
                        for (int i = 1; i < (int)power; i++)
                        {
                            powerResult = $"({powerResult}) * ({baseExpr})";
                        }
                        processedExpression = processedExpression.Replace($"{baseExpr}^{powerStr}", $"({powerResult})");
                    }
                    else
                    {
                        // Non-integer power - use Math.Pow
                        processedExpression = processedExpression.Replace("^", ", ");
                        processedExpression = $"Math.Pow({processedExpression})";
                    }
                }
            }
            
            // Handle Math functions - DataTable.Compute doesn't support these directly
            // So we'll evaluate them separately
            if (processedExpression.Contains("Math.", StringComparison.OrdinalIgnoreCase) ||
                processedExpression.Contains("exp(", StringComparison.OrdinalIgnoreCase) ||
                processedExpression.Contains("log(", StringComparison.OrdinalIgnoreCase) ||
                processedExpression.Contains("sin(", StringComparison.OrdinalIgnoreCase) ||
                processedExpression.Contains("cos(", StringComparison.OrdinalIgnoreCase) ||
                processedExpression.Contains("sqrt(", StringComparison.OrdinalIgnoreCase))
            {
                return EvaluateWithMathFunctions(processedExpression);
            }
            
            // Use DataTable.Compute for basic arithmetic
            var dataTable = new System.Data.DataTable();
            var result = dataTable.Compute(processedExpression, null);
            
            return Convert.ToDouble(result);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to evaluate expression: {expression}. Error: {ex.Message}", ex);
        }
    }

    private double EvaluateWithMathFunctions(string expression)
    {
        // Evaluate expressions containing math functions
        // This is a simplified evaluator - for production, consider using NCalc library
        
        // Handle exp(x) -> Math.Exp(x)
        expression = System.Text.RegularExpressions.Regex.Replace(
            expression, 
            @"exp\s*\(\s*([^)]+)\s*\)", 
            m => $"Math.Exp({m.Groups[1].Value})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Handle log(x) -> Math.Log(x)
        expression = System.Text.RegularExpressions.Regex.Replace(
            expression,
            @"log\s*\(\s*([^)]+)\s*\)",
            m => $"Math.Log({m.Groups[1].Value})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Handle sin(x) -> Math.Sin(x)
        expression = System.Text.RegularExpressions.Regex.Replace(
            expression,
            @"sin\s*\(\s*([^)]+)\s*\)",
            m => $"Math.Sin({m.Groups[1].Value})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Handle cos(x) -> Math.Cos(x)
        expression = System.Text.RegularExpressions.Regex.Replace(
            expression,
            @"cos\s*\(\s*([^)]+)\s*\)",
            m => $"Math.Cos({m.Groups[1].Value})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Handle sqrt(x) -> Math.Sqrt(x)
        expression = System.Text.RegularExpressions.Regex.Replace(
            expression,
            @"sqrt\s*\(\s*([^)]+)\s*\)",
            m => $"Math.Sqrt({m.Groups[1].Value})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        // Evaluate nested Math functions recursively
        while (expression.Contains("Math.", StringComparison.OrdinalIgnoreCase))
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                expression,
                @"Math\.(Exp|Log|Sin|Cos|Sqrt|Pow)\s*\(\s*([^)]+)\s*\)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            if (!match.Success) break;
            
            var funcName = match.Groups[1].Value.ToLower();
            var argStr = match.Groups[2].Value;
            
            // Parse argument (could be a single number or expression)
            var argValue = EvaluateExpression(argStr);
            
            double result = funcName switch
            {
                "exp" => Math.Exp(argValue),
                "log" => Math.Log(argValue),
                "sin" => Math.Sin(argValue),
                "cos" => Math.Cos(argValue),
                "sqrt" => Math.Sqrt(argValue),
                "pow" => EvaluatePowFunction(argStr, argValue),
                _ => throw new NotSupportedException($"Math function {funcName} not supported")
            };
            
            expression = expression.Replace(match.Value, result.ToString("F10"));
        }
        
        // Evaluate remaining arithmetic
        return EvaluateExpression(expression);
    }

    private double EvaluatePowFunction(string argStr, double unused)
    {
        // Math.Pow(x, y) - parse both arguments
        var parts = argStr.Split(',');
        if (parts.Length != 2)
        {
            throw new InvalidOperationException($"Invalid Math.Pow arguments: {argStr}");
        }
        
        var x = EvaluateExpression(parts[0].Trim());
        var y = EvaluateExpression(parts[1].Trim());
        
        return Math.Pow(x, y);
    }
}
