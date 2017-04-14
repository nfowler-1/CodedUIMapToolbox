// Guids.cs
// MUST match guids.h
using System;

namespace UIMapToolbox.VSPackage
{
    static class GuidList
    {
        public const string guidUIMapToolbox_VSPackagePkgString = "db004892-daad-459f-9e74-fef38e40a8ec";
        public const string guidUIMapToolbox_VSPackageCmdSetString = "e3fa56a7-51d6-4113-b154-96ae11667290";

        public static readonly Guid guidUIMapToolbox_VSPackageCmdSet = new Guid(guidUIMapToolbox_VSPackageCmdSetString);
    };
}