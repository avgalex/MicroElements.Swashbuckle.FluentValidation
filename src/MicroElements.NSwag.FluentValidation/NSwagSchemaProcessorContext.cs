﻿// Copyright (c) MicroElements. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using FluentValidation.Validators;
using MicroElements.OpenApi.FluentValidation;
using Namotion.Reflection;
using NJsonSchema.Generation;

namespace MicroElements.NSwag.FluentValidation
{
    public class NSwagSchemaProcessorContext : ISchemaGenerationContext<SchemaProcessorContext>
    {
        /// <inheritdoc />
        public Type SchemaType { get; }

        /// <inheritdoc />
        public IEnumerable<string> Properties => Schema.Schema.Properties?.Keys ?? Array.Empty<string>();

        /// <inheritdoc />
        public IReadOnlyList<IFluentValidationRule<SchemaProcessorContext>> Rules { get; }

        /// <inheritdoc />
        public ISchemaGenerationContext<SchemaProcessorContext> With(SchemaProcessorContext schema)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IRuleContext<SchemaProcessorContext> Create(
            string schemaPropertyName,
            ValidationRuleContext validationRuleContext,
            IPropertyValidator propertyValidator)
        {
            return new NSwagRuleContext(Schema, schemaPropertyName, propertyValidator);
        }

        /// <inheritdoc />
        public SchemaProcessorContext Schema { get; }

        /// <inheritdoc />
        public ISchemaProvider<SchemaProcessorContext> SchemaProvider { get; }

        /// <inheritdoc />
        IReadOnlyList<IFluentValidationRule> ISchemaGenerationContext.Rules => Rules;

        /// <inheritdoc />
        public ISchemaGenerationOptions SchemaGenerationOptions { get; }

        public NSwagSchemaProcessorContext(
            Type schemaType,
            SchemaProcessorContext schema,
            IReadOnlyList<IFluentValidationRule<SchemaProcessorContext>> rules,
            ISchemaProvider<SchemaProcessorContext>? schemaProvider,
            ISchemaGenerationOptions schemaGenerationOptions)
        {
            SchemaType = schemaType;
            Schema = schema;
            Rules = rules;

            SchemaProvider = schemaProvider ?? new SwashbuckleSchemaProvider(schema);
            SchemaGenerationOptions = schemaGenerationOptions;
        }
    }

    public class SwashbuckleSchemaProvider : ISchemaProvider<SchemaProcessorContext>
    {
        private SchemaProcessorContext _processorContext;

        public SwashbuckleSchemaProvider(SchemaProcessorContext processorContext)
        {
            _processorContext = processorContext;
        }

        /// <inheritdoc />
        public SchemaProcessorContext GetSchemaForType(Type type)
        {
            var schemaForType = _processorContext.Resolver.GetSchema(type, isIntegerEnumeration: false);
            return new SchemaProcessorContext(
                type.ToContextualType(),
                schemaForType,
                _processorContext.Resolver,
                _processorContext.Generator,
                _processorContext.Settings);
        }
    }

}