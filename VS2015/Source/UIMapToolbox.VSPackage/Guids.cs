// Guids.cs
// MUST match guids.h
using System;

namespace UIMapToolbox.VSPackage
{
    static class GuidList
    {
        public const string guidUIMapToolbox_VSPackagePkgString = "3d0bbd14-d6d6-4fb0-8e13-4bbccf8b9db7";
        public const string guidUIMapToolbox_VSPackageCmdSetString = "8bc36f3e-208c-49a5-9024-70b173478dcc";

        public static readonly Guid guidUIMapToolbox_VSPackageCmdSet = new Guid(guidUIMapToolbox_VSPackageCmdSetString);
    };
}