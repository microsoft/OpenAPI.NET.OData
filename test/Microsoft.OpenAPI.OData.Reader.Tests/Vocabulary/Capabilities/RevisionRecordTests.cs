// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// ------------------------------------------------------------

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Vocabularies;
using Microsoft.OpenApi.OData.Vocabulary.Core;
using Xunit;

namespace Microsoft.OpenApi.OData.Reader.Vocabulary.Capabilities.Tests;

public class RevisionRecordTests
{
    [Fact]
    public void DefaultPropertyAsNull()
    {
        // Arrange & Act
        RevisionRecord revision = new();

        // Assert
        Assert.Null(revision.Date);
        Assert.Null(revision.RemovalDate);
        Assert.Null(revision.Description);
        Assert.Null(revision.Version);
        Assert.Null(revision.Kind);
    }
    [Fact]
    public void InitializeWithNullRecordThrows()
    {
        // Arrange & Act
        RevisionRecord revision = new();

        // Assert
        Assert.Throws<ArgumentNullException>("record", () => revision.Initialize(record: null));
    }
    private readonly static EdmEnumType enumType = new("Org.OData.Core.V1", "RevisionKind");
    private readonly static EdmEnumMember deprecatedValue = new (enumType, "Deprecated", new EdmEnumMemberValue(2));
    private readonly static EdmEnumMember addedValue = new (enumType, "Added", new EdmEnumMemberValue(0));
    [Fact]
    public void InitializeWithDeprecatedRevisionsTypeRecordSuccess()
    {
        // Arrange
        IEdmRecordExpression record = new EdmRecordExpression(
            new EdmPropertyConstructor("Date", new EdmDateConstant(new Date(2021, 8, 24))),
            new EdmPropertyConstructor("RemovalDate", new EdmDateConstant(new Date(2021, 10, 24))),
            new EdmPropertyConstructor("Kind", new EdmEnumMemberExpression(deprecatedValue)), 
            new EdmPropertyConstructor("Description", new EdmStringConstant("The identityProvider API is deprecated and will stop returning data on March 2023. Please use the new identityProviderBase API.")),
            new EdmPropertyConstructor("Version", new EdmStringConstant("2021-05/test")));

        // Act
        RevisionRecord revision = new();
        revision.Initialize(record);

        // Assert
        Assert.NotNull(revision.Version);
        Assert.Equal("2021-05/test", revision.Version);

        Assert.NotNull(revision.Description);
        Assert.Equal("The identityProvider API is deprecated and will stop returning data on March 2023. Please use the new identityProviderBase API.", revision.Description);

        Assert.NotNull(revision.Date);
        Assert.Equal(new DateTime(2021, 8, 24), revision.Date);

        Assert.NotNull(revision.RemovalDate);
        Assert.Equal(new DateTime(2021, 10, 24), revision.RemovalDate);
    }
    [Fact]
    public void WorksForAllKinds()
    {
        // Arrange
        IEdmRecordExpression record1 = new EdmRecordExpression(
            new EdmPropertyConstructor("Date", new EdmDateConstant(new Date(2021, 8, 24))),
            new EdmPropertyConstructor("RemovalDate", new EdmDateConstant(new Date(2021, 10, 24))),
            new EdmPropertyConstructor("Kind", new EdmEnumMemberExpression(addedValue)), 
            new EdmPropertyConstructor("Description", new EdmStringConstant("The identityProvider API is deprecated and will stop returning data on March 2023. Please use the new identityProviderBase API.")),
            new EdmPropertyConstructor("Version", new EdmStringConstant("2021-05/test")));

        IEdmRecordExpression record2 = new EdmRecordExpression(
            new EdmPropertyConstructor("Date", new EdmDateConstant(new Date(2023, 3, 2))),
            new EdmPropertyConstructor("RemovalDate", new EdmDateConstant(new Date(2023, 05, 30))),
            new EdmPropertyConstructor("Kind", new EdmEnumMemberExpression(deprecatedValue)),
            new EdmPropertyConstructor("Description", new EdmStringConstant("Private preview test.")),
            new EdmPropertyConstructor("Version", new EdmStringConstant("2023-03/test")));

        // Act
        RevisionRecord revision1 = new();
        revision1.Initialize(record1);
        
        RevisionRecord revision2 = new();
        revision2.Initialize(record2);

        // Assert
        Assert.Equal(RevisionKind.Added, revision1.Kind);
        Assert.Equal(RevisionKind.Deprecated, revision2.Kind);
    }
}