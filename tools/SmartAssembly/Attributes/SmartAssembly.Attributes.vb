' Licence: Everyone is free to use the code contained in this file in any way.
Imports System
Namespace SmartAssembly.Attributes
    ''' <summary>
    ''' When added to any type (class, enum, interface, or struct), prevents that type's fields from being included in error reports.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Class Or AttributeTargets.Struct)>
    <DoNotObfuscate>
    <DoNotPrune>
    Public NotInheritable Class DoNotCaptureAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Prevent variables from being included in error reports.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method Or AttributeTargets.Constructor Or AttributeTargets.Struct)>
    Public NotInheritable Class DoNotCaptureVariablesAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Exclude the member from Strings Encoding.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Assembly Or AttributeTargets.Class Or AttributeTargets.Method Or AttributeTargets.Constructor Or AttributeTargets.Module Or AttributeTargets.Struct)>
    Public NotInheritable Class DoNotEncodeStringsAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Prevent the method from being moved to another type if Method Parent Obfuscation is turned on. Note that a method with any attribute is automatically excluded from moving.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method)>
    Public NotInheritable Class DoNotMoveAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Prevent all methods in the class from being moved to another type if Method Parent Obfuscation is turned on.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class)>
    Public NotInheritable Class DoNotMoveMethodsAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Exclude the member from obfuscation.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Assembly Or AttributeTargets.Class Or AttributeTargets.Delegate Or AttributeTargets.Enum Or AttributeTargets.Field Or AttributeTargets.Interface Or AttributeTargets.Method Or AttributeTargets.Module Or AttributeTargets.Struct)>
    Public NotInheritable Class DoNotObfuscateAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Do not apply Control Flow Obfuscation.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method Or AttributeTargets.Constructor Or AttributeTargets.Struct)>
    Public NotInheritable Class DoNotObfuscateControlFlowAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Exclude the type definition, as well as all the type's members from obfuscation.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Enum Or AttributeTargets.Interface Or AttributeTargets.Struct)>
    Public NotInheritable Class DoNotObfuscateTypeAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Exclude the type definition from pruning.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Assembly Or AttributeTargets.Class Or AttributeTargets.Constructor Or AttributeTargets.Delegate Or AttributeTargets.Enum Or AttributeTargets.Event Or AttributeTargets.Field Or AttributeTargets.Interface Or AttributeTargets.Method Or AttributeTargets.Module Or AttributeTargets.Parameter Or AttributeTargets.Property Or AttributeTargets.Struct)>
    Public NotInheritable Class DoNotPruneAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Exclude the type definition, as well as all type's members, from pruning.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Enum Or AttributeTargets.Interface Or AttributeTargets.Struct)>
    Public NotInheritable Class DoNotPruneTypeAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Do not seal the type. This overrides the option to automatically seal all possible classes.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class)>
    Public NotInheritable Class DoNotSealTypeAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' If Strings Encoding is turned on in options, revert the effect of <see cref="SmartAssembly.Attributes.DoNotEncodeStringsAttribute"/> set on a parent member.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method Or AttributeTargets.Constructor Or AttributeTargets.Struct)>
    Public NotInheritable Class EncodeStringsAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Turn off References Dynamic Proxy for this member.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Assembly Or AttributeTargets.Class Or AttributeTargets.Constructor Or AttributeTargets.Method Or AttributeTargets.Module Or AttributeTargets.Struct)>
    Public NotInheritable Class ExcludeFromMemberRefsProxyAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Force element to be obfuscated, even if it was excluded by safety mechanisms. Takes precedence over <see cref="SmartAssembly.Attributes.DoNotObfuscateAttribute"/>!
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method Or AttributeTargets.Struct Or AttributeTargets.Property Or AttributeTargets.Field Or AttributeTargets.Interface)>
    Public NotInheritable Class ForceObfuscateAttribute
        Inherits Attribute
        ''' <summary>
        ''' Force element to be obfuscated, even if it was excluded by safety mechanisms. Takes precedence over <see cref="SmartAssembly.Attributes.DoNotObfuscateAttribute"/>!
        ''' </summary>
        ''' <param name="useHashAsName">If <c>true</c>, uses MD5 hash of a method name prefixed with <c>"_"</c>. Otherwise, uses default Name Mangling setting.</param>
        Public Sub New(Optional ByVal useHashAsName As bool = false)
        End Sub
    End Class
    ''' <summary>
    ''' Turn on control flow obfuscation. This overrides the control flow obfuscation level set in the SmartAssembly project.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method Or AttributeTargets.Constructor Or AttributeTargets.Struct)>
    Public NotInheritable Class ObfuscateControlFlowAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Force the type to be in a chosen namespace.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Enum Or AttributeTargets.Interface Or AttributeTargets.Struct)>
    Public NotInheritable Class ObfuscateNamespaceToAttribute
        Inherits Attribute
        ''' <summary>
        ''' Force the type to be in a chosen namespace.
        ''' </summary>
        ''' <param name="newName">Name of the namespace the type should be in.</param>
        Public Sub New(ByVal newName As string)
        End Sub
    End Class
    ''' <summary>
    ''' Force the type or field to be renamed to a chosen name. Only rename methods if Advanced Renaming Algorithm is used.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Enum Or AttributeTargets.Field Or AttributeTargets.Interface Or AttributeTargets.Method Or AttributeTargets.Struct)>
    Public NotInheritable Class ObfuscateToAttribute
        Inherits Attribute
        ''' <summary>
        ''' Force the type or field to be renamed to a chosen name. Only rename methods if Advanced Renaming Algorithm is used.
        ''' </summary>
        ''' <param name="newName">Name to rename the type or field to.</param>
        Public Sub New(ByVal newName As string)
        End Sub
    End Class
    ''' <summary>
    ''' Report any unhandled exception, which occurs in this method. This is useful for DLLs because it saves you catching exceptions yourself and passing the exceptions to SmartAssembly.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method)>
    Public NotInheritable Class ReportExceptionAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' Increment feature usage counter each time the method is ran. By default, the method's name is used for the feature name.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method Or AttributeTargets.Constructor)>
    Public NotInheritable Class ReportUsageAttribute
        Inherits Attribute
        ''' <summary>
        ''' Increment feature usage counter each time the method is ran. By default, the method's name is used for the feature name.
        ''' </summary>
        Public Sub New()
        End Sub
        ''' <summary>
        ''' Increment feature usage counter each time the method is ran. By default, the method's name is used for the feature name.
        ''' </summary>
        ''' <param name="featureName">The supplied string is used for the feature name.</param>
        Public Sub New(ByVal featureName As string)
        End Sub
    End Class
    ''' <summary>
    ''' Ensure the member remains public after obfuscation. When SmartAssembly obfuscates some members, they may become internal. This can stop other applications from post-processing the obfuscated code.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Struct Or AttributeTargets.Interface Or AttributeTargets.Enum Or AttributeTargets.Delegate)>
    Public NotInheritable Class StayPublicAttribute
        Inherits Attribute
    End Class
    ''' <summary>
    ''' When a member must be visible to external assemblies (like plugins) after merging, SmartAssembly creates an embedded assembly forwarding calls to the members marked with this attribute from the original to the merged location.
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Struct Or AttributeTargets.Interface Or AttributeTargets.Enum)>
    Public NotInheritable Class ForwardWhenMergedAttribute
        Inherits Attribute
    End Class
End Namespace
