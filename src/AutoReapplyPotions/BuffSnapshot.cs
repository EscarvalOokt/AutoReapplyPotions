using System;
using System.Collections.Generic;

namespace AutoReapplyPotions
{
    internal struct BuffSnapshot
    {
        private int _firstBuffId;
        private List<int> _additionalBuffIds;

        public int Count => _firstBuffId == 0 ? 0 : 1 + (_additionalBuffIds?.Count ?? 0);

        public int this[int index] => index == 0 ? _firstBuffId : _additionalBuffIds[index - 1];

        public static BuffSnapshot Capture(int[] buffTypes, int[] buffTimes)
        {
            var candidates = new BuffSnapshot();
            int count = Math.Min(buffTypes.Length, buffTimes.Length);
            for (var i = 0; i < count; i++)
            {
                if (buffTimes[i] == 1 && buffTypes[i] > 0)
                    candidates.Add(buffTypes[i]);
            }

            return candidates;
        }

        public BuffSnapshot FindExpired(int[] buffTypes, int[] buffTimes, BuffSnapshot previouslyAtZero = default)
        {
            var expired = new BuffSnapshot();
            for (var i = 0; i < Count; i++)
            {
                int buffId = this[i];
                if (HasReachedZero(buffId, buffTypes, buffTimes, previouslyAtZero.Contains(buffId)))
                    expired.Add(buffId);
            }

            return expired;
        }

        private static bool HasReachedZero(int buffId, int[] buffTypes, int[] buffTimes, bool reachedZero)
        {
            // DelBuff can compact a zero-time slot before the UpdateBuffs postfix runs.
            // A positive-time copy takes precedence even if an earlier copy expired.
            int count = Math.Min(buffTypes.Length, buffTimes.Length);
            for (var i = 0; i < count; i++)
            {
                if (buffTypes[i] != buffId)
                    continue;

                if (buffTimes[i] > 0)
                    return false;

                if (buffTimes[i] == 0)
                    reachedZero = true;
            }

            return reachedZero;
        }

        private bool Contains(int buffId)
        {
            return _firstBuffId == buffId || (_additionalBuffIds?.Contains(buffId) ?? false);
        }

        private void Add(int buffId)
        {
            if (_firstBuffId == 0)
            {
                _firstBuffId = buffId;
                return;
            }

            if (buffId != _firstBuffId)
            {
                _additionalBuffIds ??= new List<int>();
                if (!_additionalBuffIds.Contains(buffId))
                    _additionalBuffIds.Add(buffId);
            }
        }
    }
}