using Serilog;
using System.Text;
using System.Text.RegularExpressions;

namespace NG.MicroERP.API.Helper;

public static class SQLInjectionHelper
{
    public static bool IsSafeSearchCriteria(string criteria)
    {
        if (string.IsNullOrEmpty(criteria))
            return true;

        // Layer 1: Length validation
        if (criteria.Length > 500)
        {
            Log.Warning("Search criteria rejected - Length exceeded {Length} characters: {Criteria}", criteria.Length, criteria);
            return false;
        }

        // Layer 2: Character whitelist approach
        if (!ContainsOnlyAllowedCharacters(criteria))
        {
            Log.Warning("Search criteria rejected - Invalid characters: {Criteria}", criteria);
            return false;
        }

        // Layer 3: SQL injection pattern detection
        if (ContainsSqlInjectionPatterns(criteria))
        {
            Log.Warning("Search criteria rejected - SQL injection patterns: {Criteria}", criteria);
            return false;
        }

        // Layer 4: Context-aware keyword detection
        if (ContainsDangerousSqlPatterns(criteria))
        {
            Log.Warning("Search criteria rejected - Dangerous SQL patterns: {Criteria}", criteria);
            return false;
        }

        // Layer 5: Encoding/obfuscation detection
        if (ContainsEncodedSqlPatterns(criteria))
        {
            Log.Warning("Search criteria rejected - Encoded SQL patterns: {Criteria}", criteria);
            return false;
        }

        Log.Information("Search criteria accepted: {Criteria}", criteria);
        return true;
    }

    // Layer 2: Character whitelist - only allow safe characters
    private static bool ContainsOnlyAllowedCharacters(string input)
    {
        // Allow alphanumeric, basic punctuation, and whitespace
        // Adjust this regex based on your specific search requirements
        var allowedCharacterRegex = new Regex(@"^[a-zA-Z0-9\s\-\.,!?@#\$%&\*\(\)\[\]\{\}\+=\|:;'""<>\\\/~`_]*$");
        bool isValid = allowedCharacterRegex.IsMatch(input);

        if (!isValid)
        {
            Log.Warning("Character validation failed - Contains disallowed characters: {Input}", input);
        }

        return isValid;
    }

    // Layer 3: Comprehensive SQL injection pattern detection
    private static bool ContainsSqlInjectionPatterns(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        var upperInput = input.ToUpperInvariant();

        // SQL comment and statement separators
        var sqlSyntaxPatterns = new[]
        {
        ";", "--", "/*", "*/", "#", "@@",
        "CHAR(", "NCHAR(", "VARCHAR(", "NVARCHAR(",
        "CAST(", "CONVERT(", "DECLARE", "BEGIN", "END"
    };

        foreach (var pattern in sqlSyntaxPatterns)
        {
            if (upperInput.Contains(pattern))
            {
                Log.Warning("SQL syntax pattern detected: {Pattern} in input: {Input}", pattern, input);
                return true;
            }
        }

        return false;
    }

    // Layer 4: Context-aware dangerous SQL patterns
    private static bool ContainsDangerousSqlPatterns(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        var upperInput = input.ToUpperInvariant();

        // DML operations (must be blocked in search context)
        var dangerousDmlPatterns = new[]
        {
        "DROP ", "DELETE ", "UPDATE ", "INSERT ", "CREATE ", "ALTER ",
        "TRUNCATE ", "MERGE ", "REPLACE ", "RENAME "
    };

        // System procedures and extended procedures
        var dangerousProcedurePatterns = new[]
        {
        "EXEC ", "EXECUTE ", "XP_", "SP_", "MS_", "SYS.", "DBO.",
        "SYSTEM.", "INFORMATION_SCHEMA.", "MASTER."
    };

        // Query manipulation patterns
        var queryManipulationPatterns = new[]
        {
        "UNION ", "UNION ALL", "UNION SELECT", "JOIN ", "INNER JOIN",
        "OUTER JOIN", "CROSS JOIN", "WHERE ", "HAVING ", "GROUP BY",
        "ORDER BY", "SELECT ", "FROM ", "INTO ", "VALUES "
    };

        // Check for patterns with word boundaries for better accuracy
        foreach (var pattern in dangerousDmlPatterns)
        {
            if (ContainsWord(upperInput, pattern))
            {
                Log.Warning("Dangerous DML pattern detected: {Pattern} in input: {Input}", pattern, input);
                return true;
            }
        }

        foreach (var pattern in dangerousProcedurePatterns)
        {
            if (upperInput.Contains(pattern))
            {
                Log.Warning("Dangerous procedure pattern detected: {Pattern} in input: {Input}", pattern, input);
                return true;
            }
        }

        foreach (var pattern in queryManipulationPatterns)
        {
            if (ContainsWord(upperInput, pattern))
            {
                Log.Warning("Query manipulation pattern detected: {Pattern} in input: {Input}", pattern, input);
                return true;
            }
        }

        // Block common attack concatenation patterns
        if (upperInput.Contains("'+") || upperInput.Contains("\"+") || upperInput.Contains("||") || upperInput.Contains("&&"))
        {
            Log.Warning("Attack concatenation pattern detected in input: {Input}", input);
            return true;
        }

        return false;
    }

    // Layer 5: Detect encoded/obfuscated SQL patterns
    private static bool ContainsEncodedSqlPatterns(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        var lowerInput = input.ToLowerInvariant();

        // URL encoded patterns
        var urlEncodedPatterns = new[]
        {
        "%27", "%22", "%3B", "%2D%2D", "%2F%2A", "%2A%2F",
        "%25", "%5C", "%2F", "%3C", "%3E"
    };

        // Hex encoded patterns
        var hexEncodedPatterns = new[]
        {
        "0x27", "0x22", "0x3B", "0x2D2D", "0x2F2A", "0x2A2F"
    };

        // Unicode patterns
        var unicodePatterns = new[]
        {
        "\\u0027", "\\u0022", "\\u003B", "\\u002D\\u002D",
        "\\u002F\\u002A", "\\u002A\\u002F"
    };

        foreach (var pattern in urlEncodedPatterns)
        {
            if (lowerInput.Contains(pattern))
            {
                Log.Warning("URL encoded SQL pattern detected: {Pattern} in input: {Input}", pattern, input);
                return true;
            }
        }

        foreach (var pattern in hexEncodedPatterns)
        {
            if (lowerInput.Contains(pattern))
            {
                Log.Warning("Hex encoded SQL pattern detected: {Pattern} in input: {Input}", pattern, input);
                return true;
            }
        }

        foreach (var pattern in unicodePatterns)
        {
            if (lowerInput.Contains(pattern))
            {
                Log.Warning("Unicode encoded SQL pattern detected: {Pattern} in input: {Input}", pattern, input);
                return true;
            }
        }

        // Detect base64 encoded dangerous patterns
        if (CouldContainBase64EncodedSql(lowerInput))
        {
            Log.Warning("Base64 encoded SQL pattern detected in input: {Input}", input);
            return true;
        }

        return false;
    }

    // Helper method to check for whole words (reduces false positives)
    private static bool ContainsWord(string input, string word)
    {
        int index = input.IndexOf(word);
        while (index != -1)
        {
            // Check if it's a whole word (preceded by whitespace, start of string, or punctuation)
            if (index == 0 || !char.IsLetterOrDigit(input[index - 1]))
            {
                // Check if it's followed by whitespace, end of string, or punctuation
                int endIndex = index + word.Length;
                if (endIndex >= input.Length || !char.IsLetterOrDigit(input[endIndex]))
                {
                    return true;
                }
            }

            index = input.IndexOf(word, index + 1);
        }
        return false;
    }

    // Detect potential base64 encoded SQL patterns
    private static bool CouldContainBase64EncodedSql(string input)
    {
        // Simple heuristic for base64-like patterns
        var base64Regex = new Regex(@"[A-Za-z0-9+/]{4,}={0,2}");
        var matches = base64Regex.Matches(input);

        foreach (Match match in matches)
        {
            if (match.Length >= 8) // Only check reasonably long base64 strings
            {
                try
                {
                    var bytes = Convert.FromBase64String(match.Value);
                    var decoded = Encoding.UTF8.GetString(bytes);
                    if (ContainsSqlInjectionPatterns(decoded) || ContainsDangerousSqlPatterns(decoded))
                    {
                        Log.Warning("Base64 decoded dangerous content: {DecodedContent} from {EncodedContent}", decoded, match.Value);
                        return true;
                    }
                }
                catch
                {
                    // Not valid base64, continue checking
                }
            }
        }
        return false;
    }

    // Additional utility method for logging and debugging
    public static (bool isSafe, string reason) IsSafeSearchCriteriaWithReason(string criteria)
    {
        if (string.IsNullOrEmpty(criteria))
            return (true, "Empty criteria");

        if (criteria.Length > 500)
        {
            Log.Warning("Search criteria length validation failed: {Length} characters", criteria.Length);
            return (false, "Criteria too long");
        }

        if (!ContainsOnlyAllowedCharacters(criteria))
            return (false, "Contains disallowed characters");

        if (ContainsSqlInjectionPatterns(criteria))
            return (false, "Contains SQL syntax patterns");

        if (ContainsDangerousSqlPatterns(criteria))
            return (false, "Contains dangerous SQL patterns");

        if (ContainsEncodedSqlPatterns(criteria))
            return (false, "Contains encoded SQL patterns");

        Log.Information("Search criteria validation passed: {Criteria}", criteria);
        return (true, "Safe criteria");
    }
}
