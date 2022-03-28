#region "copyright"

/*
    Copyright © 2016 - 2022 Stefan Berg <isbeorn86+NINA@googlemail.com> and the N.I.N.A. contributors

    This file is part of N.I.N.A. - Nighttime Imaging 'N' Astronomy.

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

#endregion "copyright"

using System.Reflection;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCompany("")]
[assembly: AssemblyCopyright("Copyright ©  2016 - 2022 Stefan Berg and the N.I.N.A. contributors")]
[assembly: AssemblyProduct("N.I.N.A. - Nighttime Imaging 'N' Astronomy")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

//Versioning in N.I.N.A.
//N.I.N.A. utilizes the versioning scheme MAJOR.MINOR.PATCH.CHANNEL|BUILDNRXXX
//There is currently no automation used and versions are maintained manually.

//MAJOR version increases for big changes, like changing technologies etc.

//MINOR version will increase for every new released version

//PATCH version is reserved to apply Hotfixes to a released versions

//CHANNEL|BUILDNR will not be displayed for Released versions, as these are only used to identify Release, RC, Beta and Develop versions

//CHANNEL consists of the following values:
//1: Nightly
//2: Beta
//3: Release Candidate
//9: Release

//BUILDNR should be incremented each nightly build (only in develop, beta and rc versions) by using 3 digits.

//Examples:
//Release: 1.8.0.9001 (Displayed as "1.8")
//Release: 1.8.1.9001 (Displayed as "1.8 HF1")
//Release Candidate: 1.8.0.3001 (Displayed as "1.8 RC1")
//Beta: 1.8.0.2004 (Displayed as "1.8 BETA004")
//Develop: 1.8.0.1022 (Displayed as "1.8 NIGHTLY #022")
[assembly: AssemblyVersion("2.0.0.2056")]
[assembly: AssemblyFileVersion("2.0.0.2056")]
[assembly: AssemblyInformationalVersion("2.0.0.2056-beta")]
