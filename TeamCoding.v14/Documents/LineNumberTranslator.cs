using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCoding.Documents.SourceControlRepositories;

namespace TeamCoding.Documents
{
    public static class LineNumberTranslator
    {
        public static int GetLineNumber(string[] local, string[] remote, int lineNumber, FileNumberBasis targetBasis)
        {
            var changes = Microsoft.TeamFoundation.Diff.DiffFinder<string>.LcsDiff.Diff(remote,
                                                                                        local,
                                                                                        EqualityComparer<string>.Default);

            var localLineNumber = 0;
            var serverLineNumber = 0;


            if (targetBasis == FileNumberBasis.Server)
            {
                foreach (var change in changes)
                {
                    if (lineNumber < change.ModifiedStart)
                    {
                        break;
                    }
                    else if (lineNumber > change.ModifiedEnd)
                    {
                        localLineNumber = change.ModifiedEnd;
                        serverLineNumber = change.OriginalEnd;
                    }
                    else
                    {
                        return change.OriginalStart;
                    }
                }

                return lineNumber + (serverLineNumber - localLineNumber);
            }
            else
            {
                foreach (var change in changes)
                {
                    if (lineNumber < change.OriginalStart)
                    {
                        break;
                    }
                    else if (lineNumber > change.OriginalEnd)
                    {
                        localLineNumber = change.ModifiedEnd;
                        serverLineNumber = change.OriginalEnd;
                    }
                    else
                    {
                        return change.ModifiedStart;
                    }
                }

                return lineNumber + (localLineNumber - serverLineNumber);
            }
        }
    }
}
