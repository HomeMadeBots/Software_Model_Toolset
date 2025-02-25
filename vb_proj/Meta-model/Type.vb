﻿Public MustInherit Class Type
    Inherits Software_Element

    ' -------------------------------------------------------------------------------------------- '
    ' Constructors
    ' -------------------------------------------------------------------------------------------- '

    Public Sub New()
    End Sub

    Public Sub New(
            name As String,
            description As String,
            owner As Software_Element,
            parent_node As TreeNode)
        MyBase.New(name, description, owner, parent_node)
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '
    Public Overrides Function Is_Allowed_Parent(parent As Software_Element) As Boolean
        Dim is_allowed As Boolean = False
        If parent.GetType() = GetType(Top_Level_Package) _
            Or parent.GetType() = GetType(Package) Then
            is_allowed = True
        End If
        Return is_allowed
    End Function

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        CType(Me.Owner, Package).Types.Remove(Me)
        CType(new_parent, Package).Types.Add(Me)
    End Sub

    Protected Overrides Sub Remove_Me()
        Dim parent_pkg As Package = CType(Me.Owner, Package)
        Me.Node.Remove()
        parent_pkg.Types.Remove(Me)
    End Sub

End Class


Public MustInherit Class Basic_Type
    Inherits Type

    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '
    Protected Overrides Sub Create_Node()
        Me.Node = New TreeNode(Me.Name) With {
            .ImageKey = "Basic_Type",
            .SelectedImageKey = "Basic_Type",
            .ContextMenuStrip = Software_Element.Read_Only_Context_Menu,
            .Tag = Me}
    End Sub

    Protected Overrides Sub Move_Me(new_parent As Software_Element)
        Throw New Exception("A Basic Type cannot be moved.")
    End Sub

    Protected Overrides Sub Remove_Me()
        Throw New Exception("A Basic Type cannot be removed.")
    End Sub

    Protected Overrides Function Get_Writable_Context_Menu() As ContextMenuStrip
        Return Software_Element.Read_Only_Context_Menu
    End Function

End Class


Public Class Basic_Integer_Type
    Inherits Basic_Type

    Public Shared ReadOnly Metaclass_Name As String = "Basic_Integer_Type"

    Public Overrides Function Get_Metaclass_Name() As String
        Return Basic_Integer_Type.Metaclass_Name
    End Function

End Class


Public Class Basic_Boolean_Type
    Inherits Basic_Type

    Public Shared ReadOnly Metaclass_Name As String = "Basic_Boolean_Type"

    Public Overrides Function Get_Metaclass_Name() As String
        Return Basic_Boolean_Type.Metaclass_Name
    End Function

End Class


Public Class Basic_Floating_Point_Type
    Inherits Basic_Type

    Public Shared ReadOnly Metaclass_Name As String = "Basic_Floating_Point_Type"

    Public Overrides Function Get_Metaclass_Name() As String
        Return Basic_Floating_Point_Type.Metaclass_Name
    End Function

End Class


Public Class Array_Type
    Inherits Type

    Public Multiplicity As UInteger
    Public Base_Type_Ref As Guid

    Public Shared ReadOnly Metaclass_Name As String = "Array_Type"

    Public Shared ReadOnly Multiplicity_Minimum_Value As UInteger = 2

    ' -------------------------------------------------------------------------------------------- '
    ' Constructors
    ' -------------------------------------------------------------------------------------------- '

    Public Sub New()
    End Sub

    Public Sub New(
            name As String,
            description As String,
            owner As Software_Element,
            parent_node As TreeNode,
            multiplicity As UInteger,
            base_type_ref As Guid)
        MyBase.New(name, description, owner, parent_node)
        Me.Multiplicity = multiplicity
        Me.Base_Type_Ref = base_type_ref
    End Sub


    ' -------------------------------------------------------------------------------------------- '
    ' Methods from Software_Element
    ' -------------------------------------------------------------------------------------------- '

    Protected Overrides Sub Create_Node()
        Me.Node = New TreeNode(Me.Name) With {
            .ImageKey = "Array_Type",
            .SelectedImageKey = "Array_Type",
            .ContextMenuStrip = Software_Element.Leaf_Context_Menu,
            .Tag = Me}
    End Sub

    Public Overrides Function Get_Metaclass_Name() As String
        Return Array_Type.Metaclass_Name
    End Function


    ' -------------------------------------------------------------------------------------------- '
    ' Methods for contextual menu
    ' -------------------------------------------------------------------------------------------- '

    Public Overrides Sub Edit()

        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        type_list.Remove(Me)
        Dim type_by_path_dict As Dictionary(Of String, Software_Element)
        type_by_path_dict = Software_Element.Create_Path_Dictionary_From_List(type_list)
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(type_list)

        Dim current_referenced_type_path As String = "unresolved"
        If type_by_uuid_dict.ContainsKey(Me.Base_Type_Ref) Then
            current_referenced_type_path = type_by_uuid_dict(Me.Base_Type_Ref).Get_Path()
        End If

        Dim forbidden_name_list As List(Of String)
        forbidden_name_list = Me.Owner.Get_Children_Name()
        forbidden_name_list.Remove(Me.Name)

        Dim edition_form As New Array_Type_Form(
            Element_Form.E_Form_Kind.EDITION_FORM,
            Array_Type.Metaclass_Name,
            Me.UUID.ToString,
            Me.Name,
            Me.Description,
            forbidden_name_list,
            "Base Type",
            current_referenced_type_path,
            type_by_path_dict.Keys.ToList(),
            Me.Multiplicity.ToString())
        Dim edition_form_result As DialogResult = edition_form.ShowDialog()

        ' Treat edition form result
        If edition_form_result = DialogResult.OK Then

            ' Get the type referenced by the array
            Dim new_referenced_type As Software_Element = Nothing
            new_referenced_type = type_by_path_dict(edition_form.Get_Ref_Rerenced_Element_Path())

            ' Update the array type
            Me.Name = edition_form.Get_Element_Name()
            Me.Node.Text = Me.Name
            Me.Description = edition_form.Get_Element_Description()
            Me.Multiplicity = CUInt(edition_form.Get_Multiplicity())
            Me.Base_Type_Ref = new_referenced_type.UUID

            Me.Display_Package_Modified()
        End If

    End Sub

    Public Overrides Sub View()

        ' Build the list of possible referenced type
        Dim type_list As List(Of Type) = Me.Get_Type_List_From_Project()
        Dim type_by_uuid_dict As Dictionary(Of Guid, Software_Element)
        type_by_uuid_dict = Software_Element.Create_UUID_Dictionary_From_List(type_list)

        ' Get referenced type path
        Dim referenced_type_path As String = "unresolved"
        If type_by_uuid_dict.ContainsKey(Me.Base_Type_Ref) Then
            referenced_type_path = type_by_uuid_dict(Me.Base_Type_Ref).Get_Path()
        End If

        Dim elmt_view_form As New Array_Type_Form(
            Element_Form.E_Form_Kind.VIEW_FORM,
            Array_Type.Metaclass_Name,
            Me.UUID.ToString,
            Me.Name,
            Me.Description,
            Nothing, ' Forbidden name list, useless for View
            "Base Type",
            referenced_type_path,
            Nothing, ' Useless for View
            Me.Multiplicity.ToString())
        elmt_view_form.ShowDialog()

    End Sub

End Class