using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using smartFinder.Models;

namespace smartFinder.Services
{
    public class FileSearchCriteria
    {
        public string Directory { get; set; }
        public string FilePattern { get; set; } = "*.*";
        public string SearchText { get; set; }
        public bool IncludeSubDirectories { get; set; } = true;
        public bool UseRegex { get; set; }
        public bool MatchCase { get; set; }
        public bool UseAndOperator { get; set; }
    }

    public class FileSearchService
    {
        public async Task<List<SearchResult>> SearchAsync(FileSearchCriteria criteria, CancellationToken cancellationToken, Action<SearchResult> onResultFound = null)
        {
            var results = new ConcurrentBag<SearchResult>();

            if (!Directory.Exists(criteria.Directory))
                throw new DirectoryNotFoundException("Search directory not found.");

            // Split the file patterns by ; only
            var patterns = (criteria.FilePattern ?? "*")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            if (patterns.Count == 0)
            {
                patterns.Add("*");
            }

            // Parse multiple search terms from multi-line text input
            var searchTerms = (criteria.SearchText ?? "")
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();

            // Enumerate all files in the directory tree once
            var allFiles = Directory.EnumerateFiles(criteria.Directory, "*", new EnumerationOptions 
            { 
                IgnoreInaccessible = true, 
                RecurseSubdirectories = criteria.IncludeSubDirectories 
            });

            // Convert wildcard patterns (like *.cs) to Regex for high-performance matching
            var patternRegexes = patterns.Select(p =>
            {
                string regexPattern = "^" + Regex.Escape(p)
                                                  .Replace(@"\*", ".*")
                                                  .Replace(@"\?", ".") + "$";
                return new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }).ToList();

            // Filter files that match at least one of the patterns (with cooperative cancellation)
            var files = allFiles.Where(file =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var fileName = Path.GetFileName(file);
                return patternRegexes.Any(r => r.IsMatch(fileName));
            });

            var regexOptions = criteria.MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase;
            var regexList = new List<Regex>();
            if (criteria.UseRegex && searchTerms.Count > 0)
            {
                foreach (var term in searchTerms)
                {
                    try
                    {
                        regexList.Add(new Regex(term, regexOptions | RegexOptions.Compiled));
                    }
                    catch (Exception)
                    {
                        // Ignore malformed regex
                    }
                }
            }

            await Task.Run(() =>
            {
                Parallel.ForEach(files, new ParallelOptions { CancellationToken = cancellationToken }, file =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        var matchedTermsList = new List<string>();
                        bool isMatch = true;

                        if (searchTerms.Count > 0)
                        {
                            // Check cancellation before heavy file read
                            cancellationToken.ThrowIfCancellationRequested();
                            var content = File.ReadAllText(file);
                            content = CommentStripper.StripCommentsKeepingOffsets(content, Path.GetExtension(file));

                            if (criteria.UseRegex)
                            {
                                for (int i = 0; i < searchTerms.Count; i++)
                                {
                                    if (i >= regexList.Count) break;
                                    var r = regexList[i];
                                    var matches = r.Matches(content);
                                    if (matches.Count > 0)
                                    {
                                        matchedTermsList.Add(searchTerms[i]);
                                    }
                                }
                            }
                            else
                            {
                                var comparison = criteria.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                                for (int i = 0; i < searchTerms.Count; i++)
                                {
                                    var term = searchTerms[i];
                                    if (content.Contains(term, comparison))
                                    {
                                        matchedTermsList.Add(term);
                                    }
                                }
                            }

                            if (criteria.UseAndOperator)
                            {
                                // AND logic: all search terms must be matched
                                isMatch = (matchedTermsList.Count == searchTerms.Count);
                            }
                            else
                            {
                                // OR logic: at least one search term must be matched
                                isMatch = (matchedTermsList.Count > 0);
                            }

                            if (isMatch)
                            {
                                // Split content into lines to find individual line matches
                                string[] lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                                var comparison = criteria.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                                for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    string lineText = lines[lineIdx];
                                    var lineMatchedTerms = new List<string>();
                                    int lineMatchCount = 0;

                                    if (criteria.UseRegex)
                                    {
                                        for (int i = 0; i < searchTerms.Count; i++)
                                        {
                                            if (i >= regexList.Count) break;
                                            var r = regexList[i];
                                            var matches = r.Matches(lineText);
                                            if (matches.Count > 0)
                                            {
                                                lineMatchedTerms.Add(searchTerms[i]);
                                                lineMatchCount += matches.Count;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < matchedTermsList.Count; i++)
                                        {
                                            var term = matchedTermsList[i];
                                            int termMatches = 0;
                                            int idx = 0;
                                            while ((idx = lineText.IndexOf(term, idx, comparison)) != -1)
                                            {
                                                termMatches++;
                                                idx += term.Length;
                                            }

                                            if (termMatches > 0)
                                            {
                                                lineMatchedTerms.Add(term);
                                                lineMatchCount += termMatches;
                                            }
                                        }
                                    }

                                    if (lineMatchCount > 0)
                                    {
                                        var result = new SearchResult(file)
                                        {
                                            LineNo = lineIdx + 1,
                                            LineText = lineText.Trim(),
                                            MatchCount = lineMatchCount,
                                            MatchedTerms = string.Join(", ", lineMatchedTerms)
                                        };
                                        results.Add(result);
                                        onResultFound?.Invoke(result);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // File pattern only search
                            cancellationToken.ThrowIfCancellationRequested();
                            var result = new SearchResult(file)
                            {
                                LineNo = 1,
                                MatchCount = 0,
                                MatchedTerms = ""
                            };
                            results.Add(result);
                            onResultFound?.Invoke(result);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        throw; // Do not swallow cancellation exceptions
                    }
                    catch (Exception)
                    {
                        // Ignore access denied/read errors
                    }
                });
            }, cancellationToken);

            return results.ToList();
        }
    }
}
