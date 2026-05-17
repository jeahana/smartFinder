using System;
using System.IO;

namespace smartFinder.Services
{
    public static class CommentStripper
    {
        public static string StripCommentsKeepingOffsets(string content, string extension)
        {
            if (string.IsNullOrEmpty(content)) return content;

            string ext = (extension ?? "").ToLowerInvariant();
            char[] chars = content.ToCharArray();
            int len = chars.Length;

            // Group 1: C-style languages (.cs, .cpp, .c, .h, .hpp, .js, .ts, .java, .css, .php, .go, .rs, .kt, .scala, .swift, etc.)
            bool isCStyle = ext == ".cs" || ext == ".cpp" || ext == ".c" || ext == ".h" || ext == ".hpp" || 
                            ext == ".js" || ext == ".ts" || ext == ".java" || ext == ".css" || ext == ".php" || 
                            ext == ".go" || ext == ".rs" || ext == ".kt" || ext == ".scala" || ext == ".swift";

            // Group 2: Scripting (.py, .sh, .rb, .pl, .r, .yaml, .yml, .ini, .conf, .properties)
            bool isHashStyle = ext == ".py" || ext == ".sh" || ext == ".rb" || ext == ".pl" || ext == ".r" || 
                               ext == ".yaml" || ext == ".yml" || ext == ".ini" || ext == ".conf" || 
                               ext == ".properties" || ext == ".ps1";

            // Group 3: SQL
            bool isSqlStyle = ext == ".sql";

            // Group 4: VB
            bool isVbStyle = ext == ".vb";

            // Group 5: XML / HTML / XAML
            bool isXmlStyle = ext == ".xml" || ext == ".html" || ext == ".xaml" || ext == ".svg" || 
                              ext == ".csproj" || ext == ".config" || ext == ".props" || ext == ".targets";

            if (isCStyle)
            {
                bool inString = false;
                bool inChar = false;
                bool inVerbatim = false;
                bool isEscaped = false;

                for (int i = 0; i < len; i++)
                {
                    if (inString)
                    {
                        if (isEscaped) { isEscaped = false; continue; }
                        if (chars[i] == '\\') { isEscaped = true; }
                        else if (chars[i] == '"') { inString = false; }
                        continue;
                    }
                    if (inChar)
                    {
                        if (isEscaped) { isEscaped = false; continue; }
                        if (chars[i] == '\\') { isEscaped = true; }
                        else if (chars[i] == '\'') { inChar = false; }
                        continue;
                    }
                    if (inVerbatim)
                    {
                        if (i + 1 < len && chars[i] == '"' && chars[i + 1] == '"')
                        {
                            i++; // skip second quote
                            continue;
                        }
                        if (chars[i] == '"') { inVerbatim = false; }
                        continue;
                    }

                    // Check for single line comment: //
                    if (i + 1 < len && chars[i] == '/' && chars[i + 1] == '/')
                    {
                        // replace with spaces until newline
                        while (i < len && chars[i] != '\n' && chars[i] != '\r')
                        {
                            chars[i] = ' ';
                            i++;
                        }
                        if (i < len) i--; // let loop handle newline character
                        continue;
                    }

                    // Check for multi-line comment: /*
                    if (i + 1 < len && chars[i] == '/' && chars[i + 1] == '*')
                    {
                        chars[i] = ' ';
                        chars[i + 1] = ' ';
                        i += 2;
                        while (i < len)
                        {
                            if (i + 1 < len && chars[i] == '*' && chars[i + 1] == '/')
                            {
                                chars[i] = ' ';
                                chars[i + 1] = ' ';
                                i++;
                                break;
                            }
                            if (chars[i] != '\r' && chars[i] != '\n')
                            {
                                chars[i] = ' ';
                            }
                            i++;
                        }
                        continue;
                    }

                    // Start of string / char literals
                    if (ext == ".cs" && i + 1 < len && chars[i] == '@' && chars[i + 1] == '"')
                    {
                        inVerbatim = true;
                        i++;
                        continue;
                    }
                    if (chars[i] == '"') { inString = true; isEscaped = false; }
                    else if (chars[i] == '\'') { inChar = true; isEscaped = false; }
                }
            }
            else if (isHashStyle)
            {
                bool inSingleString = false;
                bool inDoubleString = false;
                bool isEscaped = false;

                // PowerShell multi-line comment: <# ... #>
                bool isPowerShell = ext == ".ps1";

                for (int i = 0; i < len; i++)
                {
                    if (inSingleString)
                    {
                        if (isEscaped) { isEscaped = false; continue; }
                        if (chars[i] == '\\') { isEscaped = true; }
                        else if (chars[i] == '\'') { inSingleString = false; }
                        continue;
                    }
                    if (inDoubleString)
                    {
                        if (isEscaped) { isEscaped = false; continue; }
                        if (chars[i] == '\\') { isEscaped = true; }
                        else if (chars[i] == '"') { inDoubleString = false; }
                        continue;
                    }

                    // PowerShell multi-line comment: <# ... #>
                    if (isPowerShell && i + 1 < len && chars[i] == '<' && chars[i + 1] == '#')
                    {
                        chars[i] = ' ';
                        chars[i + 1] = ' ';
                        i += 2;
                        while (i < len)
                        {
                            if (i + 1 < len && chars[i] == '#' && chars[i + 1] == '>')
                            {
                                chars[i] = ' ';
                                chars[i + 1] = ' ';
                                i++;
                                break;
                            }
                            if (chars[i] != '\r' && chars[i] != '\n')
                            {
                                chars[i] = ' ';
                            }
                            i++;
                        }
                        continue;
                    }

                    // Python triple quotes multiline comment/string: """ or '''
                    if (ext == ".py" && i + 2 < len && chars[i] == '"' && chars[i + 1] == '"' && chars[i + 2] == '"')
                    {
                        chars[i] = ' '; chars[i + 1] = ' '; chars[i + 2] = ' ';
                        i += 3;
                        while (i < len)
                        {
                            if (i + 2 < len && chars[i] == '"' && chars[i + 1] == '"' && chars[i + 2] == '"')
                            {
                                chars[i] = ' '; chars[i + 1] = ' '; chars[i + 2] = ' ';
                                i += 2;
                                break;
                            }
                            if (chars[i] != '\r' && chars[i] != '\n')
                            {
                                chars[i] = ' ';
                            }
                            i++;
                        }
                        continue;
                    }
                    if (ext == ".py" && i + 2 < len && chars[i] == '\'' && chars[i + 1] == '\'' && chars[i + 2] == '\'')
                    {
                        chars[i] = ' '; chars[i + 1] = ' '; chars[i + 2] = ' ';
                        i += 3;
                        while (i < len)
                        {
                            if (i + 2 < len && chars[i] == '\'' && chars[i + 1] == '\'' && chars[i + 2] == '\'')
                            {
                                chars[i] = ' '; chars[i + 1] = ' '; chars[i + 2] = ' ';
                                i += 2;
                                break;
                            }
                            if (chars[i] != '\r' && chars[i] != '\n')
                            {
                                chars[i] = ' ';
                            }
                            i++;
                        }
                        continue;
                    }

                    // Check for single line comment: #
                    if (chars[i] == '#')
                    {
                        while (i < len && chars[i] != '\n' && chars[i] != '\r')
                        {
                            chars[i] = ' ';
                            i++;
                        }
                        if (i < len) i--;
                        continue;
                    }

                    if (chars[i] == '"') { inDoubleString = true; isEscaped = false; }
                    else if (chars[i] == '\'') { inSingleString = true; isEscaped = false; }
                }
            }
            else if (isSqlStyle)
            {
                bool inSingleString = false;
                bool inDoubleString = false;

                for (int i = 0; i < len; i++)
                {
                    if (inSingleString)
                    {
                        if (chars[i] == '\'')
                        {
                            if (i + 1 < len && chars[i + 1] == '\'') { i++; continue; } // escaped quote
                            inSingleString = false;
                        }
                        continue;
                    }
                    if (inDoubleString)
                    {
                        if (chars[i] == '"') { inDoubleString = false; }
                        continue;
                    }

                    // Single line comment: --
                    if (i + 1 < len && chars[i] == '-' && chars[i + 1] == '-')
                    {
                        while (i < len && chars[i] != '\n' && chars[i] != '\r')
                        {
                            chars[i] = ' ';
                            i++;
                        }
                        if (i < len) i--;
                        continue;
                    }

                    // Multi-line comment: /*
                    if (i + 1 < len && chars[i] == '/' && chars[i + 1] == '*')
                    {
                        chars[i] = ' ';
                        chars[i + 1] = ' ';
                        i += 2;
                        while (i < len)
                        {
                            if (i + 1 < len && chars[i] == '*' && chars[i + 1] == '/')
                            {
                                chars[i] = ' ';
                                chars[i + 1] = ' ';
                                i++;
                                break;
                            }
                            if (chars[i] != '\r' && chars[i] != '\n')
                            {
                                chars[i] = ' ';
                            }
                            i++;
                        }
                        continue;
                    }

                    if (chars[i] == '\'') inSingleString = true;
                    else if (chars[i] == '"') inDoubleString = true;
                }
            }
            else if (isVbStyle)
            {
                bool inString = false;

                for (int i = 0; i < len; i++)
                {
                    if (inString)
                    {
                        if (chars[i] == '"')
                        {
                            if (i + 1 < len && chars[i + 1] == '"') { i++; continue; }
                            inString = false;
                        }
                        continue;
                    }

                    // Single line comment: '
                    if (chars[i] == '\'')
                    {
                        while (i < len && chars[i] != '\n' && chars[i] != '\r')
                        {
                            chars[i] = ' ';
                            i++;
                        }
                        if (i < len) i--;
                        continue;
                    }

                    if (chars[i] == '"') inString = true;
                }
            }
            else if (isXmlStyle)
            {
                // XML Comment: <!-- ... -->
                for (int i = 0; i < len; i++)
                {
                    if (i + 3 < len && chars[i] == '<' && chars[i + 1] == '!' && chars[i + 2] == '-' && chars[i + 3] == '-')
                    {
                        chars[i] = ' '; chars[i + 1] = ' '; chars[i + 2] = ' '; chars[i + 3] = ' ';
                        i += 4;
                        while (i < len)
                        {
                            if (i + 2 < len && chars[i] == '-' && chars[i + 1] == '-' && chars[i + 2] == '>')
                            {
                                chars[i] = ' '; chars[i + 1] = ' '; chars[i + 2] = ' ';
                                i += 2;
                                break;
                            }
                            if (chars[i] != '\r' && chars[i] != '\n')
                            {
                                chars[i] = ' ';
                            }
                            i++;
                        }
                    }
                }
            }

            return new string(chars);
        }
    }
}
