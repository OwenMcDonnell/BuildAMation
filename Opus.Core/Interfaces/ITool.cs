﻿// <copyright file="ITool.cs" company="Mark Final">
//  Opus
// </copyright>
// <summary>Opus Core</summary>
// <author>Mark Final</author>
namespace Opus.Core
{
    public interface ITool
    {
        string
        Executable(
            BaseTarget baseTarget);

        Array<LocationKey>
        OutputLocationKeys(
            Opus.Core.BaseModule module);
    }
}
