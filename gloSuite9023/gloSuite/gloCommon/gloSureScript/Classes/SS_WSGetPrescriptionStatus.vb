﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.18444
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict Off
Option Explicit On

Imports System.Xml.Serialization

Namespace WSGetPrescriptionStatus

   
    '
    'This source code was auto-generated by xsd, Version=4.0.30319.1.
    '

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1"), _
     System.SerializableAttribute(), _
     System.Diagnostics.DebuggerStepThroughAttribute(), _
     System.ComponentModel.DesignerCategoryAttribute("code"), _
     System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True, [Namespace]:="urn:drfirst.com:epcsapi:v1_0"), _
     System.Xml.Serialization.XmlRootAttribute([Namespace]:="urn:drfirst.com:epcsapi:v1_0", IsNullable:=False)> _
    Partial Public Class EpcsRequest

        Private epcsRequestHeaderField As EpcsRequestEpcsRequestHeader

        Private epcsRequestBodyField As EpcsRequestEpcsRequestBody

        '''<remarks/>
        Public Property EpcsRequestHeader() As EpcsRequestEpcsRequestHeader
            Get
                Return Me.epcsRequestHeaderField
            End Get
            Set(value As EpcsRequestEpcsRequestHeader)
                Me.epcsRequestHeaderField = value
            End Set
        End Property

        '''<remarks/>
        Public Property EpcsRequestBody() As EpcsRequestEpcsRequestBody
            Get
                Return Me.epcsRequestBodyField
            End Get
            Set(value As EpcsRequestEpcsRequestBody)
                Me.epcsRequestBodyField = value
            End Set
        End Property
    End Class

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1"), _
     System.SerializableAttribute(), _
     System.Diagnostics.DebuggerStepThroughAttribute(), _
     System.ComponentModel.DesignerCategoryAttribute("code"), _
     System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True, [Namespace]:="urn:drfirst.com:epcsapi:v1_0")> _
    Partial Public Class EpcsRequestEpcsRequestHeader

        Private vendorNameField As String

        Private vendorLabelField As String

        Private vendorNodeNameField As String

        Private vendorNodeLabelField As String

        Private appVersionField As EpcsRequestEpcsRequestHeaderAppVersion

        Private sourceOrganizationIdField As String

        Private dateField As String

        '''<remarks/>
        Public Property VendorName() As String
            Get
                Return Me.vendorNameField
            End Get
            Set(value As String)
                Me.vendorNameField = value
            End Set
        End Property

        '''<remarks/>
        Public Property VendorLabel() As String
            Get
                Return Me.vendorLabelField
            End Get
            Set(value As String)
                Me.vendorLabelField = value
            End Set
        End Property

        '''<remarks/>
        Public Property VendorNodeName() As String
            Get
                Return Me.vendorNodeNameField
            End Get
            Set(value As String)
                Me.vendorNodeNameField = value
            End Set
        End Property

        '''<remarks/>
        Public Property VendorNodeLabel() As String
            Get
                Return Me.vendorNodeLabelField
            End Get
            Set(value As String)
                Me.vendorNodeLabelField = value
            End Set
        End Property

        '''<remarks/>
        Public Property AppVersion() As EpcsRequestEpcsRequestHeaderAppVersion
            Get
                Return Me.appVersionField
            End Get
            Set(value As EpcsRequestEpcsRequestHeaderAppVersion)
                Me.appVersionField = value
            End Set
        End Property

        '''<remarks/>
        Public Property SourceOrganizationId() As String
            Get
                Return Me.sourceOrganizationIdField
            End Get
            Set(value As String)
                Me.sourceOrganizationIdField = value
            End Set
        End Property

        '''<remarks/>
        Public Property [Date]() As String
            Get
                Return Me.dateField
            End Get
            Set(value As String)
                Me.dateField = value
            End Set
        End Property
    End Class

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1"), _
     System.SerializableAttribute(), _
     System.Diagnostics.DebuggerStepThroughAttribute(), _
     System.ComponentModel.DesignerCategoryAttribute("code"), _
     System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True, [Namespace]:="urn:drfirst.com:epcsapi:v1_0")> _
    Partial Public Class EpcsRequestEpcsRequestHeaderAppVersion

        Private appNameField As String

        Private applicationVersionField As String

        '''<remarks/>
        Public Property AppName() As String
            Get
                Return Me.appNameField
            End Get
            Set(value As String)
                Me.appNameField = value
            End Set
        End Property

        '''<remarks/>
        Public Property ApplicationVersion() As String
            Get
                Return Me.applicationVersionField
            End Get
            Set(value As String)
                Me.applicationVersionField = value
            End Set
        End Property
    End Class

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1"), _
     System.SerializableAttribute(), _
     System.Diagnostics.DebuggerStepThroughAttribute(), _
     System.ComponentModel.DesignerCategoryAttribute("code"), _
     System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True, [Namespace]:="urn:drfirst.com:epcsapi:v1_0")> _
    Partial Public Class EpcsRequestEpcsRequestBody

        Private wsGetPrescriptionStatusRequestField As EpcsRequestEpcsRequestBodyWsGetPrescriptionStatusRequest

        '''<remarks/>
        Public Property WsGetPrescriptionStatusRequest() As EpcsRequestEpcsRequestBodyWsGetPrescriptionStatusRequest
            Get
                Return Me.wsGetPrescriptionStatusRequestField
            End Get
            Set(value As EpcsRequestEpcsRequestBodyWsGetPrescriptionStatusRequest)
                Me.wsGetPrescriptionStatusRequestField = value
            End Set
        End Property
    End Class

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1"), _
     System.SerializableAttribute(), _
     System.Diagnostics.DebuggerStepThroughAttribute(), _
     System.ComponentModel.DesignerCategoryAttribute("code"), _
     System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True, [Namespace]:="urn:drfirst.com:epcsapi:v1_0")> _
    Partial Public Class EpcsRequestEpcsRequestBodyWsGetPrescriptionStatusRequest

        Private prescriptionRequestListField() As EpcsRequestEpcsRequestBodyWsGetPrescriptionStatusRequestPrescriptionRequestList

        '''<remarks/>
        <System.Xml.Serialization.XmlArrayItemAttribute("PrescriptionRequest", IsNullable:=False)> _
        Public Property PrescriptionRequestList() As EpcsRequestEpcsRequestBodyWsGetPrescriptionStatusRequestPrescriptionRequestList()
            Get
                Return Me.prescriptionRequestListField
            End Get
            Set(value As EpcsRequestEpcsRequestBodyWsGetPrescriptionStatusRequestPrescriptionRequestList())
                Me.prescriptionRequestListField = value
            End Set
        End Property
    End Class

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1"), _
     System.SerializableAttribute(), _
     System.Diagnostics.DebuggerStepThroughAttribute(), _
     System.ComponentModel.DesignerCategoryAttribute("code"), _
     System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True, [Namespace]:="urn:drfirst.com:epcsapi:v1_0")> _
    Partial Public Class EpcsRequestEpcsRequestBodyWsGetPrescriptionStatusRequestPrescriptionRequestList

        Private sourcePrescriptionIdField As String

        '''<remarks/>
        <System.Xml.Serialization.XmlElementAttribute(DataType:="integer")> _
        Public Property SourcePrescriptionId() As String
            Get
                Return Me.sourcePrescriptionIdField
            End Get
            Set(value As String)
                Me.sourcePrescriptionIdField = value
            End Set
        End Property
    End Class


End Namespace