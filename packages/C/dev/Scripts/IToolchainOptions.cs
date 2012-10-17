// <copyright file="IToolchainOptions.cs" company="Mark Final">
//  Opus package
// </copyright>
// <summary>C package</summary>
// <author>Mark Final</author>
namespace C
{
    public interface IToolchainOptions
    {
        bool IsCPlusPlus
        {
            get;
            set;
        }

#if false
        C.ECharacterSet CharacterSet
        {
            get;
            set;
        }
#endif
    }
}