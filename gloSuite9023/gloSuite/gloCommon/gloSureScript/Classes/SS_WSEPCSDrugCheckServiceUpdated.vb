﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.17929
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict Off
Option Explicit On

Imports System.Xml.Serialization
Namespace WSEPCSDrugCheckServiceMultiple
    '
    'This source code was auto-generated by xsd, Version=4.0.30319.17929.
    '

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929"), _
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
                Me.epcsRequestHeaderField = Value
            End Set
        End Property

        '''<remarks/>
        Public Property EpcsRequestBody() As EpcsRequestEpcsRequestBody
            Get
                Return Me.epcsRequestBodyField
            End Get
            Set(value As EpcsRequestEpcsRequestBody)
                Me.epcsRequestBodyField = Value
            End Set
        End Property
    End Class

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929"), _
     System.SerializableAttribute(), _
     System.Diagnostics.DebuggerStepThroughAttribute(), _
     System.ComponentModel.DesignerCategoryAttribute("code"), _
     System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True, [Namespace]:="urn:drfirst.com:epcsapi:v1_0")> _
    Partial Public Class EpcsRequestEpcsRequestHeader

        Private vendorNameField As String

        Private vendorLabelField As String

        Private vendorNodeNameField As String

        Private vendorNodeLabelField As String

        ''EPCS Api Version
        Private EpcsApiVersionField As String


        Private appVersionField As EpcsRequestEpcsRequestHeaderAppVersion

        
        Private sourceOrganizationIdField As String

        Private dateField As String

        '''<remarks/>
        Public Property VendorName() As String
            Get
                Return Me.vendorNameField
            End Get
            Set(value As String)
                Me.vendorNameField = Value
            End Set
        End Property

        '''<remarks/>
        Public Property VendorLabel() As String
            Get
                Return Me.vendorLabelField
            End Get
            Set(value As String)
                Me.vendorLabelField = Value
            End Set
        End Property

        '''<remarks/>
        Public Property VendorNodeName() As String
            Get
                Return Me.vendorNodeNameField
            End Get
            Set(value As String)
                Me.vendorNodeNameField = Value
            End Set
        End Property

        '''<remarks/>
        Public Property VendorNodeLabel() As String
            Get
                Return Me.vendorNodeLabelField
            End Get
            Set(value As String)
                Me.vendorNodeLabelField = Value
            End Set
        End Property

        '''<remarks/>
        Public Property AppVersion() As EpcsRequestEpcsRequestHeaderAppVersion
            Get
                Return Me.appVersionField
            End Get
            Set(value As EpcsRequestEpcsRequestHeaderAppVersion)
                Me.appVersionField = Value
            End Set
        End Property

        '''<remarks/>
        Public Property SourceOrganizationId() As String
            Get
                Return Me.sourceOrganizationIdField
            End Get
            Set(value As String)
                Me.sourceOrganizationIdField = Value
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

        ''EPCS Api Version
        '''<remarks/>
        Public Property EpcsApiVersion() As String
            Get
                Return Me.EpcsApiVersionField
            End Get
            Set(value As String)
                Me.EpcsApiVersionField = value
            End Set
        End Property
    End Class

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929"), _
     System.SerializableAttribute(), _
     System.Diagnostics.DebuggerStepThroughAttribute(), _
     System.ComponentModel.DesignerCategoryAttribute("code"), _
     System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True, [Namespace]:="urn:drfirst.com:epcsapi:v1_0")> _
    Partial Public Class EpcsRequestEpcsRequestHeaderAppVersion

        Private appNameField As String

        Private applicationVersionField As String

        Private textField() As String

        '''<remarks/>
        Public Property AppName() As String
            Get
                Return Me.appNameField
            End Get
            Set(value As String)
                Me.appNameField = Value
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

        '''<remarks/>
        <System.Xml.Serialization.XmlTextAttribute()> _
        Public Property Text() As String()
            Get
                Return Me.textField
            End Get
            Set(value As String())
                Me.textField = Value
            End Set
        End Property
    End Class

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929"), _
     System.SerializableAttribute(), _
     System.Diagnostics.DebuggerStepThroughAttribute(), _
     System.ComponentModel.DesignerCategoryAttribute("code"), _
     System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True, [Namespace]:="urn:drfirst.com:epcsapi:v1_0")> _
    Partial Public Class EpcsRequestEpcsRequestBody

        Private wsGetEPCSDrugCheckRequestField As EpcsRequestEpcsRequestBodyWsGetEPCSDrugCheckRequest

        '''<remarks/>
        Public Property WsGetEPCSDrugCheckRequest() As EpcsRequestEpcsRequestBodyWsGetEPCSDrugCheckRequest
            Get
                Return Me.wsGetEPCSDrugCheckRequestField
            End Get
            Set(value As EpcsRequestEpcsRequestBodyWsGetEPCSDrugCheckRequest)
                Me.wsGetEPCSDrugCheckRequestField = Value
            End Set
        End Property
    End Class

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929"), _
     System.SerializableAttribute(), _
     System.Diagnostics.DebuggerStepThroughAttribute(), _
     System.ComponentModel.DesignerCategoryAttribute("code"), _
     System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True, [Namespace]:="urn:drfirst.com:epcsapi:v1_0")> _
    Partial Public Class EpcsRequestEpcsRequestBodyWsGetEPCSDrugCheckRequest

        Private ePCSDrugCheckRequestListTypeField() As EpcsRequestEpcsRequestBodyWsGetEPCSDrugCheckRequestEPCSDrugPermissionStatusType

        '''<remarks/>
        <System.Xml.Serialization.XmlArrayItemAttribute("EPCSDrugPermissionStatusType", IsNullable:=False)> _
        Public Property EPCSDrugCheckRequestListType() As EpcsRequestEpcsRequestBodyWsGetEPCSDrugCheckRequestEPCSDrugPermissionStatusType()
            Get
                Return Me.ePCSDrugCheckRequestListTypeField
            End Get
            Set(value As EpcsRequestEpcsRequestBodyWsGetEPCSDrugCheckRequestEPCSDrugPermissionStatusType())
                Me.ePCSDrugCheckRequestListTypeField = Value
            End Set
        End Property
    End Class

    '''<remarks/>
    <System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929"), _
     System.SerializableAttribute(), _
     System.Diagnostics.DebuggerStepThroughAttribute(), _
     System.ComponentModel.DesignerCategoryAttribute("code"), _
     System.Xml.Serialization.XmlTypeAttribute(AnonymousType:=True, [Namespace]:="urn:drfirst.com:epcsapi:v1_0")> _
    Partial Public Class EpcsRequestEpcsRequestBodyWsGetEPCSDrugCheckRequestEPCSDrugPermissionStatusType

        Private ncpdpIdField As String

        Private drugNameField As String

        Private ndcIdField As String

        Private prescriberStateCodeField As String

        '''<remarks/>
        Public Property NcpdpId() As String
            Get
                Return Me.ncpdpIdField
            End Get
            Set(value As String)
                Me.ncpdpIdField = value
            End Set
        End Property

        '''<remarks/>
        Public Property DrugName() As String
            Get
                Return Me.drugNameField
            End Get
            Set(value As String)
                Me.drugNameField = Value
            End Set
        End Property

        '''<remarks/>
        Public Property NdcId() As String
            Get
                Return Me.ndcIdField
            End Get
            Set(value As String)
                Me.ndcIdField = value
            End Set
        End Property

        '''<remarks/>
        Public Property PrescriberStateCode() As String
            Get
                Return Me.prescriberStateCodeField
            End Get
            Set(value As String)
                Me.prescriberStateCodeField = Value
            End Set
        End Property
    End Class
End Namespace