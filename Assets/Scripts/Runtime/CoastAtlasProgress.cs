using System;
using System.Collections.Generic;
using Tidepool.Domain;

namespace Tidepool.Runtime
{
    public static class CoastAtlasProgress
    {
        public static CoastAtlasNode FindActiveNode(CoastAtlasDefinition definition, SaveData data)
        {
            if (definition == null)
            {
                return null;
            }

            CoastAtlasNode[] nodes = definition.Nodes;
            if (data != null && !string.IsNullOrWhiteSpace(data.activeExpeditionChapterId))
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i] != null && IdEquals(nodes[i].ChapterId, data.activeExpeditionChapterId))
                    {
                        return nodes[i];
                    }
                }
            }

            return data == null ? null : definition.FindNode(data.currentZone);
        }

        public static CoastAtlasNodeState GetNodeState(CoastAtlasNode node, SaveData data)
        {
            if (node == null)
            {
                return CoastAtlasNodeState.Locked;
            }

            if (data == null)
            {
                return string.IsNullOrWhiteSpace(node.RequiredChapterId)
                    ? CoastAtlasNodeState.Available
                    : CoastAtlasNodeState.Locked;
            }

            if (IsCompleted(node, data))
            {
                return CoastAtlasNodeState.Completed;
            }

            if (IsCurrent(node, data))
            {
                return CoastAtlasNodeState.Current;
            }

            return IsAvailable(node, data)
                ? CoastAtlasNodeState.Available
                : CoastAtlasNodeState.Locked;
        }

        public static int CountCompletedNodes(CoastAtlasDefinition definition, SaveData data)
        {
            if (definition == null || data == null)
            {
                return 0;
            }

            int count = 0;
            CoastAtlasNode[] nodes = definition.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (GetNodeState(nodes[i], data) == CoastAtlasNodeState.Completed)
                {
                    count += 1;
                }
            }

            return count;
        }

        public static string GetNodeSummary(CoastAtlasNode node, SaveData data)
        {
            if (node == null)
            {
                return string.Empty;
            }

            CoastAtlasNodeState state = GetNodeState(node, data);
            switch (state)
            {
                case CoastAtlasNodeState.Completed:
                    return node.CompletedSummary;
                case CoastAtlasNodeState.Locked:
                    return node.LockedHint;
                default:
                    return node.ActiveObjective;
            }
        }

        private static bool IsCompleted(CoastAtlasNode node, SaveData data)
        {
            return ContainsId(data.completedExpeditionChapterIds, node.ChapterId)
                || ContainsId(data.completedSetPieceIds, node.CompletionSetPieceId);
        }

        private static bool IsCurrent(CoastAtlasNode node, SaveData data)
        {
            return IdEquals(node.ChapterId, data.activeExpeditionChapterId)
                || node.Zone == data.currentZone;
        }

        private static bool IsAvailable(CoastAtlasNode node, SaveData data)
        {
            return string.IsNullOrWhiteSpace(node.RequiredChapterId)
                || ContainsId(data.completedExpeditionChapterIds, node.RequiredChapterId);
        }

        private static bool ContainsId(List<string> ids, string id)
        {
            if (ids == null || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            for (int i = 0; i < ids.Count; i++)
            {
                if (IdEquals(ids[i], id))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IdEquals(string left, string right)
        {
            return string.Equals(
                left?.Trim(),
                right?.Trim(),
                StringComparison.Ordinal);
        }
    }
}
