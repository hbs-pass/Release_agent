// Licence: Everyone is free to use the code contained in this file in any way.
using System;
namespace SmartAssembly.Attributes {
    /// <summary>
    /// When added to any type (class, enum, interface, or struct), prevents that type's fields from being included in error reports.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
    [DoNotObfuscate]
    [DoNotPrune]
    public sealed class DoNotCaptureAttribute : Attribute
    {
    }
    /// <summary>
    /// Prevent variables from being included in error reports.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct)]
    public sealed class DoNotCaptureVariablesAttribute : Attribute
    {
    }
    /// <summary>
    /// Exclude the member from Strings Encoding.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Module | AttributeTargets.Struct)]
    public sealed class DoNotEncodeStringsAttribute : Attribute
    {
    }
    /// <summary>
    /// Prevent the method from being moved to another type if Method Parent Obfuscation is turned on. Note that a method with any attribute is automatically excluded from moving.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DoNotMoveAttribute : Attribute
    {
    }
    /// <summary>
    /// Prevent all methods in the class from being moved to another type if Method Parent Obfuscation is turned on.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotMoveMethodsAttribute : Attribute
    {
    }
    /// <summary>
    /// Exclude the member from obfuscation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Module | AttributeTargets.Struct)]
    public sealed class DoNotObfuscateAttribute : Attribute
    {
    }
    /// <summary>
    /// Do not apply Control Flow Obfuscation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct)]
    public sealed class DoNotObfuscateControlFlowAttribute : Attribute
    {
    }
    /// <summary>
    /// Exclude the type definition, as well as all the type's members from obfuscation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct)]
    public sealed class DoNotObfuscateTypeAttribute : Attribute
    {
    }
    /// <summary>
    /// Exclude the type definition from pruning.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Delegate | AttributeTargets.Enum | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Module | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Struct)]
    public sealed class DoNotPruneAttribute : Attribute
    {
    }
    /// <summary>
    /// Exclude the type definition, as well as all type's members, from pruning.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct)]
    public sealed class DoNotPruneTypeAttribute : Attribute
    {
    }
    /// <summary>
    /// Do not seal the type. This overrides the option to automatically seal all possible classes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotSealTypeAttribute : Attribute
    {
    }
    /// <summary>
    /// If Strings Encoding is turned on in options, revert the effect of <see cref="SmartAssembly.Attributes.DoNotEncodeStringsAttribute"/> set on a parent member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct)]
    public sealed class EncodeStringsAttribute : Attribute
    {
    }
    /// <summary>
    /// Turn off References Dynamic Proxy for this member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Module | AttributeTargets.Struct)]
    public sealed class ExcludeFromMemberRefsProxyAttribute : Attribute
    {
    }
    /// <summary>
    /// Force element to be obfuscated, even if it was excluded by safety mechanisms. Takes precedence over <see cref="SmartAssembly.Attributes.DoNotObfuscateAttribute"/>!
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface)]
    public sealed class ForceObfuscateAttribute : Attribute
    {
        /// <summary>
        /// Force element to be obfuscated, even if it was excluded by safety mechanisms. Takes precedence over <see cref="SmartAssembly.Attributes.DoNotObfuscateAttribute"/>!
        /// </summary>
        /// <param name="useHashAsName">If <c>true</c>, uses MD5 hash of a method name prefixed with <c>"_"</c>. Otherwise, uses default Name Mangling setting.</param>
        public ForceObfuscateAttribute(bool useHashAsName = false) { }
    }
    /// <summary>
    /// Turn on control flow obfuscation. This overrides the control flow obfuscation level set in the SmartAssembly project.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Struct)]
    public sealed class ObfuscateControlFlowAttribute : Attribute
    {
    }
    /// <summary>
    /// Force the type to be in a chosen namespace.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct)]
    public sealed class ObfuscateNamespaceToAttribute : Attribute
    {
        /// <summary>
        /// Force the type to be in a chosen namespace.
        /// </summary>
        /// <param name="newName">Name of the namespace the type should be in.</param>
        public ObfuscateNamespaceToAttribute(string newName) { }
    }
    /// <summary>
    /// Force the type or field to be renamed to a chosen name. Only rename methods if Advanced Renaming Algorithm is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Field | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Struct)]
    public sealed class ObfuscateToAttribute : Attribute
    {
        /// <summary>
        /// Force the type or field to be renamed to a chosen name. Only rename methods if Advanced Renaming Algorithm is used.
        /// </summary>
        /// <param name="newName">Name to rename the type or field to.</param>
        public ObfuscateToAttribute(string newName) { }
    }
    /// <summary>
    /// Report any unhandled exception, which occurs in this method. This is useful for DLLs because it saves you catching exceptions yourself and passing the exceptions to SmartAssembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ReportExceptionAttribute : Attribute
    {
    }
    /// <summary>
    /// Increment feature usage counter each time the method is ran. By default, the method's name is used for the feature name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
    public sealed class ReportUsageAttribute : Attribute
    {
        /// <summary>
        /// Increment feature usage counter each time the method is ran. By default, the method's name is used for the feature name.
        /// </summary>
        public ReportUsageAttribute() { }
        /// <summary>
        /// Increment feature usage counter each time the method is ran. By default, the method's name is used for the feature name.
        /// </summary>
        /// <param name="featureName">The supplied string is used for the feature name.</param>
        public ReportUsageAttribute(string featureName) { }
    }
    /// <summary>
    /// Ensure the member remains public after obfuscation. When SmartAssembly obfuscates some members, they may become internal. This can stop other applications from post-processing the obfuscated code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Delegate)]
    public sealed class StayPublicAttribute : Attribute
    {
    }
    /// <summary>
    /// When a member must be visible to external assemblies (like plugins) after merging, SmartAssembly creates an embedded assembly forwarding calls to the members marked with this attribute from the original to the merged location.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum)]
    public sealed class ForwardWhenMergedAttribute : Attribute
    {
    }
}
