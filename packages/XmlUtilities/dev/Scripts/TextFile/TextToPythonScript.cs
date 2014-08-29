#region License
// Copyright 2010-2014 Mark Final
//
// This file is part of BuildAMation.
//
// BuildAMation is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// BuildAMation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with BuildAMation.  If not, see <http://www.gnu.org/licenses/>.
#endregion // License
namespace XmlUtilities
{
    public static class TextToPythonScript
    {
        public static void
        Write(
            System.Text.StringBuilder content,
            string pythonScriptPath,
            string pathToGeneratedFile)
        {
            using (var writer = new System.IO.StreamWriter(pythonScriptPath))
            {
                writer.WriteLine("#!usr/bin/python");

                writer.WriteLine(System.String.Format("with open(r'{0}', 'wt') as script:", pathToGeneratedFile));
                foreach (var line in content.ToString().Split('\n'))
                {
                    writer.WriteLine("\tscript.write('{0}\\n')", line);
                }
            }
        }
    }
}
