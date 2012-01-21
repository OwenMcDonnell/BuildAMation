﻿// <copyright file="NoCacheAction.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus main application.</summary>
// <author>Mark Final</author>

[assembly: Opus.Core.RegisterAction(typeof(Opus.NoCacheAction))]

namespace Opus
{
    [Core.PreambleAction]
    internal class NoCacheAction : Core.IAction
    {
        public string CommandLineSwitch
        {
            get
            {
                return "-nocache";
            }
        }

        public string Description
        {
            get
            {
                return "Do not create/use the package assembly cache";
            }
        }

        public bool Execute()
        {
            Core.State.CacheAssembly = false;
            return true;
        }
    }
}