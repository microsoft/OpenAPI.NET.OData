//---------------------------------------------------------------------
// <copyright file="AssemblyInfoCommon.cs" company="Microsoft">
//      Copyright (C) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.
// </copyright>
//---------------------------------------------------------------------

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;

// If you want to control this metadata globally but not with the VersionProductName property, hard-code the value below.
// If you want to control this metadata at the individual project level with AssemblyInfo.cs, comment-out the line below.
// If you leave the line below unchanged, make sure to set the property in the root build.props, e.g.: <VersionProductName Condition="'$(VersionProductName)'==''">Your Product Name</VersionProductName>
// [assembly: AssemblyProduct("%VersionProductName%")]

[assembly: AssemblyCompany("Microsoft Corporation")]

[assembly: AssemblyProduct("Microsoft® .NET/.NET Core OData Extensions")]

[assembly: AssemblyCopyright("Copyright (c) Microsoft Corporation. All rights reserved.")]

[assembly: AssemblyTrademark("Microsoft and Windows are either registered trademarks or trademarks of Microsoft Corporation in the U.S. and/or other countries.")]

[assembly: AssemblyCulture("")]

#if (DEBUG || _DEBUG)
[assembly: AssemblyConfiguration("Debug")]
#endif

// disable CLSCompliant as the common extensions library decided not to support this
// see https://github.com/aspnet/AspNetCore/issues/2689#issuecomment-354693946
[assembly: CLSCompliant(false)]

#if ASSEMBLY_ATTRIBUTE_COM_VISIBLE
[assembly: ComVisible(true)]
#else
[assembly: ComVisible(false)]
#endif

#if ASSEMBLY_ATTRIBUTE_COM_COMPATIBLE_SIDEBYSIDE
[assembly:ComCompatibleVersion(1,0,3300,0)]
#endif

#if ASSEMBLY_ATTRIBUTE_ALLOW_PARTIALLY_TRUSTED_CALLERS
[assembly: AllowPartiallyTrustedCallers]
#else
#if ASSEMBLY_ATTRIBUTE_CONDITIONAL_APTCA_L2
[assembly:AllowPartiallyTrustedCallers(PartialTrustVisibilityLevel=PartialTrustVisibilityLevel.NotVisibleByDefault)]
#endif
#endif

#if ASSEMBLY_ATTRIBUTE_TRANSPARENT_ASSEMBLY
[assembly: SecurityTransparent]
#endif

#if !SUPPRESS_SECURITY_RULES
#if SECURITY_MIGRATION && !ASSEMBLY_ATTRIBUTE_CONDITIONAL_APTCA_L2
#if ASSEMBLY_ATTRIBUTE_SKIP_VERIFICATION_IN_FULLTRUST
[assembly: SecurityRules(SecurityRuleSet.Level1, SkipVerificationInFullTrust = true)]
#else
[assembly: SecurityRules(SecurityRuleSet.Level1)]
#endif
#else
#if ASSEMBLY_ATTRIBUTE_SKIP_VERIFICATION_IN_FULLTRUST
[assembly: SecurityRules(SecurityRuleSet.Level2, SkipVerificationInFullTrust = true)]
#else
[assembly: SecurityRules(SecurityRuleSet.Level2)]
#endif
#endif
#endif

[assembly:NeutralResourcesLanguageAttribute("en-US")]
