using System;
using System.Collections.Generic;

namespace Tidepool.Domain
{
    public static class TidelingGrowthForms
    {
        public const string OriginalFormId = "";

        public static void Normalize(CaughtTideling caught)
        {
            if (caught == null)
            {
                return;
            }

            if (caught.rememberedGrowthFormIds == null)
            {
                caught.rememberedGrowthFormIds = new List<string>();
            }
            RemoveInvalidOrDuplicateRememberedForms(caught.rememberedGrowthFormIds);

            caught.activeGrowthFormId = NormalizeFormId(caught.activeGrowthFormId);
            if (!IsOriginal(caught.activeGrowthFormId) && !HasRemembered(caught, caught.activeGrowthFormId))
            {
                caught.activeGrowthFormId = OriginalFormId;
            }
        }

        public static bool Remember(CaughtTideling caught, string formId)
        {
            if (caught == null)
            {
                return false;
            }

            Normalize(caught);
            string normalizedFormId = NormalizeFormId(formId);
            if (IsOriginal(normalizedFormId) || HasRemembered(caught, normalizedFormId))
            {
                return false;
            }

            caught.rememberedGrowthFormIds.Add(normalizedFormId);
            return true;
        }

        public static bool SelectRemembered(CaughtTideling caught, string formId)
        {
            if (caught == null)
            {
                return false;
            }

            Normalize(caught);
            string normalizedFormId = NormalizeFormId(formId);
            if (!IsOriginal(normalizedFormId) && !HasRemembered(caught, normalizedFormId))
            {
                return false;
            }

            if (caught.activeGrowthFormId == normalizedFormId)
            {
                return false;
            }

            caught.activeGrowthFormId = normalizedFormId;
            return true;
        }

        public static bool HasRemembered(CaughtTideling caught, string formId)
        {
            if (caught == null || caught.rememberedGrowthFormIds == null)
            {
                return false;
            }

            string normalizedFormId = NormalizeFormId(formId);
            for (int i = 0; i < caught.rememberedGrowthFormIds.Count; i++)
            {
                if (string.Equals(caught.rememberedGrowthFormIds[i], normalizedFormId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsOriginal(string formId)
        {
            return string.IsNullOrWhiteSpace(formId);
        }

        private static string NormalizeFormId(string formId)
        {
            return string.IsNullOrWhiteSpace(formId) ? OriginalFormId : formId.Trim();
        }

        private static void RemoveInvalidOrDuplicateRememberedForms(List<string> formIds)
        {
            for (int i = formIds.Count - 1; i >= 0; i--)
            {
                string normalizedFormId = NormalizeFormId(formIds[i]);
                if (IsOriginal(normalizedFormId) || ContainsEarlier(formIds, normalizedFormId, i))
                {
                    formIds.RemoveAt(i);
                    continue;
                }

                formIds[i] = normalizedFormId;
            }
        }

        private static bool ContainsEarlier(List<string> formIds, string formId, int beforeIndex)
        {
            for (int i = 0; i < beforeIndex; i++)
            {
                if (string.Equals(formIds[i], formId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
