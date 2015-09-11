#!/usr/bin/python

import sys
import os
import subprocess
import StringIO
import time
from testconfigurations import GetTestConfig, TestOptionSetup, GetResponsePath
from builderactions import GetBuilderDetails
from optparse import OptionParser

# ----------

class Package:
    root = None
    name = None
    version = None

    def __init__(self, root, name, version):
        self.root = root
        self.name = name
        self.version = version

    def GetDescription(self):
        return "%s-%s in %s" % (self.name, self.version, self.root)

    def GetPath(self):
        return os.path.join(self.root, self.name, self.version)

    def GetId(self):
        return "-".join([self.name, self.version])

    def GetName(self):
        return self.name

# ----------

def FindAllPackagesToTest(root, options):
    """Locate packages that can be tested"""
    if options.verbose:
        print "Locating packages under '%s'" % root
    tests = []
    dirs = os.listdir(root)
    dirs.sort()
    for packageName in dirs:
        if packageName.startswith("."):
            continue
        packageDir = os.path.join(root, packageName)
        if os.path.isdir(packageDir):
            for packageVersion in os.listdir(packageDir):
                if packageVersion.startswith("."):
                    continue
                versionDir = os.path.join(packageDir, packageVersion)
                if os.path.isdir(versionDir):
                    package = Package(root, packageName, packageVersion)
                    if options.verbose:
                        print "\t%s" % package.GetId()
                    tests.append(package)
    return tests

def _preExecute(builder, options):
    if builder.preAction:
        builder.preAction()

def _runBuildAMation(options, package, responseFile, extraArgs, outputMessages, errorMessages):
    argList = []
    argList.append("bam")
    if responseFile:
        argList.append("@" + os.path.join(os.getcwd(), responseFile))
    argList.append("-buildroot=" + options.buildRoot)
    argList.append("-builder=" + options.builder)
    if sys.platform.startswith("win"):
        argList.append("-platforms=" + ";".join(options.platforms))
        argList.append("-configurations=" + ";".join(options.configurations))
    else:
        argList.append("-platforms=" + ":".join(options.platforms))
        argList.append("-configurations=" + ":".join(options.configurations))
    argList.append("-j=" + str(options.numJobs))
    if options.debugSymbols:
        argList.append("-debugsymbols")
    if options.verbose:
        argList.append("-verbosity=2")
    else:
        argList.append("-verbosity=0")
    if options.forceDefinitionUpdate:
        argList.append("-forcedefinitionupdate")
    if extraArgs:
        argList.extend(extraArgs)
    print " ".join(argList)
    p = subprocess.Popen(argList, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=package.GetPath())
    (outputStream, errorStream) = p.communicate() # this should WAIT
    if outputStream:
        outputMessages.write(outputStream)
    if errorStream:
        errorMessages.write(errorStream)
    return (p.returncode, argList)

def _postExecute(builder, options, package, outputMessages, errorMessages):
    if builder.postAction:
        exitCode = builder.postAction(package, options, outputMessages, errorMessages)
        return exitCode
    return 0

def ExecuteTests(package, configuration, options, args, outputBuffer):
    print "Package           : ", package.GetId()
    if options.verbose:
        print "Description       : ", package.GetDescription()
        print "Available builders:", configuration.GetBuilders()
    if not options.builder in configuration.GetBuilders():
        outputBuffer.write("IGNORED: Package '%s' does not support the builder '%s' in the test configuration\n" % (package.GetDescription(),options.builder))
        print "\tIgnored"
        return 0
    responseNames = configuration.GetResponseNames(options.builder, options.excludeResponseFiles)
    if len(responseNames) == 0:
        outputBuffer.write("IGNORED: Package '%s' has no response file with the current options\n" % package.GetDescription())
        print "\tIgnored"
        return 0
    if options.verbose:
        print "Response filenames: ", responseNames
        if options.excludeResponseFiles:
          print " (excluding", options.excludeResponseFiles, ")"
    nonKWArgs = []
    for arg in args:
        if '=' in arg:
            argSplit = arg.split('=')
            if argSplit[0].endswith('.version'):
                nonKWArgs.append("-%s" % arg)
    theBuilder = GetBuilderDetails(options.builder)
    exitCode = 0
    for responseName in responseNames:
        currentDir = os.getcwd()
        iterations = 1
        if responseName:
            responseFile = GetResponsePath(responseName)
            versionName = "%s_version" % responseName
            versionArgs = None
            if hasattr(options, versionName):
              versionArgs = getattr(options, versionName)
            if versionArgs:
              iterations = len(versionArgs)
        else:
            responseFile = None
            versionArgs = None

        for it in range(0,iterations):
            extraArgs = nonKWArgs[:]
            if versionArgs:
                extraArgs = [ "-%s.version=%s" % (responseName,versionArgs[it]) ]
            try:
              outputMessages = StringIO.StringIO()
              errorMessages = StringIO.StringIO()
              _preExecute(theBuilder, options)
              returncode, argList = _runBuildAMation(options, package, responseFile, extraArgs, outputMessages, errorMessages)
              if returncode == 0:
                returncode = _postExecute(theBuilder, options, package, outputMessages, errorMessages)
            except Exception, e:
                print "Popen exception: '%s'" % str(e)
                raise
            finally:
                os.chdir(currentDir)
                message = "Package '%s' with response file '%s'" % (package.GetDescription(), responseFile)
                if extraArgs:
                  message += " with extra arguments '%s'" % " ".join(extraArgs)
                if returncode == 0:
                    outputBuffer.write("SUCCESS: %s\n" % message)
                    if options.verbose:
                        if len(outputMessages.getvalue()) > 0:
                            outputBuffer.write("Messages:\n")
                            outputBuffer.write(outputMessages.getvalue())
                        if len(errorMessages.getvalue()) > 0:
                            outputBuffer.write("Errors:\n")
                            outputBuffer.write(errorMessages.getvalue())
                else:
                    outputBuffer.write("* FAILURE *: %s\n" % message)
                    outputBuffer.write("Command was: '%s'\n" % " ".join(argList))
                    if len(outputMessages.getvalue()) > 0:
                        outputBuffer.write("Messages:\n")
                        outputBuffer.write(outputMessages.getvalue())
                    if len(errorMessages.getvalue()) > 0:
                        outputBuffer.write("Errors:\n")
                        outputBuffer.write(errorMessages.getvalue())
                    outputBuffer.write("\n")
                    exitCode = exitCode - 1
    return exitCode

def CleanUp(options):
    argList = []
    if sys.platform.startswith("win"):
        argList.append(os.path.join(os.getcwd(), "removedebugprojects.bat"))
        argList.append("-nopause")
    else:
        argList.append(os.path.join(os.getcwd(), "removedebugprojects.sh"))
    if options.verbose:
        print "Executing: ", argList
    p = subprocess.Popen(argList)
    p.wait()

# ----------

if __name__ == "__main__":
    optParser = OptionParser(description="BuildAMation unittests")
    optParser.add_option("--platform", "-p", dest="platforms", action="append", default=None, help="Platforms to test")
    optParser.add_option("--configuration", "-c", dest="configurations", action="append", default=None, help="Configurations to test")
    optParser.add_option("--test", "-t", dest="tests", action="append", default=None, help="Tests to run")
    optParser.add_option("--buildroot", "-o", dest="buildRoot", action="store", default="build", help="BuildAMation build root")
    optParser.add_option("--builder", "-b", dest="builder", action="store", default="Native", help="BuildAMation builder to test")
    optParser.add_option("--keepfiles", "-k", dest="keepFiles", action="store_true", default=False, help="Keep the BuildAMation build files around")
    optParser.add_option("--jobs", "-j", dest="numJobs", action="store", type="int", default=1, help="Number of jobs to use with BuildAMation builds")
    optParser.add_option("--verbose", "-v", dest="verbose", action="store_true", default=False, help="Verbose output")
    optParser.add_option("--debug", "-d", dest="debugSymbols", action="store_true", default=False, help="Build BuildAMation packages with debug information")
    optParser.add_option("--noinitialclean", "-i", dest="noInitialClean", action="store_true", default=False, help="Disable cleaning packages before running tests")
    optParser.add_option("--forcedefinitionupdate", "-f", dest="forceDefinitionUpdate", action="store_true", default=False, help="Force definition file updates")
    optParser.add_option("--excluderesponsefiles", "-x", dest="excludeResponseFiles", action="append", default=None, help="Exclude response files")
    TestOptionSetup(optParser)
    (options,args) = optParser.parse_args()

    if options.verbose:
        print "Options are ", options
        print "Args    are ", args

    if not options.platforms:
        raise RuntimeError("No platforms were specified")

    if not options.configurations:
        raise RuntimeError("No configurations were specified")

    if not options.noInitialClean:
        CleanUp(options)

    tests = FindAllPackagesToTest(os.getcwd(), options)
    if not options.tests:
        if options.verbose:
            print "All tests will run"
    else:
        if options.verbose:
            print "Tests to run are: ", options.tests
        filteredTests = []
        for test in options.tests:
            found = False
            for package in tests:
                if package.GetId() == test:
                    filteredTests.append(package)
                    found = True
                    break
            if not found:
                raise RuntimeError("Unrecognized package '%s'" % test)
        tests = filteredTests

    outputBuffer = StringIO.StringIO()
    exitCode = 0
    for package in tests:
        config = GetTestConfig(package.GetId(), options)
        if not config:
            continue
        exitCode += ExecuteTests(package, config, options, args, outputBuffer)

    if not options.keepFiles:
        # TODO: consider keeping track of all directories created instead
        CleanUp(options)

    print "--------------------"
    print "| Results summary  |"
    print "--------------------"
    print outputBuffer.getvalue()

    if not os.path.exists("Logs"):
        os.mkdir("Logs")
    logFileName = os.path.join("Logs", "tests_" + time.strftime("%d-%m-%YT%H-%M-%S") + ".log")
    logFile = open(logFileName, "w")
    logFile.write(outputBuffer.getvalue())
    logFile.close()
    outputBuffer.close()

    sys.exit(exitCode)