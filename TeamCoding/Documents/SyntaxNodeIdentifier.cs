using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamCoding.Documents
{
    [ProtoBuf.ProtoContract]
    public class SyntaxNodeIdentifier
    {
        public static class Cache
        {
            private static readonly ConcurrentDictionary<SyntaxNode, SyntaxNodeIdentifier> CachedIdentifiers = new ConcurrentDictionary<SyntaxNode, SyntaxNodeIdentifier>();
            public static SyntaxNodeIdentifier GetIdentifier(SyntaxNode node)
            {
                SyntaxNodeIdentifier hash;
                if (CachedIdentifiers.TryGetValue(node, out hash))
                {
                    return hash;
                }

                hash = new SyntaxNodeIdentifier(node.AncestorsAndSelf().Select(a => a.GetHashCode()).ToArray());

                CachedIdentifiers.AddOrUpdate(node, hash, (n, e) => e);
                return hash;
            }
            public static void RemoveCachedIdentifier(SyntaxNode node)
            {
                SyntaxNodeIdentifier _;
                CachedIdentifiers.TryRemove(node, out _);
            }
        }
        [ProtoBuf.ProtoMember(1)]
        public int[] Id;
        public SyntaxNodeIdentifier() { } // For protobuf
        public SyntaxNodeIdentifier(int[] id) { Id = id; }
        public override bool Equals(object obj)
        {
            var syntaxNodeIdentifier = obj as SyntaxNodeIdentifier;
            if (syntaxNodeIdentifier == null) { return false; }
            return Id.SequenceEqual(syntaxNodeIdentifier.Id);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return Id.Aggregate(17, (acc, next) => acc * 31 + next);
            }
        }
    }
}
