   M S B A M L   `   `   `  ����   CAGA-    'CAGA.Dialogue.ListMapLayerOptionsWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Select map layers��$	��580��$	��700�� � WindowStartupLocation$ CenterScreen=� � SizeToContent$ WidthAndHeight=� � WindowStyle$ 
ToolWindow=� � 	Resources    HoveredItemBrush �� EndPoint �� 
StartPoint � Color � Offset  OptionListTemplate	 �� DataType
 ��Name �� Padding �� CornerRadius �� Triggers �� Value �� Binding �� RelativeSource �� Mode �� AncestorType �� Path �� 
TargetName �� Value �� Property��%x  &	        &	 |     7   7   �� $ 0.5,1&�$ 0.5,0&�.��� $ Lavender��$
 0.15Q�� $ 	AliceBlue��$
 0.85Q�� $ 	CadetBlue��$ 1Q��� $	 Whatever��.R��� $
 	TheBorder��
�������$��1q�$ 10q�$ 4��$��5q�._��� .+��� -   $	��150��$��13(�$
��Bold"����� *	LayerName+���� $ 0.5,1&�$ 0.5,0&�.��� $ 	AliceBlue��$
 0.15Q�� $ White��$
 0.85Q�� $ Lavender��$ 1Q� �� $
 True�� ��  �� $ FindAncestor=�" ��$ IsMouseOver�.a��� $ 	TheBorder��8    � ���� $
 True�� �� *
IsSelected+.a��� $ 	TheBorder��8   � ��.��� $��Center=�$��Center=�$��10q� � RowDefinitions �� $
��auto ��� $	��480 � � ColumnDefinitions �� $	��200 ��� $	��480 �.+��� -   $��	HeadingTB��$��0��$��2��$��Heading��$��5q� �� Padding$ 5q�$��30(�$
��Bold"�$
��Wrap=��� -   $��OptionLB��$��1��$��0��#��[ 
������ 
������    NoFocusVisualStyle �� 
TargetType �� Color��%8  &	       '�� %     7  (�� U     * SystemColors.HighlightBrushKey+)(�� l     *SystemColors.ControlBrushKey+)(�� �     *$"SystemColors.HighlightTextBrushKey+)�� " ��.���  � ��
 ���� �� " ��.���  �@�FocusVisualStyle8   �� $ Transparent���� $ Transparent���� $ Black��� -   $��mapGrid��$��1��$��1��