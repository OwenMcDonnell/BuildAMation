#!/usr/bin/python

from distutils.spawn import find_executable
import os
import sys
from optparse import OptionParser

bam_path = find_executable('bam')
if not bam_path:
  raise RuntimeError('Unable to locate bam')
bam_dir = os.path.dirname(bam_path)
package_tools_dir = os.path.join(bam_dir, 'packagetools')
sys.path.append(package_tools_dir)

from executeprocess import ExecuteProcess
from getpaths import GetBuildAMationPaths
from standardoptions import StandardOptions

parser = OptionParser()
(options, args, extra_args) = StandardOptions(parser)

stdPackageDir, testPackageDir, optionGeneratorExe = GetBuildAMationPaths()

# C compiler options
cCompiler_options = [
    optionGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(stdPackageDir, "ComposerXE", "12", "Scripts", "ICCompilerOptions.cs")),
    "-n=ComposerXE",
    "-c=CCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(stdPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=ComposerXECommon.PrivateData",
    "-e" # this option set derives from the GccCommon option set
]
cCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cCompiler_options, True, True)
print stdout

# C++ compiler options
cxxCompiler_options = [
    optionGeneratorExe,
    "-i=" + os.path.relpath(os.path.join(stdPackageDir, "C", "dev", "Scripts", "ICxxCompilerOptions.cs")),
    "-n=ComposerXE",
    "-c=CxxCompilerOptionCollection",
    "-p", # generate properties
    "-d", # generate delegates
    "-dd=" + os.path.relpath(os.path.join(stdPackageDir, "CommandLineProcessor", "dev", "Scripts", "CommandLineDelegate.cs")),
    "-pv=ComposerXECommon.PrivateData",
    "-e" # this option set derives from the C option set
]
cxxCompiler_options.extend(extra_args)
(stdout,stderr) = ExecuteProcess(cxxCompiler_options, True, True)
print stdout
