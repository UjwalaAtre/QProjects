   M S B A M L   `   `   `  ���� P  LgloUIControlLibrary, Version=1.0.0.293, Culture=neutral, PublicKeyToken=nullu?clr-namespace:gloUIControlLibrary.Classes.ICD10.ICD9To10Mapping1gloUIControlLibrary.Classes.ICD10.ICD9To10Mapping  F    @gloUIControlLibrary.WPFUserControl.ICD10.ICD9To10MappingTreeView   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     U	converter?clr-namespace:gloUIControlLibrary.Classes.ICD10.ICD9To10Mapping       � 	Resources  E   ?gloUIControlLibrary.Classes.ICD10.ICD9To10Mapping.WithConverter   WithConverterH   BgloUIControlLibrary.Classes.ICD10.ICD9To10Mapping.ICD10ORConverter  ICD10OrConverter�� %   &	        &	          .��� .+�R� -    R�Name$ itemsControl�� $� IsVirtualizing .  $� VirtualizationMode$ 	Recycling=�$��Stretch=�$��Stretch=�$��Stretch=�$��Stretch=�$��0q�����  �� Path$ ScenarioList�����  �� 
TargetType" Q�.���  �� Property � ��  TreeViewItemControlTemplate �� Value# �  ��  �@Q�
IsExpanded . ��  � |� .  ����  �� ItemsSource �� *
ORNodeList+.R��� 	 �� Orientation$	 
Horizontal=�.+��� $��	Scenario ���� ���� $ 
ScenarioID�
 �� ItemTemplate
 ��  �� *	iCodeList+.R��� $	 
Horizontal=�.+��� ���� $ IsFirst� �� 	Converter# [  
 �� .R��� $	 Vertical=�.+��� $��Or��y��� $ 
IsICDFirst�# [ � .+��� �� -   
 � Tag �� $ 	ICD10code�.�.��� ���� $ ICD10CodeWithDecimal��� $��5,0,0,0q����� $ Description�