// Guids.cs
// MUST match guids.h
using System;

namespace Company.FastSwitchFile
{
    static class GuidList
    {
        public const string guidSwitchFilesPkgString = "c77212ed-f134-47bf-95af-5b4053a2bbba";
        public const string guidSwitchFilesCmdSetString = "ba5461c4-b354-4af2-bdb0-d40aa743968a";

        public static readonly Guid guidSwitchFilesCmdSet = new Guid(guidSwitchFilesCmdSetString);
    };
}