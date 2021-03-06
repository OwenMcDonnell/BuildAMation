#!/usr/bin/python

import os
import platform
import subprocess
import sys

def restore_nuget():
    print >>sys.stdout, "Restoring NuGet packages"
    sys.stdout.flush()
    if platform.system() == "Linux":
        # nuget Windows executable is downloaded from https://nuget.org/downloads, and run through mono
        nuget_path = subprocess.Popen(["which", "nuget.exe"], stdout=subprocess.PIPE).communicate()[0].rstrip()
        if not nuget_path:
            raise IOError("nuget.exe not found. Does it need downloading, or adding to the PATH?")
        build_args = []
        build_args.append("mono")
        build_args.append(nuget_path)
        build_args.append("restore")
        subprocess.check_call(build_args)
    elif not platform.system() == "Windows":
        build_args = []
        build_args.append("nuget")
        build_args.append("restore")
        subprocess.check_call(build_args)
    # Windows VisualStudio builds does not need to explicitly invoke nuget (and it doesn't exist in a VisualStudio environment unless
    # downloaded on purpose

def build_bam(build_dir, configuration='Release', coveritypath=None, rebuild=False):
    current_dir = os.getcwd()
    try:
        os.chdir(build_dir)
        print >>sys.stdout, "Starting build in %s" % build_dir
        sys.stdout.flush()
        restore_nuget()
        build_args = []
        if coveritypath:
            os.environ["PATH"] += os.pathsep + coveritypath
            build_args.extend(["cov-build", "--dir", "cov-int"])
            if os.path.isdir('cov-int'):
                shutil.rmtree('cov-int')
        if platform.system() == "Windows":
            # assume Visual Studio 2013
            if os.environ.has_key("ProgramFiles(x86)"):
                buildtool = r"C:\Program Files (x86)\MSBuild\12.0\bin\MSBuild.exe"
            else:
                buildtool = r"C:\Program Files\MSBuild\12.0\bin\MSBuild.exe"
            if not os.path.isfile(buildtool):
                raise RuntimeError("Unable to locate msbuild at '%s'" % buildtool)
            build_args.append(buildtool)
        elif platform.system() == "Darwin" or platform.system() == "Linux":
            # xbuild is now redundant
            build_args.append("msbuild")
        else:
            raise RuntimeError("Unrecognized platform, %s" % platform.system())
        build_args.extend(["/property:Configuration=%s" % configuration, "/nologo", "BuildAMation.sln"])
        if rebuild or coveritypath:
            build_args.append("/t:Rebuild")
        print >>sys.stdout, "Running command: %s" % ' '.join(build_args)
        sys.stdout.flush()
        subprocess.check_call(build_args)
        print >>sys.stdout, "Finished build"
        sys.stdout.flush()
    finally:
        os.chdir(current_dir)


if __name__ == "__main__":
    try:
        build_bam(os.getcwd())
    except Exception, e:
        print >>sys.stdout, "*** Build failure reason: %s" % str(e)
        sys.stdout.flush()
    print >>sys.stdout, "Done"
    sys.stdout.flush()
