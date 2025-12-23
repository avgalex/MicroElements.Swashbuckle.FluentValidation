// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using FluentValidation;
#if OPENAPI_V2
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace MicroElements.Swashbuckle.FluentValidation.Tests
{
    /// <summary>
    /// Tests for models with enum properties.
    /// Issue #176: https://github.com/micro-elements/MicroElements.Swashbuckle.FluentValidation/issues/176
    /// </summary>
    public partial class SchemaGenerationTests
    {
        public enum SampleStatus
        {
            Pending,
            Active,
            Completed
        }

        public class ModelWithEnumProperty
        {
            public string Name { get; set; } = string.Empty;
            public SampleStatus Status { get; set; }
            public string Description { get; set; } = string.Empty;
        }

        public class ModelWithEnumPropertyValidator : AbstractValidator<ModelWithEnumProperty>
        {
            public ModelWithEnumPropertyValidator()
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                RuleFor(x => x.Description).MaximumLength(500);
            }
        }

        /// <summary>
        /// Issue #176: InvalidCastException when models contain enum properties.
        /// In OpenAPI v2 (Microsoft.OpenApi 2.x), enum properties are represented as OpenApiSchemaReference,
        /// not OpenApiSchema, causing InvalidCastException in GetProperties method.
        /// </summary>
        [Fact]
        public void Model_With_Enum_Property_Should_Not_Throw_InvalidCastException()
        {
            // Arrange
            var schemaRepository = new SchemaRepository();

            // Act - This should not throw InvalidCastException
            var referenceSchema = SchemaGenerator(new ModelWithEnumPropertyValidator())
                .GenerateSchema(typeof(ModelWithEnumProperty), schemaRepository);

            // Assert
            var schema = schemaRepository.GetSchema(referenceSchema.GetRefId()!);

            // Non-enum properties should have validation rules applied (using PascalCase as in C# property names)
            var nameProperty = schema.GetProperty("Name");
            nameProperty.Should().NotBeNull("Name property should exist in schema");
            nameProperty!.MinLength.Should().Be(1);
            nameProperty.MaxLength.Should().Be(100);

            var descriptionProperty = schema.GetProperty("Description");
            descriptionProperty.Should().NotBeNull("Description property should exist in schema");
            descriptionProperty!.MaxLength.Should().Be(500);

            // Schema should have all properties (including enum)
            schema.Properties.Should().HaveCount(3);
            schema.Properties.Keys.Should().Contain("Name");
            schema.Properties.Keys.Should().Contain("Status");
            schema.Properties.Keys.Should().Contain("Description");
        }

        public class NestedModel
        {
            public int Id { get; set; }
        }

        public class ModelWithNestedObject
        {
            public string Name { get; set; } = string.Empty;
            public NestedModel Nested { get; set; } = new();
        }

        public class ModelWithNestedObjectValidator : AbstractValidator<ModelWithNestedObject>
        {
            public ModelWithNestedObjectValidator()
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            }
        }

        /// <summary>
        /// Issue #176: NullReferenceException when models contain nested object properties.
        /// In OpenAPI v2 (Microsoft.OpenApi 2.x), nested object properties are represented as OpenApiSchemaReference,
        /// causing NullReferenceException when trying to access properties on the reference.
        /// </summary>
        [Fact]
        public void Model_With_Nested_Object_Property_Should_Not_Throw_NullReferenceException()
        {
            // Arrange
            var schemaRepository = new SchemaRepository();

            // Act - This should not throw NullReferenceException
            var referenceSchema = SchemaGenerator(new ModelWithNestedObjectValidator())
                .GenerateSchema(typeof(ModelWithNestedObject), schemaRepository);

            // Assert
            var schema = schemaRepository.GetSchema(referenceSchema.GetRefId()!);

            // Non-nested properties should have validation rules applied
            var nameProperty = schema.GetProperty("Name");
            nameProperty.Should().NotBeNull("Name property should exist in schema");
            nameProperty!.MinLength.Should().Be(1);
            nameProperty.MaxLength.Should().Be(100);

            // Schema should have all properties (including nested object)
            schema.Properties.Should().HaveCount(2);
            schema.Properties.Keys.Should().Contain("Name");
            schema.Properties.Keys.Should().Contain("Nested");
        }
    }
}
