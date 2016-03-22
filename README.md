# TeamCityMonitor

Build Monitor based on TeamCitySharp - provides a dashboard listing all failed builds and refreshed every few minutes.

## To build

Needs VS2015 and .net 4.5.2 installed
Run build.ps1 in root

## To deploy

Unzip the built package `_release/TeamcityMonitor.*.nupkg ` into a folder served by IIS or use octopus deploy.

## Features

* Lists broken builds
* Click through to teamcity
* Handles massive teamcity instances with hundreds of builds

## Thanks
Forked from https://github.com/stack72/TeamCityMonitor

## License 
http://stack72.mit-license.org/